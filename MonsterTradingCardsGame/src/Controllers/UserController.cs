using System;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Controllers;


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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // methods                                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public Response RegisterUser(UserCredentials userCredentials)
        {
            // Check if user already exists
            var existingUser = GetUser(userCredentials.Username);
            if (existingUser != null)
            {
                return Response.UsernameAlreadyExists;
            }

            //TODO Create new user in the database -> EXECUTE QUERY NOT WORKING?
            _databaseService.ExecuteQuery($"INSERT INTO users (username, password_hash) VALUES ('{userCredentials.Username}', '{userCredentials.Password}');");

            return Response.Success;
        }


        public bool LoginUser(HttpServerEventArguments e)
        {
            return true;
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // setter                                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool EditUser(HttpServerEventArguments e)
        {
            return true;
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // getter                                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public List<User> GetUsers()
        {
            var records = _databaseService.ExecuteQuery("SELECT * FROM users;");
            var userList = new List<User>();

            foreach (var record in records)
            {
                User user = new User
                {
                    Username = record["username"].ToString(),
                    Coins = Convert.ToInt32(record["coins"]),
                    Elo = Convert.ToInt32(record["elo"])
                };
                userList.Add(user);
            }

            return userList;
        }


        public User GetUser(string username)
        {
            var records = _databaseService.ExecuteQuery($"SELECT * FROM users WHERE username = '{username}';");

            Console.WriteLine(records.Count);

            if (records.Count == 0)
            {
                return null; // or handle the user not found scenario
            }

            var record = records[0]; // Assuming username is unique and returns only one record
            User user = new User
            {
                Username = record["username"].ToString(),
                Coins = Convert.ToInt32(record["coins"]),
                Elo = Convert.ToInt32(record["elo"])
            };

            return user;
        }


        public void GetStack(HttpServerEventArguments e)
        {
        }

        public void GetDeck(HttpServerEventArguments e)
        {
        }


    }
}