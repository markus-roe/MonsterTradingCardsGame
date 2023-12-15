using System.Reflection;

namespace MonsterTradingCardsGame.Server
{
    public class Router
    {
        private readonly Dictionary<string, Action<HttpServerEventArguments, Dictionary<string, string>>> _routes = new();

        // Add a route to the router
        public void AddRoute(string method, string path, Action<HttpServerEventArguments, Dictionary<string, string>> action)
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
        public Action<HttpServerEventArguments, Dictionary<string, string>>? GetRoute(string method, string path)
        {
            foreach (var route in _routes)
            {
                var parameters = new Dictionary<string, string>();
                if (IsRouteMatch(route.Key, method, path, out parameters))
                {
                    Action<HttpServerEventArguments, Dictionary<string, string>> action = RouteAction;
                    return action;
                }
            }

            return null;
        }

        private void RouteAction(HttpServerEventArguments e, Dictionary<string, string> parameters)
        {
            _routes[e.Method + " " + e.Path](e, parameters);
        }

        // Helper methods
        private IEnumerable<Type> GetControllerTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => IsControllerType(t));
        }

        private bool IsControllerType(Type type)
        {
            return type.Namespace == "MonsterTradingCardsGame.Controllers" && type.IsClass && !type.IsAbstract && type.Name != "UserCredentials" && type.Name != "UserProfileInfo";
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
                    var action = (Action<HttpServerEventArguments, Dictionary<string, string>>)Delegate.CreateDelegate(
                        typeof(Action<HttpServerEventArguments, Dictionary<string, string>>), controller, method);
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

        private bool IsRouteMatch(string pattern, string method, string path, out Dictionary<string, string> parameters)
        {
            parameters = new Dictionary<string, string>();
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
