using System.Text.Json;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Utils.UserStats;
using MonsterTradingCardsGame.Utils.UserProfile;
using MonsterTradingCardsGame.Services.Interfaces;
using MonsterTradingCardsGame.Repositories;

namespace MonsterTradingCardsGame.Controllers
{
    public class UserController
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService _authService;
        private readonly ICardRepository _cardRepository;
        private readonly ISessionRepository _sessionRepository;

        public UserController(IUserRepository userRepository, IAuthenticationService authService, ICardRepository cardRepository, ISessionRepository sessionRepository)
        {
            _userRepository = userRepository;
            _authService = authService;
            _cardRepository = cardRepository;
            _sessionRepository = sessionRepository;
        }
        public class UserCredentials
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This method retrieves a user profile based on a given username. </summary>
        [Route("GET", "/users/:username")]
        public void GetUserByUsername(IHttpServerEventArguments httpEventArguments)
        {
            if (!httpEventArguments.Parameters.TryGetValue("username", out var username))
            {
                httpEventArguments.Reply(400, "Bad Request: Username parameter is missing.");
                return;
            }

            User? user = httpEventArguments.User;

            try
            {
                if (username != httpEventArguments.User.Username || httpEventArguments.User.Username == "admin")
                {
                    httpEventArguments.Reply(403, "Forbidden: You are not allowed to access this resource.");
                    return;
                }

                UserProfile userProfile = new()
                {
                    Name = user.Name,
                    Bio = user.Bio,
                    Image = user.Image
                };

                var response = JsonSerializer.Serialize(userProfile);
                httpEventArguments.Reply(200, response);
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This method registers a new user. </summary>
        [Route("POST", "/users")]
        public void RegisterUser(IHttpServerEventArguments httpEventArguments)
        {
            try
            {
                var userCredentials = JsonSerializer.Deserialize<UserCredentials>(httpEventArguments.Payload);

                if (userCredentials == null || string.IsNullOrWhiteSpace(userCredentials.Username) || string.IsNullOrWhiteSpace(userCredentials.Password))
                {
                    httpEventArguments.Reply(400, "Invalid username or password.");
                    return;
                }

                if (_userRepository.GetUserByUsername(userCredentials.Username) != null)
                {
                    httpEventArguments.Reply(409, "Username already exists.");
                    return;
                }

                string hashedPassword = _authService.HashPassword(userCredentials.Password);

                var newUser = new User
                {
                    Username = userCredentials.Username,
                    Password = hashedPassword,
                };

                _userRepository.SaveUser(newUser);
                httpEventArguments.Reply(201, "User registered successfully.");
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This method updates an existing user's profile. </summary>
        [Route("PUT", "/users/:username")]
        public void UpdateUser(IHttpServerEventArguments httpEventArguments)
        {


            if (!httpEventArguments.Parameters.TryGetValue("username", out var username))
            {
                httpEventArguments.Reply(400, "Bad Request: Username parameter is missing.");
                return;
            }

            if (username != httpEventArguments.User.Username && httpEventArguments.User.Username != "admin")
            {
                httpEventArguments.Reply(403, "Forbidden: You are not allowed to access this resource.");
                return;
            }

            if (string.IsNullOrEmpty(httpEventArguments.Payload))
            {
                httpEventArguments.Reply(400, "Bad Request: Request body is missing or empty.");
                return;
            }

            try
            {

                var userInfo = JsonSerializer.Deserialize<UserProfile>(httpEventArguments.Payload);
                if (userInfo == null)
                {
                    httpEventArguments.Reply(400, "Invalid user data.");
                    return;
                }

                // Check if Name, Bio, and Image exist
                if (string.IsNullOrEmpty(userInfo.Name) ||
                    string.IsNullOrEmpty(userInfo.Bio) ||
                    string.IsNullOrEmpty(userInfo.Image))
                {
                    httpEventArguments.Reply(400, "Invalid user data: Name, Bio, and Image are required.");
                    return;
                }

                var existingUser = _userRepository.GetUserByUsername(username);
                if (existingUser == null)
                {
                    httpEventArguments.Reply(404, "User not found.");
                    return;
                }

                // Update user details
                existingUser.Name = userInfo.Name;
                existingUser.Bio = userInfo.Bio;
                existingUser.Image = userInfo.Image;

                _userRepository.UpdateUser(existingUser);
                httpEventArguments.Reply(200, "User updated successfully.");
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This method handles user login and session creation. </summary>
        [Route("POST", "/sessions")]
        public void Login(IHttpServerEventArguments httpEventArguments)
        {
            try
            {
                var userCredentials = JsonSerializer.Deserialize<UserCredentials>(httpEventArguments.Payload);

                if (userCredentials == null || string.IsNullOrWhiteSpace(userCredentials.Username) || string.IsNullOrWhiteSpace(userCredentials.Password))
                {
                    httpEventArguments.Reply(400, "Invalid request.");
                    return;
                }

                if (_authService.VerifyCredentials(userCredentials.Username, userCredentials.Password))
                {
                    var user = _userRepository.GetUserByUsername(userCredentials.Username);

                    var session = new Session
                    {
                        SessionId = Guid.NewGuid().ToString(),
                        UserId = user.Id,
                        StartTime = DateTime.UtcNow,
                        EndTime = DateTime.UtcNow.AddDays(7),
                    };

                    if (_sessionRepository.AddSession(session) == false)
                    {
                        httpEventArguments.Reply(500, "Internal server error: Could not create session.");
                        return;
                    }

                    user.Session = session;

                    var token = _authService.GenerateToken(user);
                    httpEventArguments.Reply(200, token);
                }
                else
                {
                    httpEventArguments.Reply(401, "Invalid username/password provided");
                }
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This method handles user logout and session termination. </summary>
        [Route("DELETE", "/sessions")]
        public void Logout(IHttpServerEventArguments httpEventArguments)
        {
            try
            {
                if (_sessionRepository.RemoveSession(httpEventArguments.User.Session))
                {
                    httpEventArguments.Reply(200, "Logout successful.");
                }
                else
                {
                    httpEventArguments.Reply(500, "Internal server error: Could not delete session.");
                }
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This method retrieves the statistics of a user. </summary>
        [Route("GET", "stats")]
        public void GetStats(IHttpServerEventArguments httpEventArguments)
        {
            try
            {
                User user = httpEventArguments.User;

                UserStats stats = _userRepository.GetStatsByUser(user);

                if (stats == null)
                {
                    httpEventArguments.Reply(404, "No stats available for this user");
                    return;
                }

                httpEventArguments.Reply(200, JsonSerializer.Serialize(stats));
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }

        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This method retrieves the scoreboard containing user statistics. </summary>
        [Route("GET", "scoreboard")]
        public void GetScoreboard(IHttpServerEventArguments httpEventArguments)
        {
            try
            {
                List<UserStats> scoreboard = _userRepository.GetScoreboard();

                if (scoreboard == null)
                {
                    httpEventArguments.Reply(404, "No users available");
                    return;
                }

                httpEventArguments.Reply(200, JsonSerializer.Serialize(scoreboard));
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }

        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> This is a mandatory feature. A user can trade a random card of his stack (not in deck) for 6 coins </summary>
        [Route("POST", "coins")]
        public void TradeCardForCoins(IHttpServerEventArguments httpEventArguments)
        {
            try
            {
                User user = httpEventArguments.User;

                List<Card> cards = user.Stack.ToList();

                if (cards.Count < 1)
                {
                    httpEventArguments.Reply(400, "Not enough cards. You need at least 1 card to trade.");
                    return;
                }

                // Get random card which is not in deck and not locked
                List<Card> cardsNotInDeck = cards.Where(c => !user.Deck.Contains(c) && !c.IsLocked).ToList();

                if (cardsNotInDeck.Count < 1)
                {
                    httpEventArguments.Reply(400, "No cards available to trade. All cards are in the deck.");
                    return;
                }

                Card card = cardsNotInDeck[new Random().Next(0, cardsNotInDeck.Count)];

                user.Coins += 6;
                _userRepository.UpdateUser(user);

                _userRepository.RemoveCardFromUser(user, card);

                httpEventArguments.Reply(200, "Card traded successfully, you received 6 coins.");
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
