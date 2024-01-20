using System.Reflection;

namespace MonsterTradingCardsGame.Server
{
    public class Router
    {
        private readonly Dictionary<string, Action<HttpServerEventArguments>> _routes = new();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Adds a route to the router. </summary>
        /// <param name="method">The HTTP method for the route (e.g., GET, POST).</param>
        /// <param name="path">The path of the route.</param>
        /// <param name="action">The action to be executed when the route is accessed.</param>
        public void AddRoute(string method, string path, Action<HttpServerEventArguments> action)

        {
            _routes[$"{method} {path}"] = action;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Automatically registers routes from controllers. </summary>
        public void AutoRegisterRoutes(IServiceProvider serviceProvider)
        {
            var controllerTypes = GetControllerTypes();
            foreach (var controllerType in controllerTypes)
            {
                RegisterControllerRoutes(serviceProvider, controllerType);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Retrieves a registered route action. </summary>
        /// <param name="method">The HTTP method for the route (e.g., GET, POST).</param>
        /// <param name="path">The path of the route.</param>
        /// <returns>The action associated with the route if it exists, null otherwise.</returns>
        public Action<HttpServerEventArguments>? GetRouteAction(string method, string path)
        {
            foreach (var route in _routes)
            {
                var parameters = new Dictionary<string, string>();
                if (IsRouteMatch(route.Key, method, path, ref parameters))
                {
                    Action<HttpServerEventArguments> action = (e) =>
                    {
                        e.Parameters = parameters;
                        route.Value(e);
                    };
                    return action;
                }
            }

            return null;
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Retrieves all controller types. </summary>
        /// <returns>An enumerable of controller types.</returns>
        private IEnumerable<Type> GetControllerTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => IsControllerType(t));
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Determines if a given type is a controller type. </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a controller, false otherwise.</returns>
        private bool IsControllerType(Type type)
        {
            return type.Namespace == "MonsterTradingCardsGame.Controllers" && type.IsClass && !type.IsAbstract;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Registers routes for a given controller type. </summary>
        /// <param name="serviceProvider">The service provider to resolve controller dependencies.</param>
        /// <param name="controllerType">The type of the controller for which to register routes.</param>
        private void RegisterControllerRoutes(IServiceProvider serviceProvider, Type controllerType)
        {
            var controller = serviceProvider.GetService(controllerType);
            if (controller == null)
                return;

            var routeMethods = GetRouteMethods(controllerType);
            foreach (var (method, attribute) in routeMethods)
            {
                try
                {
                    var action = (Action<HttpServerEventArguments>)Delegate.CreateDelegate(typeof(Action<HttpServerEventArguments>), controller, method);
                    AddRoute(attribute.Method, attribute.Path, action);
                }
                catch (Exception)
                {
                    // handle exceptions
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Retrieves route methods from a controller type. </summary>
        /// <param name="controllerType">The controller type to examine.</param>
        /// <returns>An enumerable of method and attribute pairs representing the routes.</returns>
        private IEnumerable<(MethodInfo Method, RouteAttribute Attribute)> GetRouteMethods(Type controllerType)
        {
            return controllerType.GetMethods()
                .SelectMany(method => method.GetCustomAttributes<RouteAttribute>(inherit: false)
                .Select(attr => (Method: method, Attribute: attr)));
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Determines if a given route matches a request. </summary>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="path">The path of the request.</param>
        /// <param name="parameters">A dictionary to store extracted parameters.</param>
        /// <returns>True if the route matches the request, false otherwise.</returns>
        private bool IsRouteMatch(string pattern, string method, string path, ref Dictionary<string, string> parameters)
        {
            var patternParts = pattern.Split(' ');
            if (method != patternParts[0])
                return false;

            return IsPathMatch(path, patternParts[1], parameters);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Determines if a path matches a given path pattern. </summary>
        /// <param name="path">The request path.</param>
        /// <param name="pathPattern">The pattern to match against.</param>
        /// <param name="parameters">A dictionary to store extracted parameters.</param>
        /// <returns>True if the path matches the pattern, false otherwise.</returns>
        private bool IsPathMatch(string path, string pathPattern, Dictionary<string, string> parameters)
        {
            var pathSegments = path.Trim('/').Split('/');
            var patternSegments = pathPattern.Trim('/').Split('/');

            if (pathSegments.Length != patternSegments.Length)
                return false;

            for (int i = 0; i < patternSegments.Length; i++)
            {
                if (patternSegments[i].StartsWith(":"))
                {
                    parameters[patternSegments[i].Substring(1)] = pathSegments[i];
                }
                else if (patternSegments[i] != pathSegments[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
