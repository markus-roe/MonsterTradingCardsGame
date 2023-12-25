using System.Text.Json;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Utils.UserStats;
using MonsterTradingCardsGame.Services.Interfaces;

namespace MonsterTradingCardsGame.Controllers
{
    public class UserController
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService authService;
        private readonly ICardRepository _cardRepository;

        public UserController(IUserRepository userRepository, IAuthenticationService authService, ICardRepository cardRepository)
        {
            this._userRepository = userRepository;

            this.authService = authService;
            this._cardRepository = cardRepository;
        }
        public class UserCredentials
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        public class UserProfileInfo
        {
            public string? Name { get; set; }
            public string? Bio { get; set; }
            public string? Image { get; set; }
        }


        public class UserProfileResponse
        {
            public string? Name { get; set; }
            public string? Bio { get; set; }
            public string? Image { get; set; }
        }

        [Route("GET", "/users/:username")]
        public void GetUserByUsername(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("username", out var username))
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

                var userProfile = new UserProfileResponse
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

        [Route("POST", "/users")]
        public void RegisterUser(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
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

                var newUser = new User
                {
                    Username = userCredentials.Username,
                    Password = userCredentials.Password,
                };

                _userRepository.Save(newUser);
                httpEventArguments.Reply(201, "User registered successfully.");
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        [Route("PUT", "/users/:username")]
        public void UpdateUser(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {


            if (!parameters.TryGetValue("username", out var username))
            {
                httpEventArguments.Reply(400, "Bad Request: Username parameter is missing.");
                return;
            }

            if (username != httpEventArguments.User.Username || httpEventArguments.User.Username == "admin")
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

                var userInfo = JsonSerializer.Deserialize<UserProfileInfo>(httpEventArguments.Payload);
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

                _userRepository.Update(existingUser);
                httpEventArguments.Reply(200, "User updated successfully.");
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        [Route("POST", "/sessions")]
        public void Login(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            try
            {
                var userCredentials = JsonSerializer.Deserialize<UserCredentials>(httpEventArguments.Payload);

                if (userCredentials == null || string.IsNullOrWhiteSpace(userCredentials.Username) || string.IsNullOrWhiteSpace(userCredentials.Password))
                {
                    httpEventArguments.Reply(400, "Invalid request.");
                    return;
                }

                if (authService.VerifyCredentials(userCredentials.Username, userCredentials.Password))
                {
                    var user = _userRepository.GetUserByUsername(userCredentials.Username);
                    var token = authService.GenerateToken(user);
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

        [Route("GET", "/cards")]
        public void GetCardsByUser(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            User user = httpEventArguments.User;

            var response = JsonSerializer.Serialize(user.Stack);
            httpEventArguments.Reply(200, response);
        }

        [Route("GET", "/deck")]
        public void GetDeck(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            try
            {
                User user = httpEventArguments.User;
                var deck = user.Deck;

                if (deck.Count == 0)
                {
                    httpEventArguments.Reply(204, "The request was fine, but the deck doesn't have any cards");
                    return;
                }

                // Extract the format parameter from the query parameters
                httpEventArguments.QueryParameters.TryGetValue("format", out var format);
                format = format?.ToLower() ?? "json";

                string response;
                if (format == "plain")
                {
                    response = string.Join("\n", deck.Select(card => card.ToString()));
                }
                else
                {
                    response = JsonSerializer.Serialize(deck);
                }

                httpEventArguments.Reply(200, response);
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        [Route("PUT", "/deck")]
        public void ConfigureDeck(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            try
            {
                User user = httpEventArguments.User;

                // Check if the request body is empty
                if (string.IsNullOrEmpty(httpEventArguments.Payload))
                {
                    httpEventArguments.Reply(400, "Bad Request: Request body is missing or empty.");
                    return;
                }

                // Deserialize the request body
                var cardIds = JsonSerializer.Deserialize<string[]>(httpEventArguments.Payload);

                // Check if the request body is valid
                if (cardIds == null || cardIds.Length != 4)
                {
                    httpEventArguments.Reply(400, "Bad Request: Request body is invalid.");
                    return;
                }

                // Check if the user owns all the cards
                var cards = user.Stack.Where(card => cardIds.Contains(card.Id));
                if (cards.Count() != 4)
                {
                    httpEventArguments.Reply(403, "Forbidden: At least one of the provided cards does not belong to the user or is not available.");
                    return;
                }

                // Configure the deck
                _userRepository.SetCardDeck(user, cards.ToList());

                httpEventArguments.Reply(200, "The deck has been successfully configured.");
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        [Route("POST", "transactions/packages")]
        public void buyPackage(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            try
            {
                User user = httpEventArguments.User;

                if (user.Coins < 5)
                {
                    httpEventArguments.Reply(403, "Not enough money for buying a card package");
                    return;
                }

                List<Card> package = _cardRepository.GetCardPackage();

                if (package.Count == 0)
                {
                    httpEventArguments.Reply(404, "No card package available for buying");
                    return;
                }


                _cardRepository.SavePackageToUser(user, package);
                user.Coins -= 5;
                _userRepository.Update(user);

                httpEventArguments.Reply(200, "Package and cards successfully bought");
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        [Route("GET", "stats")]
        public void GetStats(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            try
            {
                User user = httpEventArguments.User;

                var stats = _userRepository.GetStatsByUser(user);

                if (stats == null)
                {
                    httpEventArguments.Reply(404, "No stats available for this user");
                    return;
                }

                httpEventArguments.Reply(200, stats);
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }

        }

        [Route("GET", "scoreboard")]
        public void GetScoreboard(IHttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
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
    }
}
