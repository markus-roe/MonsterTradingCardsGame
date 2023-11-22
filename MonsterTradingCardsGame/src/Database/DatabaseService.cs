using System;
using System.Data.Common;
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
