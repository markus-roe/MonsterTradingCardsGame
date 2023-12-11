using System.Text.Json;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;

namespace MonsterTradingCardsGame.Controllers
{
    public class UserController
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService authService;

        public UserController(IUserRepository userRepository, IAuthenticationService authService)
        {
            this._userRepository = userRepository;
            this.authService = authService;
        }
        public class UserCredentials
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        public class UserUpdateInfo
        {
            public string Name { get; set; }
            public string Bio { get; set; }
            public string Image { get; set; }
        }

        [Route("GET", "/users")]
        public void GetAll(HttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            try
            {
                var users = _userRepository.GetAll();
                var response = JsonSerializer.Serialize(users);
                httpEventArguments.Reply(200, response);
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        [Route("GET", "/users/:username")]
        public void GetUserByUsername(HttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("username", out var username))
            {
                httpEventArguments.Reply(400, "Bad Request: Username parameter is missing.");
                return;
            }

            try
            {
                var user = _userRepository.GetUserByUsername(username); 
                if (user == null)
                {
                    httpEventArguments.Reply(404, "User not found.");
                    return;
                }

                var jsonResponse = JsonSerializer.Serialize(user);
                httpEventArguments.Reply(200, jsonResponse);
            }
            catch (Exception ex)
            {
                httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
            }
        }

        [Route("POST", "/users")]
        public void RegisterUser(HttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
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
        public void UpdateUser(HttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("username", out var username))
            {
                httpEventArguments.Reply(400, "Bad Request: Username parameter is missing.");
                return;
            }

            if (string.IsNullOrEmpty(httpEventArguments.Payload))
            {
                httpEventArguments.Reply(400, "Bad Request: Request body is missing or empty.");
                return;
            }

            try
            {

                var userInfo = JsonSerializer.Deserialize<UserUpdateInfo>(httpEventArguments.Payload);
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
        public void Login(HttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
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
                    httpEventArguments.Reply(200, JsonSerializer.Serialize(new { token = token }));
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
        public void GetCardsByUser(HttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            User user = httpEventArguments.User;

            var response = JsonSerializer.Serialize(user.Stack);
            httpEventArguments.Reply(200, response);
        }

        [Route("GET", "/deck")]
        public void GetDeck(HttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
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
        public void ConfigureDeck(HttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
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

    }
}
