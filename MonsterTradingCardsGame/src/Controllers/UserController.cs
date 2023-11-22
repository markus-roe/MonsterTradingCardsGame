using System;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Database;


namespace MonsterTradingCardsGame
{
    public class UserController
    {

        public readonly DatabaseService _databaseService;


        public UserController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // methods                                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public int RegisterUser(HttpServerEventArguments e)
        {
            return 0;
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