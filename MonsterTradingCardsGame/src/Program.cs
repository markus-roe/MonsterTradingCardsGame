using MonsterTradingCardsGame.Server;
using Microsoft.Extensions.DependencyInjection;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Data;

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

            // Configure DI container
            var serviceProvider = new ServiceCollection()
                .AddScoped<UserRepository>()
                .AddTransient<IRepository<User>, UserRepository>()
                .AddScoped<IUnitOfWork, UnitOfWork>()  // Register UnitOfWork
                .AddTransient<UserController>() // Transient lifecycle
                .AddSingleton<HttpServer>() // Singleton lifecycle
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
