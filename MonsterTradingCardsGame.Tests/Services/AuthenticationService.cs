using NUnit.Framework;
using Moq;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services;

namespace MonsterTradingCardsGame.Tests.Services
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private AuthenticationService _authenticationService;

        [SetUp]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authenticationService = new AuthenticationService(_userRepositoryMock.Object);
        }

        [Test]
        public void VerifyCredentials_WithValidCredentials_ReturnsTrue()
        {
            // Arrange
            var user = new User { Username = "testUser", Password = "testPassword" };
            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("testUser")).Returns(user);

            // Act
            var result = _authenticationService.VerifyCredentials("testUser", "testPassword");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyCredentials_WithInvalidCredentials_ReturnsFalse()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("testUser")).Returns((User)null);

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
            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("invalidUser")).Returns((User)null);

            // Act
            var result = _authenticationService.ValidateToken("invalidUser-mtcgToken");

            // Assert
            Assert.IsFalse(result);
        }

        // Additional tests for GetUserFromToken and GenerateToken can be added here
    }
}
