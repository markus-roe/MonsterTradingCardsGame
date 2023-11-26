using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Database;
using System.Text.Json;

namespace MonsterTradingCardsGame.Controllers
{
    public class UserController : BaseController
    {
        public class UserCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public readonly DatabaseService _databaseService;

        public UserController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public void RegisterUser(HttpServerEventArguments e, Dictionary<string, string> parameters)
        {
            UserCredentials userCredentials;
            try
            {
                userCredentials = JsonSerializer.Deserialize<UserCredentials>(e.Payload);
                if (userCredentials == null || string.IsNullOrWhiteSpace(userCredentials.Username) || string.IsNullOrWhiteSpace(userCredentials.Password))
                {
                    e.Reply(400, "Invalid user credentials."); // Bad Request
                    return;
                }
            }
            catch (JsonException)
            {
                e.Reply(400, "Bad request: Invalid JSON format."); // Bad Request
                return;
            }

            var existingUser = _databaseService.GetUserByUsername(userCredentials.Username);
            if (existingUser != null)
            {
                e.Reply(409, "Username already exists."); // Conflict
                return;
            }

            // TODO: Hash the password before storing it
            AddUserToDatabase(userCredentials);
            e.Reply(201, "User successfully registered."); // OK
        }


        public void EditUser(HttpServerEventArguments e, Dictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("username", out var username))
            {
                e.Reply(400, "Bad Request: Username parameter is missing.");
                return;
            }

            try
            {
                var newDetails = JsonSerializer.Deserialize<UserCredentials>(e.Payload);
                if (newDetails == null || string.IsNullOrWhiteSpace(newDetails.Password))
                {
                    e.Reply(400, "Invalid new details."); // Bad Request
                    return;
                }

                var passwordHash = HashPassword(newDetails.Password);

                _databaseService.UpdateUserDetails(username, passwordHash);
                e.Reply(200, "User successfully updated."); // OK
            }
            catch (JsonException)
            {
                e.Reply(400, "Bad request: Invalid JSON format."); // Bad Request
            }
        }

        public void GetUser(HttpServerEventArguments e, Dictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("username", out var username))
            {
                e.Reply(400, "Bad Request: Username parameter is missing.");
                return;
            }

            var user = _databaseService.GetUserByUsername(username);
            if (user == null)
            {
                e.Reply(404, "User not found.");
            }
            else
            {
                var response = JsonSerializer.Serialize(user);
                e.Reply(200, response); // OK
            }
        }

        // *********************************************************************************************************************
        // Private methods

        private void AddUserToDatabase(UserCredentials userCredentials)
        {
            var passwordHash = HashPassword(userCredentials.Password);
            _databaseService.AddUser(userCredentials.Username, passwordHash);
        }

        private string HashPassword(string password)
        {
            //TODO Implement HashPassword method
            return password;
        }


    }
}