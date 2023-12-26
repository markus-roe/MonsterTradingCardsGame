using System.Reflection;

namespace MonsterTradingCardsGame.Server
{
    public class Router
    {
        private readonly Dictionary<string, Action<HttpServerEventArguments>> _routes = new();

        // Add a route to the router
        public void AddRoute(string method, string path, Action<HttpServerEventArguments> action)
        {
            _routes[$"{method} {path}"] = action;
        }

        // Automatically register routes from controllers
        public void AutoRegisterRoutes(IServiceProvider serviceProvider)
        {
            var controllerTypes = GetControllerTypes();
            foreach (var controllerType in controllerTypes)
            {
                RegisterControllerRoutes(serviceProvider, controllerType);
            }
        }

        // Get a registered route action
        // Get a registered route action
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


        // Helper methods
        private IEnumerable<Type> GetControllerTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => IsControllerType(t));
        }

        private bool IsControllerType(Type type)
        {
            return type.Namespace == "MonsterTradingCardsGame.Controllers" && type.IsClass && !type.IsAbstract && type.Name != "UserCredentials" && type.Name != "UserProfileInfo" && type.Name != "UserStats";
        }

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

        private IEnumerable<(MethodInfo Method, RouteAttribute Attribute)> GetRouteMethods(Type controllerType)
        {
            return controllerType.GetMethods()
                .SelectMany(method => method.GetCustomAttributes<RouteAttribute>(inherit: false)
                .Select(attr => (Method: method, Attribute: attr)));
        }

        private bool IsRouteMatch(string pattern, string method, string path, ref Dictionary<string, string> parameters)
        {
            var patternParts = pattern.Split(' ');
            if (method != patternParts[0])
                return false;

            return IsPathMatch(path, patternParts[1], parameters);
        }

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
