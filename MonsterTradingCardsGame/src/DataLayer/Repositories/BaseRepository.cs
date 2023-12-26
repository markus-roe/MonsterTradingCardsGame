using Npgsql;

namespace MonsterTradingCardsGame.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly NpgsqlConnection connection;

        public BaseRepository()
        {
            var connectionString = "Server=127.0.0.1;Port=5432;Database=MTCG;User Id=postgres;Password=mtcgpw;";
            connection = new NpgsqlConnection(connectionString);
            connection.Open();
        }

    }
}
