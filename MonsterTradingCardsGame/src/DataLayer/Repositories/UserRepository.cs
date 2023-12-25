using System.Data;
using Npgsql;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Utils.UserStats;
using System.Text.Json;

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
                Console.WriteLine($"An error occurred while retrieving user by username: {ex.Message}\n {ex.StackTrace}");
                return null;
            }
        }

        public bool AddWin(User user)
        {
            try
            {
                using (var command = new NpgsqlCommand("UPDATE userstats SET wins = wins + 1, elo = elo + 3 WHERE userid = @userid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);

                    int rowsAffected = command.ExecuteNonQuery();
                    bool rowUpdated = rowsAffected > 0;
                    return rowUpdated;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating user: {ex.Message}");
                return false;
            }
        }

        public bool AddLoss(User user)
        {
            try
            {
                using (var command = new NpgsqlCommand("UPDATE userstats SET losses = losses + 1, elo = elo -5 WHERE userid = @userid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);

                    int rowsAffected = command.ExecuteNonQuery();
                    bool rowUpdated = rowsAffected > 0;
                    return rowUpdated;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating user: {ex.Message}");
                return false;
            }
        }

        public User? GetUserById(int id)
        {
            User? user = null;
            try
            {
                using (var command = new NpgsqlCommand("SELECT * FROM Users WHERE Id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);

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
                Console.WriteLine($"An error occurred while retrieving user by id: {ex.Message}\n {ex.StackTrace}");
                return null;
            }
        }



        public bool SetCardDeck(User user, List<Card> cards)
        {
            try
            {
                int totalRowsUpdated = 0;
                foreach (Card card in cards)
                {
                    using (var command = new NpgsqlCommand("UPDATE user_cards SET indeck = 'true' WHERE userid = @userid AND cardid = @cardid", connection))
                    {
                        command.Parameters.AddWithValue("@userid", user.Id);
                        command.Parameters.AddWithValue("@cardid", card.Id);
                        int rowsUpdated = command.ExecuteNonQuery();
                        totalRowsUpdated += rowsUpdated;
                    }
                }

                return totalRowsUpdated == cards.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SetCardDeck: " + ex.Message);
                return false;
            }
        }

        public string? GetStatsByUser(User user)
        {
            try
            {
                using (var command = new NpgsqlCommand("SELECT * FROM user_statsview WHERE userid = @userid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var stats = new
                            {
                                Name = reader["name"].ToString(),
                                Elo = reader["elo"].ToString(),
                                Wins = reader["wins"].ToString(),
                                Losses = reader["losses"].ToString()
                            };
                            return JsonSerializer.Serialize(stats);
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetStatsByUser: " + ex.Message);
                return null;
            }
        }



        public List<UserStats>? GetScoreboard()
        {
            try
            {
                var statsList = new List<UserStats>();

                using (var command = new NpgsqlCommand("SELECT * FROM user_statsview ORDER BY elo DESC", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var stats = new UserStats
                            {
                                Name = reader["name"]?.ToString() ?? string.Empty,
                                Elo = (int)reader["elo"],
                                Wins = (int)reader["wins"],
                                Losses = (int)reader["losses"]
                            };

                            statsList.Add(stats);
                        }
                    }
                }

                return statsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetScoreboard: " + ex.Message);
                return null;
            }
        }



        public override bool Update(User user)
        {
            try
            {
                using (var command = new NpgsqlCommand("UPDATE Users SET Name = @name, Bio = @bio, Image = @image, Elo = @elo, Coins = @coins WHERE Username = @username", connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@name", user.Name ?? string.Empty);
                    command.Parameters.AddWithValue("@bio", user.Bio ?? string.Empty);
                    command.Parameters.AddWithValue("@image", user.Image ?? string.Empty);
                    command.Parameters.AddWithValue("@elo", user.Elo);
                    command.Parameters.AddWithValue("@coins", user.Coins);

                    int rowsAffected = command.ExecuteNonQuery();
                    bool rowUpdated = rowsAffected > 0;
                    return rowUpdated;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here, e.g. log the error or throw a custom exception
                Console.WriteLine($"An error occurred while updating user: {ex.Message}");
                return false;
            }
        }


        public override bool Save(User user)
        {
            try
            {
                using (var command = new NpgsqlCommand("INSERT INTO Users (Username, Password_Hash) VALUES (@username, @password) RETURNING Id", connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@password", user.Password);

                    object result = command.ExecuteScalar();
                    int? userId = result as int?;

                    if (userId.HasValue)
                    {
                        using (var statsCommand = new NpgsqlCommand("INSERT INTO UserStats (userid, wins, losses) VALUES (@userid, 0, 0)", connection))
                        {
                            statsCommand.Parameters.AddWithValue("@userid", userId.Value);

                            statsCommand.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // Handle the exception here, e.g. log the error or throw a custom exception
                Console.WriteLine($"An error occurred while saving user: {ex.Message}");
                return false;
            }
        }

        public override void Delete(User user)
        {
            using (var command = new NpgsqlCommand("DELETE FROM Users WHERE id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Username", user.Id);

                command.ExecuteNonQuery();
            }
        }

    }
}
