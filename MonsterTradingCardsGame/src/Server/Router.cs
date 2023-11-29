
namespace MonsterTradingCardsGame.Server
{
    public class Router
    {
        private readonly Dictionary<string, Action<HttpServerEventArguments, Dictionary<string, string>>> _routes = new();

        public void AddRoute(string method, string path, Action<HttpServerEventArguments, Dictionary<string, string>> action)
        {
            _routes[$"{method} {path}"] = action;
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
