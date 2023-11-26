using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using MonsterTradingCardsGame.Controllers;


namespace MonsterTradingCardsGame.Server
{
    /// <summary>This class implements a HTTP server.</summary>
    public sealed class HttpServer
    {

        /// <summary>TCP listener.</summary>
        private TcpListener? _Listener;
        private readonly IServiceProvider _serviceProvider;
        private readonly Router _routeHandler = new Router();

        public HttpServer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeRoutes();

        }

        /// <summary>Occurs when a HTTP message has been received.</summary>
        public event IncomingEventHandler? Incoming;

        private void InitializeRoutes()
        {
            var userController = _serviceProvider.GetService<UserController>();
            if (userController != null)
            {
                _routeHandler.AddRoute("POST", "/users", userController.RegisterUser);
                _routeHandler.AddRoute("GET", "/users/:username", userController.GetUser);
                _routeHandler.AddRoute("PUT", "/users/:username", userController.EditUser);
            }
        }

        public void HandleIncomingRequests(HttpServerEventArguments e)
        {
            try
            {
                var routeHandler = _routeHandler.GetRoute(e.Method, e.Path);
                if (routeHandler != null)
                {
                    var parameters = new Dictionary<string, string>();
                    routeHandler(e, parameters);

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


        private void HandleException(HttpServerEventArguments e, Exception ex)
        {
            // Centralized exception handling
            e.Reply(500, $"Internal Server Error: {ex.Message}");
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
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
