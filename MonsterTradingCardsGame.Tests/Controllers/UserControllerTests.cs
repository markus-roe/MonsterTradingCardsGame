using Moq;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using System.Text.Json;
using NUnit.Framework;

namespace MonsterTradingCardsGame.Tests.Controllers
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserRepository> mockUserRepository;
        private UserController userController;
        private MockHttpServerEventArguments httpEventArguments;

        [SetUp]
        public void Setup()
        {
            mockUserRepository = new Mock<IUserRepository>();
            userController = new UserController(mockUserRepository.Object);
            httpEventArguments = new MockHttpServerEventArguments();
        }

        [Test]
        public void GetAll_WhenCalled_ReturnsAllUsers()
        {
            // Arrange
            var mockUsers = new List<User> { new User { Username = "TestUser1" }, new User { Username = "TestUser2" } };
            mockUserRepository.Setup(repo => repo.GetAll()).Returns(mockUsers);

            // Act
            userController.GetAll(httpEventArguments, new Dictionary<string, string>());

            // Assert
            AssertResponseContent(httpEventArguments.ResponseContent, 200, mockUsers.Count);
        }

        [TestCase("ExistingUser", 200)]
        [TestCase("NonExistingUser", 404)]
        public void GetUserByUsername_VariousScenarios_ReturnsAppropriateResponse(string username, int expectedStatusCode)
        {
            // Arrange
            var expectedUser = new User { Username = "ExistingUser" };
            mockUserRepository.Setup(repo => repo.GetUserByUsername("ExistingUser")).Returns(expectedUser);
            mockUserRepository.Setup(repo => repo.GetUserByUsername("NonExistingUser")).Returns((User)null);

            // Act
            userController.GetUserByUsername(httpEventArguments, new Dictionary<string, string> { { "username", username } });

            // Assert
            Assert.That(httpEventArguments.ResponseStatusCode, Is.EqualTo(expectedStatusCode));
        }

        [Test]
        public void GetUserByUsername_RepositoryThrowsException_ReturnsServerError()
        {
            // Arrange
            mockUserRepository.Setup(repo => repo.GetUserByUsername(It.IsAny<string>())).Throws(new Exception());

            // Act
            userController.GetUserByUsername(httpEventArguments, new Dictionary<string, string> { { "username", "AnyUser" } });

            // Assert
            Assert.That(httpEventArguments.ResponseStatusCode, Is.EqualTo(500));
        }

        private void AssertResponseContent(string responseContent, int expectedStatusCode, int expectedCount)
        {
            Assert.That(httpEventArguments.ResponseStatusCode, Is.EqualTo(expectedStatusCode));
            var response = JsonSerializer.Deserialize<List<User>>(responseContent);
            Assert.That(response.Count, Is.EqualTo(expectedCount));
        }
    }
}
