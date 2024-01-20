using Microsoft.Extensions.Configuration;
using Npgsql;

namespace MonsterTradingCardsGame.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly NpgsqlConnection connection;

        /// <summary>
        /// This is the constructor for the BaseRepository class.
        /// It creates a connection to the database and provides it to the derived classes.
        /// </summary>
        public BaseRepository()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();

            var connectionString = configuration.GetConnectionString("postgres");

            connection = new NpgsqlConnection(connectionString);
            connection.Open();
        }
    }
}
