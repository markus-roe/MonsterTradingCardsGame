using Moq;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using System.Text.Json;
using MonsterTradingCardsGame.Services.Interfaces;
using MonsterTradingCardsGame.Server;
using System.Net.Sockets;


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




    }

}