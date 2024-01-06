using Microsoft.Extensions.Configuration;
using Npgsql;

namespace MonsterTradingCardsGame.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly NpgsqlConnection connection;

        public BaseRepository()
        {
            // Build configuration
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("MTCGDatabase");

            connection = new NpgsqlConnection(connectionString);
            connection.Open();
        }
    }
}
