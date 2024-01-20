using System.Data;
using Npgsql;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Utils.UserStats;
using System.Text.Json;

namespace MonsterTradingCardsGame.Repositories
{

    public class UserRepository : BaseRepository, IUserRepository
    {
        private readonly ICardRepository _cardRepository;

        public UserRepository(ICardRepository cardRepository) : base()
        {
            _cardRepository = cardRepository;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Fills a User object with data from a database record. </summary>
        /// <param name="user">The User object to fill.</param>
        /// <param name="record">The data record containing user information.</param>
        protected void Fill(User user, IDataRecord record)
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Retrieves all users from the database. </summary>
        /// <returns>A list of all users.</returns>
        public List<User> GetAll()
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
                Console.WriteLine($"An error occurred while retrieving users: {ex.Message}");
            }
            return users;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Retrieves a user by username. </summary>
        /// <param name="username">The username of the user to retrieve.</param>
        /// <returns>The User object if found, null otherwise.</returns>
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Increments the win count of a user and updates their ELO score. </summary>
        /// <param name="user">The user to update.</param>
        /// <returns>True if the update is successful, false otherwise.</returns>
        public bool AddWin(User user)
        {
            try
            {
                using (var command = new NpgsqlCommand("UPDATE userstats SET wins = wins + 1 WHERE userid = @userid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);

                    int rowsAffected = command.ExecuteNonQuery();
                    bool rowUpdated = rowsAffected > 0;
                    if (rowUpdated)
                    {
                        using (var command2 = new NpgsqlCommand("UPDATE users SET elo = elo + 3 WHERE id = @userid", connection))
                        {
                            command2.Parameters.AddWithValue("@userid", user.Id);
                            command2.ExecuteNonQuery();
                        }
                    }
                    return rowUpdated;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating user: {ex.Message}");
                return false;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Increments the loss count of a user and updates their ELO score. </summary>
        /// <param name="user">The user to update.</param>
        /// <returns>True if the update is successful, false otherwise.</returns>
        public bool AddLoss(User user)
        {
            try
            {
                using (var command = new NpgsqlCommand("UPDATE userstats SET losses = losses + 1 WHERE userid = @userid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);

                    int rowsAffected = command.ExecuteNonQuery();
                    bool rowUpdated = rowsAffected > 0;
                    if (rowUpdated)
                    {
                        using (var command2 = new NpgsqlCommand("UPDATE users SET elo = GREATEST(elo - 5, 0) WHERE id = @userid", connection))
                        {
                            command2.Parameters.AddWithValue("@userid", user.Id);
                            command2.ExecuteNonQuery();
                        }
                    }
                    return rowUpdated;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating user: {ex.Message}");
                return false;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Updates the ELO rating of a user. </summary>
        /// <param name="user">The user whose ELO rating is to be updated.</param>
        /// <param name="eloChange">The amount to change the ELO rating by.</param>
        public void UpdateElo(User user, int eloChange)
        {
            try
            {
                using (var command = new NpgsqlCommand("UPDATE users SET elo = GREATEST(elo + @eloChange, 0) WHERE id = @userid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@eloChange", eloChange);
                    command.ExecuteNonQuery();
                }
                user.Elo = Math.Max(user.Elo + eloChange, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating user ELO: {ex.Message}");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Retrieves a user by their ID. </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>The User object if found, null otherwise.</returns>
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



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Sets the deck for a user. </summary>
        /// <param name="user">The user whose deck is to be set.</param>
        /// <param name="cards">The list of cards to be set as the deck.</param>
        /// <returns>True if the deck is successfully set, false otherwise.</returns>
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Retrieves statistics for a user. </summary>
        /// <param name="user">The user whose statistics are to be retrieved.</param>
        /// <returns>The UserStats object if found, null otherwise.</returns>
        public UserStats? GetStatsByUser(User user)
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
                            var stats = new UserStats
                            {
                                Name = reader["name"].ToString() ?? string.Empty,
                                Elo = (int)reader["elo"],
                                Wins = (int)reader["wins"],
                                Losses = (int)reader["losses"],
                                Winratio = (double)reader["winratio"]
                            };
                            return stats;
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Retrieves the scoreboard with user statistics. </summary>
        /// <returns>A list of UserStats objects representing the scoreboard.</returns>
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
                                Losses = (int)reader["losses"],
                                Winratio = (double)reader["winratio"]
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Updates a user's profile information. </summary>
        /// <param name="user">The user whose information is to be updated.</param>
        /// <returns>True if the update is successful, false otherwise.</returns>
        public bool UpdateUser(User user)
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
                Console.WriteLine($"An error occurred while updating user: {ex.Message}");
                return false;
            }
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Saves a new user to the database. </summary>
        /// <param name="user">The user to save.</param>
        /// <returns>The ID of the saved user, null if the operation fails.</returns>
        public int? SaveUser(User user)
        {
            try
            {
                int? userId = null;

                using (var command = new NpgsqlCommand("INSERT INTO Users (Username, Password_Hash) VALUES (@username, @password) RETURNING Id", connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@password", user.Password);

                    object result = command.ExecuteScalar();
                    userId = result as int?;

                    if (userId.HasValue)
                    {
                        using (var statsCommand = new NpgsqlCommand("INSERT INTO UserStats (userid, wins, losses) VALUES (@userid, 0, 0)", connection))
                        {
                            statsCommand.Parameters.AddWithValue("@userid", userId.Value);

                            statsCommand.ExecuteNonQuery();
                        }
                    }
                }
                return userId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving user: {ex.Message}");
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Deletes a user from the database. </summary>
        /// <param name="user">The user to delete.</param>
        /// <returns>True if the user is successfully deleted, false otherwise.</returns>
        public bool DeleteUser(User user)
        {
            try
            {
                using (var command = new NpgsqlCommand("DELETE FROM Users WHERE Username = @username", connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);

                    int rowsAffected = command.ExecuteNonQuery();
                    bool rowDeleted = rowsAffected > 0;
                    return rowDeleted;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting user: {ex.Message}");
                return false;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Saves a card to a users stack. </summary>
        /// <param name="user">The user to save the card to.</param>
        /// <param name="card">The card to save to the user.</param>
        /// <returns>True if the association is successful, false otherwise.</returns>
        public bool SaveCardToUser(User user, Card card)
        {
            try
            {
                using (var command = new NpgsqlCommand("INSERT INTO user_cards (userid, cardid, indeck, lockedintrade) VALUES (@userid, @cardid, 'true', 'false')", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@cardid", card.Id);
                    command.ExecuteNonQuery();
                }

                user.Deck.Add(card);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SaveCardToUser: " + ex.Message);
                return false;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Removes a card from a users stack. </summary>
        /// <param name="user">The user from whom the card is to be removed.</param>
        /// <param name="card">The card to remove from the user.</param>
        /// <returns>True if the removal is successful, false otherwise.</returns>
        public bool RemoveCardFromUser(User user, Card card)
        {
            try
            {
                using (var command = new NpgsqlCommand("DELETE FROM user_cards WHERE userid = @userid AND cardid = @cardid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@cardid", card.Id);
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RemoveCardFromUser: " + ex.Message);
                return false;
            }
        }

    }
}
