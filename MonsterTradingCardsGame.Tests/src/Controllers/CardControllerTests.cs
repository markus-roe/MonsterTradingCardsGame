using Moq;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using System.Text.Json;
using MonsterTradingCardsGame.Server;


namespace MonsterTradingCardsGame.Tests.Controllers
{

    [TestFixture]
    public class CardControllerTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<ICardRepository> _mockCardRepository;
        private Mock<IHttpServerEventArguments> _mockHttpEventArguments;

        private CardController _cardController;

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockCardRepository = new Mock<ICardRepository>();
            _mockHttpEventArguments = new Mock<IHttpServerEventArguments>();

            _cardController = new CardController(
                _mockCardRepository.Object,
                _mockUserRepository.Object);
        }

    }

}