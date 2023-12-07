using Moq;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using System.Text.Json;
using NUnit.Framework;
using MonsterTradingCardsGame.Services.Interfaces;
using MonsterTradingCardsGame.Server;
using System;
using System.Collections.Generic;

namespace MonsterTradingCardsGame.Tests.Controllers
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserRepository> userRepositoryMock;
        private Mock<IAuthenticationService> authServiceMock;
        private UserController userController;
        private Mock<HttpServerEventArguments> httpEventArgumentsMock;

        [SetUp]
        public void SetUp()
        {
            userRepositoryMock = new Mock<IUserRepository>();
            authServiceMock = new Mock<IAuthenticationService>();
            userController = new UserController(userRepositoryMock.Object, authServiceMock.Object);
            httpEventArgumentsMock = new Mock<HttpServerEventArguments>(null, string.Empty);
        }

        [Test]
        public void GetAll_WhenCalled_ReturnsAllUsers()
        {
            // Arrange
            var mockUsers = new List<User> { new User { Username = "TestUser1" }, new User { Username = "TestUser2" } };
            userRepositoryMock.Setup(repo => repo.GetAll()).Returns(mockUsers);

            var httpEventArguments = new HttpServerEventArguments(null, string.Empty)
            {
                Payload = JsonSerializer.Serialize(mockUsers) // This should now work
            };

            // Act
            userController.GetAll(httpEventArguments, new Dictionary<string, string>());

            // Assert
            AssertResponseContent(httpEventArguments, 200, mockUsers.Count);
        }


        [TestCase("ExistingUser", 200)]
        [TestCase("NonExistingUser", 404)]
        public void GetUserByUsername_VariousScenarios_ReturnsAppropriateResponse(string username, int expectedStatusCode)
        {
            // Arrange
            var expectedUser = new User { Username = "ExistingUser" };
            userRepositoryMock.Setup(repo => repo.GetUserByUsername("ExistingUser")).Returns(expectedUser);
            userRepositoryMock.Setup(repo => repo.GetUserByUsername("NonExistingUser")).Returns((User)null);

            // Act
            userController.GetUserByUsername(httpEventArgumentsMock.Object, new Dictionary<string, string> { { "username", username } });

            // Assert
            Assert.That(httpEventArgumentsMock.Object.ResponseStatusCode, Is.EqualTo(expectedStatusCode));
        }

        [Test]
        public void GetUserByUsername_RepositoryThrowsException_ReturnsServerError()
        {
            // Arrange
            userRepositoryMock.Setup(repo => repo.GetUserByUsername(It.IsAny<string>())).Throws(new Exception());

            // Act
            userController.GetUserByUsername(httpEventArgumentsMock.Object, new Dictionary<string, string> { { "username", "AnyUser" } });

            // Assert
            Assert.That(httpEventArgumentsMock.Object.ResponseStatusCode, Is.EqualTo(500));
        }

        private void AssertResponseContent(HttpServerEventArguments httpEventArguments, int expectedStatusCode, int expectedCount)
        {
            Assert.That(httpEventArguments.ResponseStatusCode, Is.EqualTo(expectedStatusCode));

            if (!string.IsNullOrWhiteSpace(httpEventArguments.Payload))
            {
                var response = JsonSerializer.Deserialize<List<User>>(httpEventArguments.Payload);
                Assert.That(response.Count, Is.EqualTo(expectedCount));
            }
            else
            {
                Assert.Fail("Response content is empty or invalid.");
            }
        }


    }
}
