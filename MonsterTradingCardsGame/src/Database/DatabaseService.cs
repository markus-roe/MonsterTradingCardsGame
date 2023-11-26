using Npgsql;

namespace MonsterTradingCardsGame.Database
{
    public class DatabaseService : IDisposable
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddUser(string username, string passwordHash)
        {
            // Use parameterized query to prevent SQL injection
            var query = "INSERT INTO users (username, password_hash) VALUES (@username, @passwordHash);";
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@username", username),
                new NpgsqlParameter("@passwordHash", passwordHash)
            };
            ExecuteQuery(query, parameters.ToArray());
        }

        public User GetUserByUsername(string username)
        {
            var query = "SELECT * FROM users WHERE username = @username;";
            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@username", username)
            };
            var records = ExecuteQuery(query, parameters);

            if (records.Count == 0)
            {
                return null;
            }

            return MapToUser(records[0]);
        }

        public void UpdateUserDetails(string username, string passwordHash)
        {
            var query = "UPDATE users SET password_hash = @passwordHash WHERE username = @username;";
            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@passwordHash", passwordHash),
                new NpgsqlParameter("@username", username)
            };
            ExecuteQuery(query, parameters);
        }


        private User MapToUser(Dictionary<string, object> record)
        {
            return new User
            {
                Username = record["username"].ToString(),
                Coins = Convert.ToInt32(record["coins"]),
                Elo = Convert.ToInt32(record["elo"])
            };
        }

        public List<Dictionary<string, object>> ExecuteQuery(string sql, NpgsqlParameter[]? parameters = null)
        {
            var records = new List<Dictionary<string, object>>();

            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(sql, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var record = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    record[reader.GetName(i)] = reader.GetValue(i);
                }
                records.Add(record);
            }
            return records;
        }

        public void Dispose()
        {
            /*_connection.Close();*/ // ??? 
        }
    }
}
