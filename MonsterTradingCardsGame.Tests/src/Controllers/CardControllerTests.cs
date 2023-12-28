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

        //admin tries to create new packages with less than 5 cards
        [Test]
        public void CardController_CreatePackage_LessThan5Cards()
        {

            string payload = "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"Name\":\"WaterGoblin\", \"Damage\": 10.0}, {\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"Name\":\"Dragon\", \"Damage\": 50.0}, {\"Id\":\"e85e3976-7c86-4d06-9a80-641c2019a79f\", \"Name\":\"WaterSpell\", \"Damage\": 20.0}, {\"Id\":\"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\", \"Name\":\"Ork\", \"Damage\": 45.0}]";


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
            _mockHttpEventArguments.Verify(m => m.Reply(400, "You need to provide 5 cards"), Times.Once());
        }

        //at least one card in the package already exists
        [Test]
        public void CardController_CreatePackage_CardAlreadyExists()
        {

            string payload = "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"Name\":\"WaterGoblin\", \"Damage\": 10.0}, {\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"Name\":\"Dragon\", \"Damage\": 50.0}, {\"Id\":\"e85e3976-7c86-4d06-9a80-641c2019a79f\", \"Name\":\"WaterSpell\", \"Damage\": 20.0}, {\"Id\":\"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\", \"Name\":\"Ork\", \"Damage\": 45.0}, {\"Id\":\"dfdd758f-649c-40f9-ba3a-8657f4b3439f\", \"Name\":\"FireSpell\",    \"Damage\": 25.0}]";

            //mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/packages");
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(payload);
            _mockHttpEventArguments.Setup(m => m.User).Returns(new User() { Username = "admin" });

            //mock card repository
            _mockCardRepository.Setup(m => m.GetCardById(It.IsAny<string>())).Returns(new Card());
            _mockCardRepository.Setup(m => m.SavePackage(It.IsAny<List<Card>>())).Returns(true);

            // Act
            _cardController.CreatePackage(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(409, "At least one card in the packages already exists"), Times.Once());

        }

        //get cards
        [Test]
        public void CardController_GetUserCards()
        {
            // Arrange user with stack of cards
            User user = new User()
            {
                Username = "testuser",
                Stack = new List<Card>()
                {
                    new Card()
                    {
                        Id = "1",
                        Name = "testcard",
                        Damage = 10.0
                    },
                    new Card()
                    {
                        Id = "2",
                        Name = "testcard2",
                        Damage = 20.0
                    }
                }
            };

            // Arrange mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("GET");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/cards");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);

            // Act
            _cardController.GetCardsByUser(_mockHttpEventArguments.Object);

            string userStack = JsonSerializer.Serialize(user.Stack);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, userStack), Times.Once());
        }

        //get user deck
        [Test]
        public void CardController_GetUserDeck_DefaultJson()
        {
            // Arrange user with deck of cards
            User user = new User()
            {
                Username = "testuser",
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "1",
                        Name = "testcard",
                        Damage = 10.0
                    },
                    new Card()
                    {
                        Id = "2",
                        Name = "testcard2",
                        Damage = 20.0
                    }
                }
            };

            // Arrange mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("GET");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/deck");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);

            // Act
            _cardController.GetDeck(_mockHttpEventArguments.Object);

            string userDeck = JsonSerializer.Serialize(user.Deck);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, userDeck), Times.Once());
        }

        //test format plain
        [Test]
        public void CardController_GetUserDeck_Plain()
        {
            // Arrange user with deck of cards
            User user = new User()
            {
                Username = "testuser",
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "1",
                        Name = "testcard",
                        Damage = 10.0
                    },
                    new Card()
                    {
                        Id = "2",
                        Name = "testcard2",
                        Damage = 20.0
                    }
                }
            };

            // Arrange mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("GET");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/deck");
            _mockHttpEventArguments.Setup(m => m.QueryParameters).Returns(new Dictionary<string, string>() { { "format", "plain" } });
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);

            // Act
            _cardController.GetDeck(_mockHttpEventArguments.Object);

            string userDeck = string.Join("\n", user.Deck.Select(card => card.ToString()));

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, userDeck), Times.Once());
        }

        //test format json
        [Test]
        public void CardController_GetUserDeck_Json()
        {
            // Arrange user with deck of cards
            User user = new User()
            {
                Username = "testuser",
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "1",
                        Name = "testcard",
                        Damage = 10.0
                    },
                    new Card()
                    {
                        Id = "2",
                        Name = "testcard2",
                        Damage = 20.0
                    }
                }
            };

            // Arrange mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("GET");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/deck");
            _mockHttpEventArguments.Setup(m => m.QueryParameters).Returns(new Dictionary<string, string>() { { "format", "json" } });
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);

            // Act
            _cardController.GetDeck(_mockHttpEventArguments.Object);

            string userDeck = JsonSerializer.Serialize(user.Deck);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, userDeck), Times.Once());
        }

        //test configuring deck
        [Test]
        public void CardController_ConfigureDeck()
        {
            // user with empty deck
            User initialUser = new User()
            {
                Username = "testuser",
                Stack = {
                    new Card()
                    {
                        Id = "845f0dc7-37d0-426e-994e-43fc3ac83c08",
                        Name = "WaterGoblin",
                        Damage = 10.0
                    },
                    new Card()
                    {
                        Id = "99f8f8dc-e25e-4a95-aa2c-782823f36e2a",
                        Name = "Dragon",
                        Damage = 50.0
                    },
                    new Card()
                    {
                        Id = "e85e3976-7c86-4d06-9a80-641c2019a79f",
                        Name = "WaterSpell",
                        Damage = 20.0
                    },
                    new Card()
                    {
                        Id = "1cb6ab86-bdb2-47e5-b6e4-68c5ab389334",
                        Name = "Ork",
                        Damage = 45.0
                    },
                },
                Deck = new List<Card>(),
            };


            string payload = "[\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"e85e3976-7c86-4d06-9a80-641c2019a79f\", \"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\"]";

            // Arrange mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("PUT");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/deck");
            _mockHttpEventArguments.Setup(m => m.User).Returns(initialUser);
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(payload);


            // Act
            _cardController.ConfigureDeck(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, "The deck has been successfully configured."), Times.Once());
        }

        //test if user has not every card in the stack which he wants to configure in the deck
        [Test]
        public void CardController_ConfigureDeck_MissingCard()
        {
            // user with empty deck
            User initialUser = new User()
            {
                Username = "testuser",
                Stack = {
                    new Card()
                    {
                        Id = "845f0dc7-37d0-426e-994e-43fc3ac83c08",
                        Name = "WaterGoblin",
                        Damage = 10.0
                    },
                    new Card()
                    {
                        Id = "99f8f8dc-e25e-4a95-aa2c-782823f36e2a",
                        Name = "Dragon",
                        Damage = 50.0
                    },
                    new Card()
                    {
                        Id = "e85e3976-7c86-4d06-9a80-641c2019a79f",
                        Name = "WaterSpell",
                        Damage = 20.0
                    }
                },
                Deck = new List<Card>(),
            };


            string payload = "[\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"e85e3976-7c86-4d06-9a80-641c2019a79f\", \"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\"]";

            // Arrange mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("PUT");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/deck");
            _mockHttpEventArguments.Setup(m => m.User).Returns(initialUser);
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(payload);


            // Act
            _cardController.ConfigureDeck(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(403, "At least one of the provided cards does not belong to the user or is not available."), Times.Once());
        }

        //test if one card is locked in trade
        [Test]
        public void CardController_ConfigureDeck_CardLocked()
        {
            // user with empty deck
            User initialUser = new User()
            {
                Username = "testuser",
                Stack = {
                    new Card()
                    {
                        Id = "845f0dc7-37d0-426e-994e-43fc3ac83c08",
                        Name = "WaterGoblin",
                        Damage = 10.0
                    },
                    new Card()
                    {
                        Id = "99f8f8dc-e25e-4a95-aa2c-782823f36e2a",
                        Name = "Dragon",
                        Damage = 50.0
                    },
                    new Card()
                    {
                        Id = "e85e3976-7c86-4d06-9a80-641c2019a79f",
                        Name = "WaterSpell",
                        Damage = 20.0
                    },
                    new Card()
                    {
                        Id = "1cb6ab86-bdb2-47e5-b6e4-68c5ab389334",
                        Name = "Ork",
                        Damage = 45.0,
                        IsLocked = true
                    },
                },
                Deck = new List<Card>(),
            };


            string payload = "[\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"e85e3976-7c86-4d06-9a80-641c2019a79f\", \"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\"]";

            // Arrange mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("PUT");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/deck");
            _mockHttpEventArguments.Setup(m => m.User).Returns(initialUser);
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(payload);


            // Act
            _cardController.ConfigureDeck(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(403, "At least one of the provided cards does not belong to the user or is not available."), Times.Once());
        }

    }

}