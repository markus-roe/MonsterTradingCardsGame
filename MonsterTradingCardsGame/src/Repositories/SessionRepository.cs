/*
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
                                Losses = (int)reader["losses"]
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

    }
}

*/
using Npgsql;
using MonsterTradingCardsGame.Interfaces;

namespace MonsterTradingCardsGame.Repositories
{
    public class Session
    {
        public required string SessionId { get; set; }
        public required int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class SessionRepository : BaseRepository, ISessionRepository
    {
        private List<Session> sessions;

        public SessionRepository()
        {
            sessions = new List<Session>();
        }

        public bool AddSession(Session session)
        {
            try
            {
                using (var command = new NpgsqlCommand("INSERT INTO sessions (sessionid, userid, starttime, endtime) VALUES (@sessionid, @userid, @starttime, @endtime)", connection))
                {
                    command.Parameters.AddWithValue("@sessionid", Guid.Parse(session.SessionId.ToString()));
                    command.Parameters.AddWithValue("@userid", session.UserId);
                    command.Parameters.AddWithValue("@starttime", session.StartTime);

                    if (session.EndTime.HasValue)
                    {
                        command.Parameters.AddWithValue("@endtime", session.EndTime.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@endtime", DBNull.Value);
                    }

                    command.ExecuteNonQuery();
                }

                sessions.Add(session);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving session: {ex.Message}");
                return false;
            }

        }

        public bool RemoveSession(Session session)
        {
            try
            {
                using (var command = new NpgsqlCommand("DELETE FROM sessions WHERE sessionid = @sessionid", connection))
                {
                    command.Parameters.AddWithValue("@sessionid", Guid.Parse(session.SessionId));

                    command.ExecuteNonQuery();
                }

                sessions.Remove(session);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting session: {ex.Message}");
                return false;
            }
        }

        public Session? GetSessionById(string sessionId)
        {
            try
            {
                using (var command = new NpgsqlCommand("SELECT * FROM sessions WHERE sessionId = @sessionId::uuid", connection))
                {
                    command.Parameters.AddWithValue("@sessionId", Guid.Parse(sessionId));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var session = new Session
                            {
                                SessionId = reader["sessionid"].ToString(),
                                UserId = (int)reader["userid"],
                                StartTime = (DateTime)reader["starttime"],
                                EndTime = reader.IsDBNull(reader.GetOrdinal("endtime")) ? null : (DateTime?)reader["endtime"]
                            };
                            return session;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving session by user: {ex.Message}\n {ex.StackTrace}");
                return null;
            }
        }

    }
}