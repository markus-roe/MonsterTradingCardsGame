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

        public void GetAll(HttpServerEventArguments e, Dictionary<string, string> parameters)
        {
            try
            {
                unitOfWork.CreateTransaction();

                var users = unitOfWork.UserRepository().GetAll();

                var response = JsonSerializer.Serialize(users);
                e.Reply(200, response);

                unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                e.Reply(500, "Internal Server Error");
            }
            finally
            {
                /*unitOfWork.Dispose();*/
            }
        }


        public void GetUserByUsername(HttpServerEventArguments e, Dictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("username", out var username))
            {
                e.Reply(400, "Bad Request: Username parameter is missing.");
                return;
            }

            try
            {
                var user = unitOfWork.UserRepository().GetUserByUsername(username);
                if (user == null)
                {
                    e.Reply(404, "User not found.");
                    return;
                }

                var jsonResponse = JsonSerializer.Serialize(user);
                e.Reply(200, jsonResponse);
            }
            catch (Exception ex)
            {
                e.Reply(500, "Internal server error.");
            }
        }
    }
}

