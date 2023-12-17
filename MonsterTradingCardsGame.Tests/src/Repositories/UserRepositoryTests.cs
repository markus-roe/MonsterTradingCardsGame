using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;
using Moq;

namespace MonsterTradingCardsGame.Tests.Repositories
{



    [TestFixture]
    internal class UserRepositoryTests
    {

        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IAuthenticationService> _mockAuthService;
        private Mock<ICardRepository> _mockCardRepository;
        private Mock<IHttpServerEventArguments> _mockHttpEventArguments;
        private UserRepository _userRepository;

        //Setup for the tests
        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockCardRepository = new Mock<ICardRepository>();
            _mockHttpEventArguments = new Mock<IHttpServerEventArguments>();

            _userRepository = new UserRepository(_mockCardRepository.Object);
        }

        [Test]
        public void GetUserByUsername_ExistingUsername_ReturnsUser()
        {
            // Arrange
            string username = "kienboec";

            // Act
            User? user = _userRepository.GetUserByUsername(username);

            // Assert
            Assert.IsNotNull(user);
            Assert.That(user.Username, Is.EqualTo(username));
        }

        [Test]
        public void GetUserByUsername_NonExistingUsername_ReturnsNull()
        {
            // Arrange
            string username = "nonexistinguser";

            // Act
            User? user = _userRepository.GetUserByUsername(username);

            // Assert
            Assert.IsNull(user);
        }

        //test for update user
        [Test]
        public void UpdateUser_ExistingUser_ReturnsTrue()
        {
            // Arrange
            User user = new User()
            {
                Username = "kienboec",
                Name = "Kienboec",
                Bio = "I am a cool guy",
                Image = ":----D",
                Coins = 100,
                Elo = 1000
            };

            // Act
            bool result = _userRepository.Update(user);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void UpdateNonExistingUser_ReturnsFalse()
        {
            // Arrange
            User user = new User()
            {
                Username = "nonexistinguser",
                Name = "Kienboec",
                Bio = "I am a cool guy",
                Image = ":----D",
                Coins = 100,
                Elo = 1000
            };

            // Act
            bool result = _userRepository.Update(user);

            // Assert
            Assert.IsFalse(result);
        }

        //test for register user
        [Test]
        public void RegisterUser_ReturnsTrue()
        {
            // Arrange
            User user = new User()
            {
                Username = "newUser",
                Password = "12345678"
            };
            // Cleanup before test
            _userRepository.Delete(user);

            // Act
            bool result = _userRepository.Save(user);

            // Assert
            Assert.IsTrue(result);

            // Cleanup
            _userRepository.Delete(user);
        }

    }
}