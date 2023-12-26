using Moq;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using System.Text.Json;
using MonsterTradingCardsGame.Services.Interfaces;
using MonsterTradingCardsGame.Server;


namespace MonsterTradingCardsGame.Tests.Controllers
{

    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IAuthenticationService> _mockAuthService;
        private Mock<ICardRepository> _mockCardRepository;
        private UserController _userController;
        private Mock<IHttpServerEventArguments> _mockHttpEventArguments;

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockCardRepository = new Mock<ICardRepository>();
            _mockHttpEventArguments = new Mock<IHttpServerEventArguments>();

            _userController = new UserController(
                _mockUserRepository.Object,
                _mockAuthService.Object,
                _mockCardRepository.Object);
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

            Dictionary<string, string> parameters = new Dictionary<string, string>();


            // Act
            _userController.RegisterUser(_mockHttpEventArguments.Object, parameters);

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

            _mockUserRepository = new Mock<IUserRepository>();
            _mockUserRepository.Setup(m => m.GetUserByUsername(username)).Returns(user);

            _mockAuthService = new Mock<IAuthenticationService>();
            _mockCardRepository = new Mock<ICardRepository>();
            _mockHttpEventArguments = new Mock<IHttpServerEventArguments>();

            _userController = new UserController(
                _mockUserRepository.Object,
                _mockAuthService.Object,
                _mockCardRepository.Object);



            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/users");
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(JsonSerializer.Serialize(user));


            //Act
            _userController.RegisterUser(_mockHttpEventArguments.Object, new Dictionary<string, string>());

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

            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuthService = new Mock<IAuthenticationService>();

            _mockUserRepository.Setup(m => m.GetUserByUsername(username)).Returns(user);
            _mockAuthService.Setup(m => m.GenerateToken(user)).Returns($"{user.Username}-mtcgToken");
            _mockAuthService.Setup(m => m.VerifyCredentials(user.Username, user.Password)).Returns(true);

            _userController = new UserController(
                _mockUserRepository.Object,
                _mockAuthService.Object,
                _mockCardRepository.Object);



            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/sessions");
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(JsonSerializer.Serialize(user));


            // Act
            _userController.Login(_mockHttpEventArguments.Object, new Dictionary<string, string>());

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, "testuser-mtcgToken"), Times.Once());
        }

        //login with wrong password
        [Test]
        public void UserController_LoginUser_WrongCredentials()
        {
            // Arrange
            string username = "testuser";
            string password = "testpassword";

            User user = new User()
            {
                Username = username,
                Password = password
            };

            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockHttpEventArguments = new Mock<IHttpServerEventArguments>();

            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/session");
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(JsonSerializer.Serialize(user));

            _userController = new UserController(
                _mockUserRepository.Object,
                _mockAuthService.Object,
                _mockCardRepository.Object);

            // Act
            _userController.Login(_mockHttpEventArguments.Object, new Dictionary<string, string>());

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(401, "Invalid username/password provided"), Times.Once());

        }

    }

}