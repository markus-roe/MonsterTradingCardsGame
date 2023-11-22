using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using MonsterTradingCardsGame.Controllers;
using BaseCtrl = MonsterTradingCardsGame.Controllers.BaseController;


namespace MonsterTradingCardsGame.Server
{
    /// <summary>This class implements a HTTP server.</summary>
    public sealed class HttpServer
    {

        /// <summary>TCP listener.</summary>
        private TcpListener? _Listener;
        private readonly IServiceProvider _serviceProvider;



        public HttpServer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>Occurs when a HTTP message has been received.</summary>
        public event IncomingEventHandler? Incoming;


        public void HandleIncomingRequests(HttpServerEventArguments e)
        {
            try
            {
                var routeHandler = GetRouteHandler(e.Method, e.Path);
                if (routeHandler != null)
                {
                    routeHandler(e);
                }
                else
                {
                    e.Reply(404, "Not Found");
                }
            }
            catch (Exception ex)
            {
                HandleException(e, ex);
            }
        }

        private Action<HttpServerEventArguments>? GetRouteHandler(string method, string path)
        {
            var routePatterns = new Dictionary<string, Action<HttpServerEventArguments, Dictionary<string, string>>>
            {
                // Users
                { "POST /users", RegisterUser },
                { "GET /users/:username", GetUser },
/*              { "PUT /users/:username", UpdateUser },
                { "POST /sessions", LoginUser },

                // Package management
                { "POST /packages", CreatePackage },
                { "POST /transactions/packages", BuyPackage },

                // Card management
                { "GET /cards", ShowUserCards },
                { "GET /deck", ShowUserDeck },
                { "PUT /deck", ConfigureDeck },

                // Game management
                { "GET /stats", GetStats },
                { "GET /scoreboard", GetScoreboard },
                { "POST /battles", StartBattle },

                // Trading
                { "GET /tradings", GetTradingDeals },
                { "POST /tradings", CreateTradingDeal },
                { "POST /tradings/:tradingdealid", CarryOutTrade },
                { "DELETE /tradings/:tradingdealid", DeleteTradingDeal }*/
            };

            foreach (var routePattern in routePatterns.Keys)
            {
                var parameters = new Dictionary<string, string>();
                if (IsRouteMatch(routePattern, method, path, parameters))
                {
                    return (e) => routePatterns[routePattern](e, parameters);
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

        private void GetUser(HttpServerEventArguments e, Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("username", out var username))
            {
                var userController = _serviceProvider.GetService<Controllers.UserController>();
                var user = userController?.GetUser(username);

                if (user == null)
                {
                    e.Reply(404, "User not found.");
                }
                else
                {
                    var response = JsonSerializer.Serialize(user);
                    e.Reply(200, response);
                }
            }
            else
            {
                e.Reply(400, "Bad Request: Username parameter is missing.");
            }
        }

        private void RegisterUser(HttpServerEventArguments e, Dictionary<string, string> parameters)
        {
            // Assuming the user credentials are sent in the body of the POST request
            var userCredentials = JsonSerializer.Deserialize<BaseCtrl.UserCredentials>(e.Payload);

            if (userCredentials == null)
            {
                e.Reply(400, "Invalid user credentials.");
                return;
            }

            var userController = _serviceProvider.GetService<Controllers.UserController>();
            var response = userController?.RegisterUser(userCredentials);

            switch (response)
            {
                case UserController.Response.Success:
                    e.Reply(200, "User successfully registered.");
                    break;
                case UserController.Response.UsernameAlreadyExists:
                    e.Reply(404, "Username already exists.");
                    break;
                default:
                    e.Reply(400, "Invalid request.");
                    break;
            }
        }





        private void HandleException(HttpServerEventArguments e, Exception ex)
        {
            // Centralized exception handling
            e.Reply(500, $"Internal Server Error: {ex.Message}");
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public peoperties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets or sets if the server is active.</summary>
        public bool Active { get; set; } = false;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Runs the HTTP server.</summary>
        public void Run()
        {
            if (Active) return;

            Active = true;
            _Listener = new(IPAddress.Parse("127.0.0.1"), 10001);
            _Listener.Start();
            Console.WriteLine("Running!");


            byte[] buf = new byte[256];

            while (Active)
            {
                TcpClient client = _Listener.AcceptTcpClient();

                string data = string.Empty;
                while (client.GetStream().DataAvailable || string.IsNullOrEmpty(data))
                {
                    int n = client.GetStream().Read(buf, 0, buf.Length);
                    data += Encoding.ASCII.GetString(buf, 0, n);
                }

                Incoming?.Invoke(this, new HttpServerEventArguments(client, data));
            }

            _Listener.Stop();
        }


        /// <summary>Stops the HTTP server.</summary>
        public void Stop()
        {
            Active = false;
        }
    }


}
