using System.Data;
using Npgsql;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Interfaces;

namespace MonsterTradingCardsGame.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly ICardRepository _cardRepository;

        public UserRepository(ICardRepository cardRepository) : base()
        {
            _cardRepository = cardRepository;
        }

        protected override void Fill(User user, IDataRecord record)
        {
            user.Id = record.GetInt32(record.GetOrdinal("id"));
            user.Username = record.GetString(record.GetOrdinal("Username"));
            user.Password = record.GetString(record.GetOrdinal("password_hash"));
            user.Coins = record.GetInt32(record.GetOrdinal("Coins"));
            user.Elo = record.GetInt32(record.GetOrdinal("Elo"));
            user.Name = record.IsDBNull(record.GetOrdinal("Name")) ? null : record["Name"].ToString();
            user.Bio = record.IsDBNull(record.GetOrdinal("Bio")) ? null : record["Bio"].ToString();
            user.Image = record.IsDBNull(record.GetOrdinal("Image")) ? null : record["Image"].ToString();
        }

        public override List<User> GetAll()
        {
            var users = new List<User>();
            try
            {
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
            }
            catch (Exception ex)
            {
                // Handle the exception here, e.g. log the error or throw a custom exception
                Console.WriteLine($"An error occurred while retrieving users: {ex.Message}");
            }
            return users;
        }

        public User? GetUserByUsername(string username)
        {
            User? user = null;
            try
            {
                using (var command = new NpgsqlCommand("SELECT * FROM Users WHERE Username = @username", connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User();
                            Fill(user, reader);
                            user.Stack = _cardRepository.GetCardsByUser(user);
                            user.Deck = _cardRepository.GetDeckByUser(user);
                        }
                    }
                    return user;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here, e.g. log the error or throw a custom exception
                Console.WriteLine($"An error occurred while retrieving user by username: {ex.Message}\n {ex.StackTrace}");
                return null;
            }
        }



        public bool SetCardDeck(User user, List<Card> cards)
        {
            try
            {
                using (var command = new NpgsqlCommand("DELETE FROM user_cards WHERE userid = @userid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.ExecuteNonQuery();
                }

                foreach (Card card in cards)
                {
                    using (var command = new NpgsqlCommand("INSERT INTO user_cards (userid, cardid) VALUES (@userid, @cardid)", connection))
                    {
                        command.Parameters.AddWithValue("@userid", user.Id);
                        command.Parameters.AddWithValue("@cardid", card.Id);
                        command.ExecuteNonQuery();
                    }
                }
                user.Deck = cards.ToList();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SetCardDeck: " + ex.Message);
                return false;
            }
        }

        public override void Update(User user)
        {
            try
            {
                using (var command = new NpgsqlCommand("UPDATE Users SET Name = @name, Bio = @bio, Image = @image, Elo = @elo, Coins = @coins WHERE Username = @username", connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@name", user.Name ?? string.Empty);
                    command.Parameters.AddWithValue("@bio", user.Bio);
                    command.Parameters.AddWithValue("@image", user.Image);
                    command.Parameters.AddWithValue("@elo", user.Elo);
                    command.Parameters.AddWithValue("@coins", user.Coins);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here, e.g. log the error or throw a custom exception
                Console.WriteLine($"An error occurred while updating user: {ex.Message}");
            }
        }


        public override void Save(User user)
        {
            try
            {
                using (var command = new NpgsqlCommand("INSERT INTO Users (Username, Password_Hash) VALUES (@username, @password)", connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@password", user.Password);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here, e.g. log the error or throw a custom exception
                Console.WriteLine($"An error occurred while saving user: {ex.Message}");
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
