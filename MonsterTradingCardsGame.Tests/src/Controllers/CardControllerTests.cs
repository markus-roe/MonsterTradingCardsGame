using Moq;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using System.Text.Json;
using MonsterTradingCardsGame.Server;


namespace MonsterTradingCardsGame.UnitTests.Controllers
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
            _mockCardRepository.Setup(m => m.SavePackage(It.IsAny<List<Card>>())).Returns(It.IsAny<int>());

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
            _mockCardRepository.Setup(m => m.SavePackage(It.IsAny<List<Card>>())).Returns(It.IsAny<int>());

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
            _mockCardRepository.Setup(m => m.SavePackage(It.IsAny<List<Card>>())).Returns(It.IsAny<int>());

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
            _mockCardRepository.Setup(m => m.SavePackage(It.IsAny<List<Card>>())).Returns(It.IsAny<int>());

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
            _mockCardRepository.Setup(m => m.SavePackage(It.IsAny<List<Card>>())).Returns(It.IsAny<int>());

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


        //invalid request body
        [Test]
        public void CardController_ConfigureDeck_InvalidRequestBody()
        {
            // user with empty deck
            User user = new User()
            {
                Username = "testuser",
                Stack =
                {
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


            string payload = "invalid payload";

            // Arrange mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("PUT");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/deck");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);
            _mockHttpEventArguments.Setup(m => m.Payload).Returns(payload);


            // Act
            _cardController.ConfigureDeck(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(400, "The provided deck did not include the required amount of cards"), Times.Once());

        }

        [Test]
        public void CardController_BuyPackage()
        {
            // Arrange user with empty stack
            User user = new User()
            {
                Username = "testuser",
                Stack = new List<Card>(),
                Deck = new List<Card>(),
            };

            //available package
            List<Card> availablePackage = new List<Card>()
            {
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
                new Card()
                {
                    Id = "dfdd758f-649c-40f9-ba3a-8657f4b3439f",
                    Name = "FireSpell",
                    Damage = 25.0
                }
            };


            // Arrange mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/transactions");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);

            // Arrange mock card repository
            _mockCardRepository.Setup(m => m.GetCardPackage()).Returns(availablePackage);
            _mockCardRepository.Setup(m => m.SavePackageToUser(user, availablePackage)).Returns(true);

            // Act
            _cardController.BuyPackage(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, "Package and cards successfully bought"), Times.Once());
        }

        //test if user has not enough coins
        [Test]
        public void CardController_BuyPackage_NotEnoughCoins()
        {
            // Arrange user with empty stack
            User user = new User()
            {
                Username = "testuser",
                Stack = new List<Card>(),
                Deck = new List<Card>(),
                Coins = 4
            };

            //available package
            List<Card> availablePackage = new List<Card>()
            {
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
                new Card()
                {
                    Id = "dfdd758f-649c-40f9-ba3a-8657f4b3439f",
                    Name = "FireSpell",
                    Damage = 25.0
                }
            };

            // Arrange mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/transactions");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);

            // Arrange mock card repository
            _mockCardRepository.Setup(m => m.GetCardPackage()).Returns(availablePackage);
            _mockCardRepository.Setup(m => m.SavePackageToUser(user, availablePackage)).Returns(true);

            // Act
            _cardController.BuyPackage(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(403, "Not enough money for buying a card package"), Times.Once());
        }

        //test if no package is available
        [Test]
        public void CardController_BuyPackage_NoPackageAvailable()
        {
            // Arrange user with empty stack
            User user = new User()
            {
                Username = "testuser",
                Stack = new List<Card>(),
                Deck = new List<Card>(),
                Coins = 100
            };

            //available package
            List<Card> availablePackage = new List<Card>();

            // Arrange mock http event arguments
            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/transactions");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);

            // Arrange mock card repository
            _mockCardRepository.Setup(m => m.GetCardPackage()).Returns(availablePackage);
            _mockCardRepository.Setup(m => m.SavePackageToUser(user, availablePackage)).Returns(true);

            // Act
            _cardController.BuyPackage(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(404, "No card package available for buying"), Times.Once());
        }

        // MergeCards with not enough coins
        [Test]
        public void UserController_MergeCards_NotEnoughCoins()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Password = "testpassword",
                Name = "testuser",
                Bio = "testbio",
                Image = "testimage",
                Coins = 2
            };

            List<Card> cards = new List<Card>();
            cards.Add(new Card()
            {
                Id = "testid",
                Name = "testcard",
                Damage = 100,
                Element = ElementType.fire,
                Type = CardType.spell
            });

            cards.Add(new Card()
            {
                Id = "testid2",
                Name = "testcard2",
                Damage = 100,
                Element = ElementType.fire,
                Type = CardType.spell
            });

            user.Stack = cards;

            _mockUserRepository.Setup(m => m.GetUserByUsername(user.Username)).Returns(user);
            _mockCardRepository.Setup(m => m.GetCardsByUser(user)).Returns(cards);

            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/merge");


            // Act
            _cardController.MergeCards(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(400, "Not enough coins! You need 3 coins to merge cards."), Times.Once());
        }

        // MergeCards with not enough cards
        [Test]
        public void UserController_MergeCards_NotEnoughCards()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Password = "testpassword",
                Name = "testuser",
                Bio = "testbio",
                Image = "testimage",
                Coins = 3
            };

            List<Card> cards = new List<Card>();
            cards.Add(new Card()
            {
                Id = "testid",
                Name = "testcard",
                Damage = 100,
                Element = ElementType.fire,
                Type = CardType.spell
            });

            user.Stack = cards;

            _mockUserRepository.Setup(m => m.GetUserByUsername(user.Username)).Returns(user);
            _mockCardRepository.Setup(m => m.GetCardsByUser(user)).Returns(cards);

            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/merge");


            // Act
            _cardController.MergeCards(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(400, "Not enough cards. You need at least 2 cards to merge."), Times.Once());
        }

        /*
         * 
         * 
             /// <summary> This is a mandatory feature. It merges two random deck cards and if they are different element type.
    /// Creates a new one, replacing one random old card.
    /// Costs 3 coins </summary>
    [Route("POST", "merge")]
    public void MergeCards(IHttpServerEventArguments httpEventArguments)
    {
      try
      {
        User user = httpEventArguments.User;

        if (user.Coins < 3)
        {
          httpEventArguments.Reply(400, "Not enough coins! You need 3 coins to merge cards.");
          return;
        }

        List<Card> cards = user.Deck.ToList();

        if (cards.Count < 2)
        {
          httpEventArguments.Reply(400, "Not enough cards. You need at least 2 cards to merge.");
          return;
        }

        Card card1 = cards[new Random().Next(0, cards.Count)];

        cards.Remove(card1);

        Card card2 = cards[new Random().Next(0, cards.Count)];

        user.Coins -= 3;
        _userRepository.UpdateUser(user);

        if (card1.Element == card2.Element)
        {
          httpEventArguments.Reply(400, "No luck today! Cards has to be of different element type to merge.");
          return;
        }

        Card newCard = new Card
        {
          Id = Guid.NewGuid().ToString(),
          Name = $"{card1.Name} {card2.Name} Fusion",
          Type = (new Random().Next(0, 2) == 0) ? card1.Type : card2.Type,
          Element = (new Random().Next(0, 2) == 0) ? card1.Element : card2.Element,
          Damage = card1.Damage + card2.Damage,
          IsLocked = false,
        };

        // Save new card
        _cardRepository.SaveCard(newCard);

        // Randomly choose whether to remove card1 or card2
        if (new Random().Next(0, 2) == 0)
          _cardRepository.RemoveCardFromDeck(user, card1);
        else
          _cardRepository.RemoveCardFromDeck(user, card2);

        // Add new card to user
        _userRepository.SaveCardToUserDeck(user, newCard);

        string card1Name = card1.Name;
        string card2Name = card2.Name;
        string mergedName = $"{card1Name} {card2Name} Fusion";

        httpEventArguments.Reply(200, $"You got lucky! Cards {card1Name} and {card2Name} merged successfully. You got: {mergedName}");
      }
      catch (Exception ex)
      {
        httpEventArguments.Reply(500, $"Internal server error: {ex.Message}");
      }

    }

        */

        // MergeCards with success
        [Test]
        public void UserController_MergeCards_Success()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Password = "testpassword",
                Name = "testuser",
                Bio = "testbio",
                Image = "testimage",
                Coins = 3
            };

            List<Card> cards = new List<Card>();
            cards.Add(new Card()
            {
                Id = "testid",
                Name = "testcard",
                Damage = 100,
                Element = ElementType.fire,
                Type = CardType.spell
            });

            cards.Add(new Card()
            {
                Id = "testid2",
                Name = "testcard2",
                Damage = 100,
                Element = ElementType.water,
                Type = CardType.spell
            });

            user.Deck = cards;

            _mockUserRepository.Setup(m => m.GetUserByUsername(user.Username)).Returns(user);
            _mockCardRepository.Setup(m => m.GetCardsByUser(user)).Returns(cards);

            _mockHttpEventArguments.Setup(m => m.Method).Returns("POST");
            _mockHttpEventArguments.Setup(m => m.User).Returns(user);
            _mockHttpEventArguments.Setup(m => m.Path).Returns("/merge");


            // Act
            _cardController.MergeCards(_mockHttpEventArguments.Object);

            // Assert
            _mockHttpEventArguments.Verify(m => m.Reply(200, "You got lucky! Cards testcard and testcard2 merged successfully. You got: testcard testcard2 Fusion"), Times.Once());
        }

    }

}