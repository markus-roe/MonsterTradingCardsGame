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

            var existingUser = FindUserByUsername(userCredentials.Username);
            if (existingUser == null)
            {
                return Response.UsernameAlreadyExists;
            }

            AddUserToDatabase(userCredentials);
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


        public User FindUserByUsername(string username)
        {
            var records = _databaseService.ExecuteQuery($"SELECT * FROM users WHERE username = '{username}';");
            if (records.Count == 0)
            {
                return null;
            }

            // Mapping the record to a User object
            return MapToUser(records[0]);
        }

        private void AddUserToDatabase(UserCredentials userCredentials)
        {
            //TODO Hash Password
            _databaseService.ExecuteQuery($"INSERT INTO users (username, password_hash) VALUES ('{userCredentials.Username}', '{userCredentials.Password}');");
        }

        private User MapToUser(Dictionary<string, object> record)
        {
            // Method to map a database record to a User object
            return new User
            {
                Username = record["username"].ToString(),
                Coins = Convert.ToInt32(record["coins"]),
                Elo = Convert.ToInt32(record["elo"])
            };
        }


        public void GetStack(HttpServerEventArguments e)
        {
        }

        public void GetDeck(HttpServerEventArguments e)
        {
        }


    }
}