using NUnit.Framework;
using Moq;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MonsterTradingCardsGame.Tests.Controllers
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<UserRepository> mockUserRepository;
        private UserController userController;

        [SetUp]
        public void Setup()
        {
            mockUserRepository = new Mock<UserRepository>();
            userController = new UserController(mockUserRepository.Object);
        }

        [Test]
        public void GetUserByUsername_UserExists_ReturnsUser()
        {
            // Arrange
            var expectedUser = new User { Username = "TestUser" };
            mockUserRepository.Setup(repo => repo.GetUserByUsername("TestUser")).Returns(expectedUser);

            var httpEventArguments = new HttpServerEventArguments(null, string.Empty);
            var parameters = new Dictionary<string, string> { { "username", "TestUser" } };

            // Act
            userController.GetUserByUsername(httpEventArguments, parameters);

            // Assert
            Assert.AreEqual(200, httpEventArguments.ResponseStatusCode);
            Assert.AreEqual(JsonSerializer.Serialize(expectedUser), httpEventArguments.ResponseBody);
        }

        [Test]
        public void GetUserByUsername_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            mockUserRepository.Setup(repo => repo.GetUserByUsername("NonExistingUser")).Returns((User)null);

            var httpEventArguments = new HttpServerEventArguments(null, string.Empty);
            var parameters = new Dictionary<string, string> { { "username", "NonExistingUser" } };

            // Act
            userController.GetUserByUsername(httpEventArguments, parameters);

            // Assert
            Assert.AreEqual(404, httpEventArguments.ResponseStatusCode);
            Assert.AreEqual("User not found.", httpEventArguments.ResponseBody);
        }

        [Test]
        public void GetUserByUsername_ErrorOccurs_ReturnsServerError()
        {
            // Arrange
            mockUserRepository.Setup(repo => repo.GetUserByUsername(It.IsAny<string>())).Throws(new Exception());

            var httpEventArguments = new HttpServerEventArguments(null, string.Empty);
            var parameters = new Dictionary<string, string> { { "username", "AnyUser" } };

            // Act
            userController.GetUserByUsername(httpEventArguments, parameters);

            // Assert
            Assert.AreEqual(500, httpEventArguments.ResponseStatusCode);
            Assert.AreEqual("Internal server error.", httpEventArguments.ResponseBody);
        }
    }
}
