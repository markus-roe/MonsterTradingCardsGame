using System.Text.Json;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Interfaces;

namespace MonsterTradingCardsGame.Controllers
{
    public class UserController : BaseController
    {
        public class UserCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private readonly IUnitOfWork unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [Route("GET", "/users")]
        public void GetAll(HttpServerEventArguments httpEventArguments, Dictionary<string, string> parameters)
        {
            try
            {
                unitOfWork.CreateTransaction();

                var users = unitOfWork.UserRepository().GetAll();

                var response = JsonSerializer.Serialize(users);
                httpEventArguments.Reply(200, response);

                unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                httpEventArguments.Reply(500, "Internal Server Error");
            }
            finally
            {
                /*unitOfWork.Dispose();*/
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
                var user = unitOfWork.UserRepository().GetUserByUsername(username);
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
                httpEventArguments.Reply(500, "Internal server error.");
            }
        }
    }
}

