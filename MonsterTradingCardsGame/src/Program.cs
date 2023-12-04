    using MonsterTradingCardsGame.Server;
    using Microsoft.Extensions.DependencyInjection;
    using MonsterTradingCardsGame.Controllers;
    using MonsterTradingCardsGame.Middleware;
    using MonsterTradingCardsGame.Interfaces;
    using MonsterTradingCardsGame.Repositories;
    using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;
using MonsterTradingCardsGame.Services;

    namespace MonsterTradingCardsGame
    {
        internal class Program
        {

            static void Main(string[] args)
            {
                try
                {
                    // Configure Dependency Injection container
                    var serviceProvider = ConfigureServices();

                    // Resolve HttpServer
                    var httpServer = serviceProvider.GetService<HttpServer>();
                    if (httpServer == null)
                    {
                        throw new InvalidOperationException("HttpServer could not be resolved.");
                    }

                    var authMiddleware = serviceProvider.GetService<AuthenticationMiddleware>();
                    if (authMiddleware != null)
                    {
                        httpServer.UseMiddleware(authMiddleware);
                    }
                    else
                    {
                        throw new InvalidOperationException("AuthenticationMiddleware could not be resolved.");
                    }

                    httpServer.Incoming += ProcessMessage;
                    httpServer.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unhandled Exception: {ex.Message}\n{ex.StackTrace}");


                    Console.WriteLine("An error occurred. The application will now close.");
                }
            }

            /// <summary>Configures services and returns the service provider for dependency injection.</summary>
            /// <returns>Configured service provider.</returns>
            private static ServiceProvider ConfigureServices()
            {
                return new ServiceCollection()
                    .AddSingleton<IAuthenticationService, AuthenticationService>()
                    .AddScoped<IUserRepository, UserRepository>() // User-specific repository
                    .AddScoped<IRepository<User>, UserRepository>() // General repository for User
                    .AddTransient<UserController>() // Transient lifecycle
                    .AddSingleton<HttpServer>() // Singleton lifecycle
                    .AddSingleton<AuthenticationMiddleware>() 
                    .BuildServiceProvider(); // Build service provider (DI container)
            }

            /// <summary>Event handler for incoming server requests.</summary>
            /// <param name="sender">Sender.</param>
            /// <param name="httpEventArguments">Event arguments.</param>
            private static void ProcessMessage(object sender, HttpServerEventArguments httpEventArguments)
            {
                Console.WriteLine(httpEventArguments.PlainMessage);

                // Call the HandleIncomingRequests method to process specific requests
                var httpServer = sender as HttpServer;
                httpServer?.HandleIncomingRequests(httpEventArguments);
            }
        }
    }
