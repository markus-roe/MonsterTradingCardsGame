using Moq;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using System.Text.Json;
using MonsterTradingCardsGame.Services.Interfaces;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Utils.UserProfile;
using MonsterTradingCardsGame.Utils.UserStats;
using MonsterTradingCardsGame.Repositories;


namespace MonsterTradingCardsGame.UnitTests.Controllers
{

    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IAuthenticationService> _mockAuthService;
        private Mock<ICardRepository> _mockCardRepository;
        private Mock<IHttpServerEventArguments> _mockHttpEventArguments;
        private Mock<ISessionRepository> _mockSessionRepository;

        private UserController _userController;

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockCardRepository = new Mock<ICardRepository>();
            _mockHttpEventArguments = new Mock<IHttpServerEventArguments>();
            _mockSessionRepository = new Mock<ISessionRepository>();

            _userController = new UserController(
                _mockUserRepository.Object,
                _mockAuthService.Object,
                _mockCardRepository.Object,
                _mockSessionRepository.Object);
        }


        [Test]
        public void UserController_RegisterUser()
        {
            // Arrange
            string username = "testuser";
            string password = "testpassword";

            User user = new User()
            {
                Username = username,
                Password = password
            };

            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/users");
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(JsonSerializer.Serialize(user));

            // Act
            _userController.RegisterUser(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(201, "User registered successfully."), Times.Once());
        }

        [Test]
        public void UserController_RegisterUser_UsernameAlreadyExists()
        {
            // Arrange
            string username = "testuser";
            string password = "testpassword";

            User user = new User()
            {
                Username = username,
                Password = password
            };

            _mockUserRepository.Setup(m => m.GetUserByUsername(username)).Returns(user);
            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/users");
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(JsonSerializer.Serialize(user));


            //Act
            _userController.RegisterUser(_mockHttpEventArguments.Object);

            //Assert
            _mockHttpEventArguments.Verify(m => m.Reply(409, "Username already exists."), Times.Once());
        }

        [Test]
        //login
        public void UserController_LoginUser()
        {
            // Arrange
            string username = "testuser";
            string password = "testpassword";

            User user = new User()
            {
                Username = username,
                Password = password
            };


            _mockUserRepository.Setup(m => m.GetUserByUsername(username)).Returns(user);
            _mockSessionRepository.Setup(m => m.AddSession(It.IsAny<Session>())).Returns(true);
            _mockAuthService.Setup(m => m.GenerateToken(user)).Returns($"{user.Username}-mtcgToken");
            _mockAuthService.Setup(m => m.VerifyCredentials(user.Username, user.Password)).Returns(true);


            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/sessions");
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(JsonSerializer.Serialize(user));


            // Act
            _userController.Login(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, "testuser-mtcgToken"), Times.Once());
        }

        //login with wrong credentials
        [Test]
        public void UserController_LoginUser_WrongCredentials()
        {
            // Arrange
            string username = "wronguser";
            string password = "wrongpassword";

            User user = new User()
            {
                Username = username,
                Password = password
            };

            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/session");
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(JsonSerializer.Serialize(user));

            // Act
            _userController.Login(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(401, "Invalid username/password provided"), Times.Once());

        }

        //get user
        [Test]
        public void UserController_GetUser()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Password = "testpassword",
                Name = "testuser",
                Bio = "testbio",
                Image = "testimage"
            };

            UserProfile userProfile = new()
            {
                Name = user.Name,
                Bio = user.Bio,
                Image = user.Image
            };


            _mockUserRepository.Setup(m => m.GetUserByUsername(user.Username)).Returns(user);

            _mockHttpEventArguments.Setup(m => m.Method).Returns("GET");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/users/testuser");
            _mockHttpEventArguments.Setup(m => m.Parameters).Returns(new Dictionary<string, string> { { "username", "testuser" } });

            // Act
            _userController.GetUserByUsername(_mockHttpEventArguments.Object);


            // Assert
            string response = JsonSerializer.Serialize(userProfile);

            _mockHttpEventArguments.Verify(m => m.Reply(200, response), Times.Once());
        }


        //change user profile data
        [Test]
        public void UserController_ChangeUserProfile()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Password = "testpassword",
                Name = "testuser",
                Bio = "testbio",
                Image = "testimage"
            };

            UserProfile userProfile = new()
            {
                Name = user.Name,
                Bio = user.Bio,
                Image = user.Image
            };


            _mockUserRepository.Setup(m => m.GetUserByUsername(user.Username)).Returns(user);

            _mockHttpEventArguments.Setup(m => m.Method).Returns("PUT");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/users/testuser");
            _mockHttpEventArguments.Setup(m => m.Parameters).Returns(new Dictionary<string, string> { { "username", "testuser" } });
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(JsonSerializer.Serialize(userProfile));


            // Act
            _userController.UpdateUser(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, "User updated successfully."), Times.Once());
        }

        //change user profile data with wrong credentials
        [Test]
        public void UserController_ChangeUserProfileOfOtherUser()
        {
            // Arrange
            User user = new User()
            {
                Username = "wronguser",
                Password = "wrongpassword",
                Name = "testuser",
                Bio = "testbio",
                Image = "testimage"
            };

            UserProfile userProfile = new()
            {
                Name = user.Name,
                Bio = user.Bio,
                Image = user.Image
            };


            _mockUserRepository.Setup(m => m.GetUserByUsername(user.Username)).Returns(user);

            _mockHttpEventArguments.Setup(m => m.Method).Returns("PUT");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/users/testuser");
            _mockHttpEventArguments.Setup(m => m.Parameters).Returns(new Dictionary<string, string> { { "username", "testuser" } });
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(JsonSerializer.Serialize(userProfile));


            // Act
            _userController.UpdateUser(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(403, "Forbidden: You are not allowed to access this resource."), Times.Once());
        }

        //stats request
        [Test]
        public void UserController_GetStats()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Name = "testuser",
            };

            UserStats stats = new UserStats()
            {
                Name = user.Name,
                Elo = 100,
                Wins = 5,
                Losses = 2,
                Winratio = 0.71
            };


            _mockUserRepository.Setup(m => m.GetStatsByUser(user)).Returns(stats);

            _mockHttpEventArguments.Setup(m => m.Method).Returns("GET");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/stats");
            _mockHttpEventArguments.Setup(m => m.Parameters).Returns(new Dictionary<string, string> { { "username", "testuser" } });


            // Act
            _userController.GetStats(_mockHttpEventArguments.Object);

            string returnedStats = JsonSerializer.Serialize(stats);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, returnedStats), Times.Once());
        }

        // get scoreboard for all users
        [Test]
        public void UserController_GetScoreboard()
        {

            List<UserStats> scoreboard = new List<UserStats>();
            scoreboard.Add(new UserStats()
            {
                Name = "Testuser1",
                Elo = 100,
                Wins = 5,
                Losses = 2,
                Winratio = 0.71
            });
            scoreboard.Add(new UserStats()
            {
                Name = "Testuser2",
                Elo = 200,
                Wins = 10,
                Losses = 3,
                Winratio = 0.77
            });


            _mockUserRepository.Setup(m => m.GetScoreboard()).Returns(scoreboard);

            _mockHttpEventArguments.Setup(m => m.Method).Returns("GET");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/scoreboard");


            // Act
            _userController.GetScoreboard(_mockHttpEventArguments.Object);

            string returnedScoreboard = JsonSerializer.Serialize(scoreboard);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, returnedScoreboard), Times.Once());
        }

        // TradeCardForCoins with not enough cards
        [Test]
        public void UserController_TradeCardForCoins_NotEnoughCards()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Password = "testpassword",
                Name = "testuser",
                Bio = "testbio",
                Image = "testimage",
                Coins = 0
            };

            List<Card> cards = new List<Card>();

            _mockUserRepository.Setup(m => m.GetUserByUsername(user.Username)).Returns(user);
            _mockCardRepository.Setup(m => m.GetCardsByUser(user)).Returns(cards);

            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/coins");


            // Act
            _userController.TradeCardForCoins(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(400, "Not enough cards. You need at least 1 card to trade."), Times.Once());
        }

        // TradeCardForCoins with no cards available to trade
        [Test]
        public void UserController_TradeCardForCoins_NoCardsAvailableToTrade()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Password = "testpassword",
                Name = "testuser",
                Bio = "testbio",
                Image = "testimage",
                Coins = 0
            };

            List<Card> cards = new List<Card>();
            cards.Add(new Card()
            {
                Id = "testid",
                Name = "testcard",
                Damage = 100,
                Element = ElementType.fire,
                Type = CardType.spell
            });

            user.Stack = cards;
            user.Deck = cards;

            _mockUserRepository.Setup(m => m.GetUserByUsername(user.Username)).Returns(user);
            _mockCardRepository.Setup(m => m.GetCardsByUser(user)).Returns(cards);

            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/coins");


            // Act
            _userController.TradeCardForCoins(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(400, "No cards available to trade. All cards are in the deck."), Times.Once());

        }

        // TradeCardForCoins with success
        [Test]
        public void UserController_TradeCardForCoins_Success()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Password = "testpassword",
                Name = "testuser",
                Bio = "testbio",
                Image = "testimage",
                Coins = 0
            };

            List<Card> cards = new List<Card>();
            cards.Add(new Card()
            {
                Id = "testid",
                Name = "testcard",
                Damage = 100,
                Element = ElementType.fire,
                Type = CardType.spell
            });

            user.Stack = cards;

            _mockUserRepository.Setup(m => m.GetUserByUsername(user.Username)).Returns(user);
            _mockCardRepository.Setup(m => m.GetCardsByUser(user)).Returns(cards);
            _mockUserRepository.Setup(m => m.UpdateUser(user)).Returns(true);
            _mockUserRepository.Setup(m => m.RemoveCardFromUser(user, cards[0])).Returns(true);

            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/coins");


            // Act
            _userController.TradeCardForCoins(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, "Card traded successfully, you received 6 coins."), Times.Once());
        }

    }

}