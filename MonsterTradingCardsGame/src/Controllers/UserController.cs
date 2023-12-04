using System.Text.Json;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;

namespace MonsterTradingCardsGame.Controllers
{
    public class UserController
    {
        private readonly IUserRepository userRepository;
        private readonly IAuthenticationService authService;

        public UserController(IUserRepository userRepository, IAuthenticationService authService)
        {
            this.userRepository = userRepository;
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
                var users = userRepository.GetAll();
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
                var user = userRepository.GetUserByUsername(username); 
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

                if (userRepository.GetUserByUsername(userCredentials.Username) != null)
                {
                    httpEventArguments.Reply(409, "Username already exists.");
                    return;
                }

                var newUser = new User
                {
                    Username = userCredentials.Username,
                    Password = userCredentials.Password,
                };

                userRepository.Save(newUser);
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

            try
            {
                var userInfo = JsonSerializer.Deserialize<UserUpdateInfo>(httpEventArguments.Payload);
                if (userInfo == null)
                {
                    httpEventArguments.Reply(400, "Invalid user data.");
                    return;
                }

                var existingUser = userRepository.GetUserByUsername(username);
                if (existingUser == null)
                {
                    httpEventArguments.Reply(404, "User not found.");
                    return;
                }

                // Update user details
                existingUser.Name = userInfo.Name;
                existingUser.Bio = userInfo.Bio;
                existingUser.Image = userInfo.Image;

                userRepository.Update(existingUser);
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
                    var user = userRepository.GetUserByUsername(userCredentials.Username);
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


    }
}
