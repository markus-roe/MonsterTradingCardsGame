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