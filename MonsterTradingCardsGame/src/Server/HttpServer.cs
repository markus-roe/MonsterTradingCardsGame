using System.Net;
using System.Net.Sockets;
using System.Text;
using MonsterTradingCardsGame.Middleware;

namespace MonsterTradingCardsGame.Server
{
    /// <summary>This class implements a HTTP server.</summary>
    public sealed class HttpServer
    {

        /// <summary>TCP listener.</summary>
        private TcpListener? _Listener;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<IMiddleware> _middlewares = new List<IMiddleware>();
        private readonly Router _routeHandler = new Router();

        /// <summary>Occurs when a HTTP message has been received.</summary>
        public event IncomingEventHandler? Incoming;

        public HttpServer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
           _routeHandler.AutoRegisterRoutes(serviceProvider);

        }

        public void UseMiddleware(IMiddleware middleware)
        {
            _middlewares.Add(middleware);
        }



        public void HandleIncomingRequests(HttpServerEventArguments httpEventArguments)
        {
            try
            {

                foreach (var middleware in _middlewares)
                {
                    middleware.Invoke(httpEventArguments);

                    // Check if the response has been set by the middleware
                    // If so, stop further processing
                    if (httpEventArguments.ResponseSent)
                    {
                        return;
                    }
                }

                var routeHandler = _routeHandler.GetRoute(httpEventArguments.Method, httpEventArguments.Path);
                if (routeHandler != null)
                {
                    var parameters = new Dictionary<string, string>();
                    routeHandler(httpEventArguments, parameters);

                }
                else
                {
                    httpEventArguments.Reply(404, "Not Found");
                }
            }
            catch (Exception ex)
            {
                HandleException(httpEventArguments, ex);
            }
        }


        private void HandleException(HttpServerEventArguments httpEventArguments, Exception ex)
        {
            // Centralized exception handling
            httpEventArguments.Reply(500, $"Internal Server Error: {ex.Message}");
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
 /*         public void Run()
          {
              if (Active) return;

              Active = true;
              _Listener = new(IPAddress.Parse("127.0.0.1"), 10001);
              _Listener.Start();
              Console.WriteLine("Listening on 127.0.0.1:10001");


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
          }*/

        public void Run()
        {
            if (Active) return;

            Active = true;
            _Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 10001);
            _Listener.Start();
            Console.WriteLine("Server is running on 127.0.0.1:10001");

            Task.Run(async () =>
            {
                try
                {
                    while (Active)
                    {
                        TcpClient client = await _Listener.AcceptTcpClientAsync();
                        Console.WriteLine("Accepted new client.");
                        await Task.Run(() =>
                            {
                                byte[] buf = new byte[256];
                            string data = string.Empty;

                            while (client.GetStream().DataAvailable || string.IsNullOrEmpty(data))
                            {
                                int n = client.GetStream().Read(buf, 0, buf.Length);
                                    data += Encoding.ASCII.GetString(buf, 0, n);
                                }

                            Incoming?.Invoke(this, new HttpServerEventArguments(client, data));

                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in server main loop: {ex.Message}");
                    Active = false;
                }
            });


            Console.ReadLine(); // Keep the main thread active
            _Listener.Stop();
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                byte[] buf = new byte[256];
                string data = string.Empty;

                while (client.GetStream().DataAvailable || string.IsNullOrEmpty(data))
                {
                    int n = client.GetStream().Read(buf, 0, buf.Length);
                    data += Encoding.ASCII.GetString(buf, 0, n);
                }

                Incoming?.Invoke(this, new HttpServerEventArguments(client, data));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }



        /// <summary>Stops the HTTP server.</summary>
        public void Stop()
        {
            Active = false;
        }
    }


}
