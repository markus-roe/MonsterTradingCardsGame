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
using System.Net.Sockets;

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
            var cardRepositoryMock = new Mock<ICardRepository>(); // Add this line
            userController = new UserController(userRepositoryMock.Object, authServiceMock.Object, cardRepositoryMock.Object); // Update this line
            httpEventArgumentsMock = new Mock<HttpServerEventArguments>(new TcpClient(), string.Empty);
        }

        [Test]
        public void GetAll_WhenCalled_ReturnsAllUsers()
        {
            // Arrange
            var mockUsers = new List<User> { new User { Username = "TestUser1" }, new User { Username = "TestUser2" } };
            userRepositoryMock.Setup(repo => repo.GetAll()).Returns(mockUsers);



            // Act
            userController.GetAll(httpEventArgumentsMock.Object, new Dictionary<string, string>());

            // Assert
            AssertResponseContent(httpEventArgumentsMock.Object, 200, mockUsers.Count);
        }


        [Test]
        public void GetUserByUsername_VariousScenarios_Returns404()
        {
            // Arrange
            var expectedUser = new User { Username = "ExistingUser" };
            userRepositoryMock.Setup(repo => repo.GetUserByUsername("ExistingUser")).Returns(expectedUser);

            // Act
            userController.GetUserByUsername(httpEventArgumentsMock.Object, new Dictionary<string, string> { { "username", "ExistingUser" } });

            // Assert
            Assert.That(httpEventArgumentsMock.Object.ResponseStatusCode, Is.EqualTo(404));
        }

        [Test]
        public void GetUserByUsername_VariousScenarios_Returns200()
        {
            // Arrange
            var expectedUser = new User { Username = "ExistingUser" };
            userRepositoryMock.Setup(repo => repo.GetUserByUsername("ExistingUser")).Returns(expectedUser);

            // Act
            userController.GetUserByUsername(httpEventArgumentsMock.Object, new Dictionary<string, string> { { "username", "ExistingUser" } });

            // Assert
            Assert.That(httpEventArgumentsMock.Object.ResponseStatusCode, Is.EqualTo(200));
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
