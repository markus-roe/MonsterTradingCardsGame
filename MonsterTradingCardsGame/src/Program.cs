using MonsterTradingCardsGame.Server;
using Microsoft.Extensions.DependencyInjection;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Middleware;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Services.Interfaces;
using MonsterTradingCardsGame.Services;
using Microsoft.Extensions.Configuration;

namespace MonsterTradingCardsGame
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = ConfigureServices();
            await InitializeDatabase(serviceProvider);
            StartHttpServer(serviceProvider);
        }

        private static async Task InitializeDatabase(ServiceProvider serviceProvider)
        {
            try
            {
                var dbInitService = serviceProvider.GetService<IDatabaseInitializationService>();
                var configuration = serviceProvider.GetService<IConfiguration>();

                var resetDB = configuration?.GetValue<bool?>("ResetDB") ?? false;
                Console.WriteLine(resetDB ? "Resetting Database" : "Initializing Database");

                if (dbInitService == null)
                    throw new InvalidOperationException("Database could not be initialized.");

                await dbInitService.InitializeOrResetDatabase(resetDB);
            }
            catch (Exception ex)
            {
                LogException("Error in database initialization", ex);
            }
        }

        private static void StartHttpServer(ServiceProvider serviceProvider)
        {
            try
            {
                var httpServer = serviceProvider.GetService<HttpServer>()
                    ?? throw new InvalidOperationException("HttpServer could not be resolved.");

                var authMiddleware = serviceProvider.GetService<AuthenticationMiddleware>()
                    ?? throw new InvalidOperationException("AuthenticationMiddleware could not be resolved.");

                httpServer.UseMiddleware(authMiddleware);
                httpServer.Incoming += ProcessMessage;
                httpServer.Run();
            }
            catch (Exception ex)
            {
                LogException("Unhandled exception", ex);
                Console.WriteLine("An error occurred. The application will now close.");
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            return new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<IAuthenticationService, AuthenticationService>()
                .AddSingleton<IBattleService, BattleService>()
                .AddSingleton<LobbyService>()
                .AddTransient<BattleController>()
                .AddTransient<CardController>()
                .AddTransient<TradingController>()
                .AddScoped<ICardRepository, CardRepository>() // Card-specific repository
                .AddScoped<IUserRepository, UserRepository>() // User-specific repository
                .AddScoped<ITradingRepository, TradingRepository>() // Trading-specific repository
                .AddScoped<ISessionRepository, SessionRepository>() // Session-specific repository
                .AddTransient<UserController>() // Transient lifecycle
                .AddSingleton<HttpServer>() // Singleton lifecycle
                .AddSingleton<AuthenticationMiddleware>()
                .AddSingleton<IDatabaseInitializationService, DatabaseInitializationService>()
                .BuildServiceProvider(); // Build service provider (DI container)
        }

        private static void ProcessMessage(object sender, HttpServerEventArguments httpEventArguments)
        {
            Console.WriteLine(httpEventArguments.PlainMessage);
            var httpServer = sender as HttpServer;
            httpServer?.HandleIncomingRequests(httpEventArguments);
        }

        private static void LogException(string message, Exception ex)
        {
            Console.WriteLine($"{message}: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
