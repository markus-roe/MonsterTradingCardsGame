using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using Npgsql;

namespace MonsterTradingCardsGame.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly IUnitOfWork unitOfWork;

        public UserRepository(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public List<User>? GetAll()
        {
            var users = new List<User>();

            // Assuming unitOfWork exposes the NpgsqlConnection and NpgsqlTransaction
            var command = new NpgsqlCommand("SELECT * FROM Users", unitOfWork.Connection, unitOfWork.Transaction);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                    });
                }
            }

            return users;
        }

        public User? GetUserByUsername(string username)
        {
            var command = new NpgsqlCommand("SELECT * FROM users WHERE username = @username", unitOfWork.Connection, unitOfWork.Transaction);
            command.Parameters.AddWithValue("@username", username);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new User
                    {
                        Username = reader.GetString(reader.GetOrdinal("username")),
                    };
                }
            }

            return null;
        }

        public bool Delete(User obj)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public User? GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public User? Add(User obj)
        {
            throw new NotImplementedException();
        }

        public User? Update(User obj)
        {
            throw new NotImplementedException();
        }
    }
}
