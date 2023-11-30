
using System.Reflection;

namespace MonsterTradingCardsGame.Server
{
    public class Router
    {
        private readonly Dictionary<string, Action<HttpServerEventArguments, Dictionary<string, string>>> _routes = new();

        public void AddRoute(string method, string path, Action<HttpServerEventArguments, Dictionary<string, string>> action)
        {
            _routes[$"{method} {path}"] = action;
        }

        public void AutoRegisterRoutes(IServiceProvider serviceProvider)
        {
            var controllerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "MonsterTradingCardsGame.Controllers" && t.IsClass && !t.IsAbstract);

            foreach (var controllerType in controllerTypes)
            {
                var controller = serviceProvider.GetService(controllerType);
                if (controller == null)
                {
                    // If controller is null, meaning it was not registered in the DI container or does not exist
                    /*throw new InvalidOperationException("Controller does not exist.");*/

                    ///  TODO: ERROR: UserCredentials inside UserController class is no controller class

                }

                var routeMethods = controllerType.GetMethods()
                    .SelectMany(method => method.GetCustomAttributes<RouteAttribute>(inherit: false)
                    .Select(attr => new { Method = method, Attribute = attr }));

                foreach (var routeMethod in routeMethods)
                {
                    try
                    {
                        var action = (Action<HttpServerEventArguments, Dictionary<string, string>>)Delegate.CreateDelegate(
                            typeof(Action<HttpServerEventArguments, Dictionary<string, string>>), controller, routeMethod.Method);

                        AddRoute(routeMethod.Attribute.Method, routeMethod.Attribute.Path, action);
                    }
                    catch (ArgumentException)
                    {
                        // TODO: Handle or log the exception for delegate creation failure
                        // This might indicate a method signature mismatch
                    }
                }
            }
        }
        public Action<HttpServerEventArguments, Dictionary<string, string>>? GetRoute(string method, string path)
        {
            foreach (var route in _routes)
            {
                var parameters = new Dictionary<string, string>();
                if (IsRouteMatch(route.Key, method, path, parameters))
                {
                    Action<HttpServerEventArguments, Dictionary<string, string>> action = (e, _) => route.Value(e, parameters);
                    return action;
                }
            }

            return null;
        }


        private bool IsRouteMatch(string pattern, string method, string path, Dictionary<string, string> parameters)
        {
            var patternParts = pattern.Split(' ');
            var methodPattern = patternParts[0];
            var pathPattern = patternParts[1];

            if (method != methodPattern)
                return false;

            var pathSegments = path.Trim('/').Split('/');
            var patternSegments = pathPattern.Trim('/').Split('/');

            if (pathSegments.Length != patternSegments.Length)
                return false;

            for (int i = 0; i < patternSegments.Length; i++)
            {
                if (patternSegments[i].StartsWith(":"))
                {
                    var paramName = patternSegments[i].Substring(1);

                    parameters[paramName] = pathSegments[i];
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
