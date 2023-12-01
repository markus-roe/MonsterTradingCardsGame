using System.Data;
using Npgsql;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Interfaces;

namespace MonsterTradingCardsGame.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository() : base() { }

        protected override void Fill(User user, IDataRecord record)
        {
            user.Username = record.GetString(record.GetOrdinal("Username"));
            // Populate other User properties from the record as needed
        }

        public override List<User> GetAll()
        {
            var users = new List<User>();
            using (var command = new NpgsqlCommand("SELECT * FROM Users", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var user = new User();
                    Fill(user, reader);
                    users.Add(user);
                }
            }
            return users;
        }

        public User? GetUserByUsername(string username)
        {
            User? user = null;
            using (var command = new NpgsqlCommand("SELECT * FROM Users WHERE Username = @username", connection))
            {
                command.Parameters.AddWithValue("@username", username);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User();
                        Fill(user, reader);
                    }
                }
            }
            return user;
        }

        public override void Save(User user)
        {
            using (var command = new NpgsqlCommand("INSERT INTO Users (Username, ...) VALUES (@username, ...) ON CONFLICT (Id) DO UPDATE SET Username = @username, ...", connection))
            {
                command.Parameters.AddWithValue("@username", user.Username);
                // Add other parameters as needed

                command.ExecuteNonQuery();
            }
        }

        public override void Delete(User user)
        {
            using (var command = new NpgsqlCommand("DELETE FROM Users WHERE Username = @Username", connection))
            {
                command.Parameters.AddWithValue("@Username", user.Username);

                command.ExecuteNonQuery();
            }
        }

    }
}
