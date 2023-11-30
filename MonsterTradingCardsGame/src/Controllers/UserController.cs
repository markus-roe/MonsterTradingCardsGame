using System.Text.Json;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Repositories;

namespace MonsterTradingCardsGame.Controllers
{
    public class UserController : BaseController
    {
        public class UserCredentials
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        private readonly UserRepository userRepository;

        public UserController()
        {
            userRepository = new UserRepository();
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
            catch (Exception)
            {
                httpEventArguments.Reply(500, "Internal Server Error");
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
            catch (Exception)
            {
                httpEventArguments.Reply(500, "Internal server error.");
            }
        }

    }
}
