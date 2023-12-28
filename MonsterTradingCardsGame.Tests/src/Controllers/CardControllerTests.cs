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

        //admin creates new packages
        [Test]
        public void CardController_CreatePackage()
        {

            string payload = "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"Name\":\"WaterGoblin\", \"Damage\": 10.0}, {\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"Name\":\"Dragon\", \"Damage\": 50.0}, {\"Id\":\"e85e3976-7c86-4d06-9a80-641c2019a79f\", \"Name\":\"WaterSpell\", \"Damage\": 20.0}, {\"Id\":\"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\", \"Name\":\"Ork\", \"Damage\": 45.0}, {\"Id\":\"dfdd758f-649c-40f9-ba3a-8657f4b3439f\", \"Name\":\"FireSpell\",    \"Damage\": 25.0}]";

            User user = new User()
            {
                Username = "admin"
            };

            //mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/packages");
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(payload);
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);

            //mock card repository
            _mockCardRepository.Setup(m => m.GetCardById(It.IsAny<string>())).Returns((Card)null);
            _mockCardRepository.Setup(m => m.SavePackage(It.IsAny<List<Card>>())).Returns(true);

            // Act
            _cardController.CreatePackage(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, "Package and cards successfully created"), Times.Once());

        }

        //non admin tries to create new packages
        [Test]
        public void CardController_CreatePackage_NonAdmin()
        {

            string payload = "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"Name\":\"WaterGoblin\", \"Damage\": 10.0}, {\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"Name\":\"Dragon\", \"Damage\": 50.0}, {\"Id\":\"e85e3976-7c86-4d06-9a80-641c2019a79f\", \"Name\":\"WaterSpell\", \"Damage\": 20.0}, {\"Id\":\"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\", \"Name\":\"Ork\", \"Damage\": 45.0}, {\"Id\":\"dfdd758f-649c-40f9-ba3a-8657f4b3439f\", \"Name\":\"FireSpell\",    \"Damage\": 25.0}]";

            //mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/packages");
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(payload);
            _mockHttpEventArguments.Setup(m => m.User).Returns(new User() { Username = "notanadmin" });

            //mock card repository
            _mockCardRepository.Setup(m => m.GetCardById(It.IsAny<string>())).Returns((Card)null);
            _mockCardRepository.Setup(m => m.SavePackage(It.IsAny<List<Card>>())).Returns(true);

            // Act
            _cardController.CreatePackage(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(403, "You are not allowed to create packages"), Times.Once());

        }

        //admin tries to create new packages with invalid payload
        [Test]
        public void CardController_CreatePackage_InvalidPayload()
        {

            string payload = "invalid payload";

            //mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/packages");
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(payload);
            _mockHttpEventArguments.Setup(m => m.User).Returns(new User() { Username = "admin" });

            //mock card repository
            _mockCardRepository.Setup(m => m.GetCardById(It.IsAny<string>())).Returns((Card)null);
            _mockCardRepository.Setup(m => m.SavePackage(It.IsAny<List<Card>>())).Returns(true);

            // Act
            _cardController.CreatePackage(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(400, "Payload is not a valid list of cards"), Times.Once());
        }

    }

}