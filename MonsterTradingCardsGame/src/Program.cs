using MonsterTradingCardsGame.Server;
using Microsoft.Extensions.DependencyInjection;
using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Controllers;

namespace MonsterTradingCardsGame
{
    internal class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // entry point                                                                                                      //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Main entry point.</summary>
        /// <param name="args">Arguments.</param>
        static void Main(string[] args)
        {
            var connectionString = "Server=127.0.0.1;Port=5432;Database=MTCG;User Id=postgres;Password=mtcgpw;";

            // Configure DI container
            var serviceProvider = new ServiceCollection()
                .AddTransient<UserController>() // Transient lifecycle
                .AddSingleton<HttpServer>() // Singleton lifecycle
                .AddTransient(sp => new DatabaseService(connectionString)) // Scoped lifecycle
                .BuildServiceProvider(); // Build service provider (DI container)

            var httpServer = serviceProvider.GetService<HttpServer>(); // Resolve HttpServer
            if (httpServer == null)
            {
                throw new InvalidOperationException("HttpServer could not be resolved.");
            }

            httpServer.Incoming += _ProcessMessage;
            httpServer.Run();

        }


        /// <summary>Event handler for incoming server requests.</summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private static void _ProcessMessage(object sender, HttpServerEventArguments e)
        {
            // Existing code to log the incoming message
            Console.WriteLine(e.PlainMessage);

            // Call the HandleIncomingRequests method to process specific requests
            var httpServer = sender as HttpServer;
            httpServer?.HandleIncomingRequests(e);

 /*           e.Reply(200, "Understood!");*/
        }

    }
}
