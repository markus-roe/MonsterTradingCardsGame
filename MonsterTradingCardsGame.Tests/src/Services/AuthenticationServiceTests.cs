using Moq;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services;
using Microsoft.Extensions.Configuration;


namespace MonsterTradingCardsGame.UnitTests.Services
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<ISessionRepository> _sessionRepositoryMock;
        private Mock<IConfiguration> _configurationMock;
        private AuthenticationService _authenticationService;

        [SetUp]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _sessionRepositoryMock = new Mock<ISessionRepository>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(config => config["IsTesting"]).Returns("true");

            _authenticationService = new AuthenticationService(_userRepositoryMock.Object, _sessionRepositoryMock.Object, _configurationMock.Object);

        }

        [Test]
        public void VerifyCredentials_WithValidCredentials_ReturnsTrue()
        {
            // Arrange
            string hashedPassword = _authenticationService.HashPassword("testPassword");
            User user = new User { Username = "testUser", Password = hashedPassword };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("testUser")).Returns(user);

            _authenticationService = new AuthenticationService(_userRepositoryMock.Object, _sessionRepositoryMock.Object, _configurationMock.Object);

            // Act
            var result = _authenticationService.VerifyCredentials("testUser", "testPassword");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyCredentials_WithInvalidCredentials_ReturnsFalse()
        {
            // Arrange
            User user = new User { Username = "testUser", Password = "testPassword" };
            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("testUser")).Returns(It.IsAny<User>());

            // Act
            var result = _authenticationService.VerifyCredentials("testUser", "wrongPassword");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateToken_WithValidToken_ReturnsTrue()
        {
            // Arrange
            var user = new User { Username = "testUser" };
            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("testUser")).Returns(user);

            // Act
            var result = _authenticationService.ValidateToken("testUser-mtcgToken");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateToken_WithInvalidToken_ReturnsFalse()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("invalidUser")).Returns(It.IsAny<User>());

            // Act
            var result = _authenticationService.ValidateToken("invalidUser-mtcgToken");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetUserFromToken_WithValidToken_ReturnsUser()
        {
            // Arrange
            var user = new User { Username = "testUser" };
            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("testUser")).Returns(user);

            // Act
            var result = _authenticationService.GetUserFromToken("testUser-mtcgToken");

            // Assert
            Assert.That(result, Is.EqualTo(user));
        }

        [Test]
        public void GetUserFromToken_WithInvalidToken_ReturnsNull()
        {
            // Arrange
            // ... nothing to arrange here ?

            // Act
            User? result = _authenticationService.GetUserFromToken("invalidUser-mtcgToken");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GenerateToken_ReturnsCorrectToken() //user is always valid, because gets checked before
        {
            // Arrange
            var user = new User { Username = "testUser" };

            // Act
            var result = _authenticationService.GenerateToken(user);

            // Assert
            Assert.That(result, Is.EqualTo("testUser-mtcgToken"));
        }

    }
}
