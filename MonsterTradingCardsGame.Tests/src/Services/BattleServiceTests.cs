using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using Moq;

namespace MonsterTradingCardsGame.Tests.Services
{


    [TestFixture]
    internal class BattleServiceTests
    {

        private Mock<ICardRepository> _mockCardRepository;
        private Mock<IUserRepository> _mockUserRepository;

        private BattleService _battleService;

        //Setup for the tests
        [SetUp]
        public void Setup()
        {
            _mockCardRepository = new Mock<ICardRepository>();
            _mockUserRepository = new Mock<IUserRepository>();

            _battleService = new BattleService(_mockCardRepository.Object, _mockUserRepository.Object);

        }

        [Test]
        public void StartBattle_UserHasNoCards_ReturnsEmptyCardsMessage()
        {
            // Arrange
            User user1 = new User()
            {
                Username = "testuser1",
                Elo = 100,
                Deck = new List<Card>()
            };


            User user2 = new User()
            {
                Username = "testuser2",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "abcd",
                        Name = "testcard1",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    },
                    new Card()
                    {
                        Id = "efgh",
                        Name = "testcard2",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };


            // Act
            string battleLog = _battleService.StartBattle(user1, user2);

            // Assert
            Assert.That(battleLog, Is.EqualTo($"{user1.Username} has no playable cards left!"));

        }

        [Test]
        public void StartBattle_UserHasLockedCard_ReturnsEmptyCardsMessage()
        {
            // Arrange
            User user1 = new User()
            {
                Username = "testuser1",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "abcd",
                        Name = "testcard1",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = true
                    }
                }
            };


            User user2 = new User()
            {
                Username = "testuser2",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "efgh",
                        Name = "testcard2",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };


            // Act
            string battleLog = _battleService.StartBattle(user1, user2);

            // Assert
            Assert.That(battleLog, Is.EqualTo($"{user1.Username} has no playable cards left!"));
        }

        [Test]
        public void StartBattle_SpecialInteraction_GoblinVsDragon_ReturnsGoblinWinsMessage()
        {
            // Arrange
            User user1 = new User()
            {
                Username = "testuser1",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "abcd",
                        Name = "Goblin",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };

            User user2 = new User()
            {
                Username = "testuser2",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "efgh",
                        Name = "Dragon",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };

            // Act
            string battleLog = _battleService.StartBattle(user1, user2);

            // Assert
            StringAssert.Contains("Special Interaction: Goblins are too afraid of Dragons to attack.", battleLog);

        }

        [Test]  
        public void StartBattle_SpecialInteraction_WizardVsOrc_ReturnsWizardWinsMessage()
        {
            // Arrange
            User user1 = new User()
            {
                Username = "testuser1",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "abcd",
                        Name = "Wizard",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };

            User user2 = new User()
            {
                Username = "testuser2",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "efgh",
                        Name = "Orc",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };

            // Act
            string battleLog = _battleService.StartBattle(user1, user2);

            // Assert
            StringAssert.Contains("Special Interaction: Wizards can control Orcs, making them unable to attack.", battleLog);

        }

        [Test]
        public void StartBattle_SpecialInteraction_KnightVsWaterSpell_ReturnsKnightWinsMessage()
        {
            // Arrange
            User user1 = new User()
            {
                Username = "testuser1",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "abcd",
                        Name = "Knight",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };

            User user2 = new User()
            {
                Username = "testuser2",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "efgh",
                        Name = "WaterSpell",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };

            // Act
            string battleLog = _battleService.StartBattle(user1, user2);

            // Assert
            StringAssert.Contains("Special Interaction: The heavy armor of Knights makes them drown instantly in Water Spells.", battleLog);

        }

        [Test]
        public void StartBattle_SpecialInteraction_KrakenVsSpell_ReturnsKrakenWinsMessage()
        {
            // Arrange
            User user1 = new User()
            {
                Username = "testuser1",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "abcd",
                        Name = "Kraken",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };

            User user2 = new User()
            {
                Username = "testuser2",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "efgh",
                        Name = "Spell",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };

            // Act
            string battleLog = _battleService.StartBattle(user1, user2);

            // Assert
            StringAssert.Contains("Special Interaction: Krakens are immune against all spells.", battleLog);

        }

        [Test]
        public void StartBattle_SpecialInteraction_FireElfVsDragon_ReturnsFireElfWinsMessage()
        {
            // Arrange
            User user1 = new User()
            {
                Username = "testuser1",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "abcd",
                        Name = "FireElf",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };

            User user2 = new User()
            {
                Username = "testuser2",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "efgh",
                        Name = "Dragon",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };

            // Act
            string battleLog = _battleService.StartBattle(user1, user2);

            // Assert
            StringAssert.Contains("Special Interaction: Fire Elves can evade attacks from Dragons due to their familiarity.", battleLog);

        }


        //test for AdjustDamageBasedOnElement
        [Test]
        public void AdjustDamageBasedOnElement_WaterVsFire_ReturnsDoubleDamage()
        {
            // Arrange
            Card card1 = new Card()
            {
                Id = "abcd",
                Name = "testcard1",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.water,
                IsLocked = false
            };

            Card card2 = new Card()
            {
                Id = "efgh",
                Name = "testcard2",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.fire,
                IsLocked = false
            };

            // Act
            bool result = _battleService.AdjustDamageBasedOnElement(card1, card2.Element);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(card1.Damage, Is.EqualTo(200));

        }

        [Test]
        public void AdjustDamageBasedOnElement_FireVsNormal_ReturnsDoubleDamage()
        {
            // Arrange
            Card card1 = new Card()
            {
                Id = "abcd",
              
                Name = "testcard1",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.fire,
                IsLocked = false
            };

            Card card2 = new Card()
            {
                Id = "efgh",
                Name = "testcard2",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.normal,
                IsLocked = false
            };

            // Act
            bool result = _battleService.AdjustDamageBasedOnElement(card1, card2.Element);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(card1.Damage, Is.EqualTo(200));

        }

        [Test]
        public void AdjustDamageBasedOnElement_NormalVsWater_ReturnsDoubleDamage()
        {
            // Arrange
            Card card1 = new Card()
            {
                Id = "abcd",
                Name = "testcard1",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.normal,
                IsLocked = false
            };

            Card card2 = new Card()
            {
                Id = "efgh",
                Name = "testcard2",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.water,
                IsLocked = false
            };

            // Act
            bool result = _battleService.AdjustDamageBasedOnElement(card1, card2.Element);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(card1.Damage, Is.EqualTo(200));

        }

        [Test]
        public void AdjustDamageBasedOnElement_FireVsWater_ReturnsHalfDamage()
        {
            // Arrange
            Card card1 = new Card()
            {
                Id = "abcd",
                Name = "testcard1",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.fire,
                IsLocked = false
            };

            Card card2 = new Card()
            {
                Id = "efgh",
                Name = "testcard2",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.water,
                IsLocked = false
            };

            // Act
            bool result = _battleService.AdjustDamageBasedOnElement(card1, card2.Element);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(card1.Damage, Is.EqualTo(50));

        }

        [Test]
        public void AdjustDamageBasedOnElement_NormalVsFire_ReturnsHalfDamage()
        {
            // Arrange
            Card card1 = new Card()
            {
                Id = "abcd",
                Name = "testcard1",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.normal,
                IsLocked = false
            };

            Card card2 = new Card()
            {
                Id = "efgh",
                Name = "testcard2",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.fire,
                IsLocked = false
            };

            // Act
            bool result = _battleService.AdjustDamageBasedOnElement(card1, card2.Element);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(card1.Damage, Is.EqualTo(50));

        }

        [Test]
        public void AdjustDamageBasedOnElement_WaterVsNormal_ReturnsHalfDamage()
        {
            // Arrange
            Card card1 = new Card()
            {
                Id = "abcd",
                Name = "testcard1",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.water,
                IsLocked = false
            };

            Card card2 = new Card()
            {
                Id = "efgh",
                Name = "testcard2",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.normal,
                IsLocked = false
            };

            // Act
            bool result = _battleService.AdjustDamageBasedOnElement(card1, card2.Element);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(card1.Damage, Is.EqualTo(50));

        }

        [Test]
        public void StartBattle_SpellEffect_AppliesDamage()
        {
            // Arrange
            User user1 = new User()
            {
                Username = "testuser1",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "abcd",
                        Name = "testcard1",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.water,
                        IsLocked = false
                    }
                }
            };


            User user2 = new User()
            {
                Username = "testuser2",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "efgh",
                        Name = "testcard2",
                        Damage = 100,
                        Type = CardType.monster,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };


            // Act
            string battleLog = _battleService.StartBattle(user1, user2);

            // Assert
            StringAssert.Contains("Spell effect applied to testcard1: water vs fire", battleLog);
            StringAssert.Contains(" > Original Damage: 100, Multiplier Applied, New Damage: 200", battleLog);

        }

        [Test]
        public void StartBattle_SpellEffect_AppliesDamageToBothCards()
        {
            // Arrange
            User user1 = new User()
            {
                Username = "testuser1",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "abcd",
                        Name = "testcard1",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.water,
                        IsLocked = false
                    }
                }
            };


            User user2 = new User()
            {
                Username = "testuser2",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "efgh",
                        Name = "testcard2",
                        Damage = 100,
                        Type = CardType.spell,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };


            // Act
            string battleLog = _battleService.StartBattle(user1, user2);


            // Assert
            StringAssert.Contains("Spell effect applied to testcard1: water vs fire", battleLog);
            StringAssert.Contains(" > Original Damage: 100, Multiplier Applied, New Damage: 200", battleLog);
            StringAssert.Contains("Spell effect applied to testcard2: fire vs water", battleLog);
            StringAssert.Contains(" > Original Damage: 100, Multiplier Applied, New Damage: 50", battleLog);

        }

        [Test]
        public void CompareCardDamage_Card1HasHigherDamage_ReturnsCard1()
        {
            // Arrange
            Card card1 = new Card()
            {
                Id = "abcd",
                Name = "testcard1",
                Damage = 200,
                Type = CardType.spell,
                Element = ElementType.water,
                IsLocked = false
            };

            Card card2 = new Card()
            {
                Id = "efgh",
                Name = "testcard2",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.fire,
                IsLocked = false
            };

            // Act
            Card? result = _battleService.CompareCardDamage(card1, card2);

            // Assert
            Assert.That(result, Is.EqualTo(card1));

        }

        [Test]
        public void CompareCardDamage_Card2HasHigherDamage_ReturnsCard2()
        {
            // Arrange
            Card card1 = new Card()
            {
                Id = "abcd",
                Name = "testcard1",
                Damage = 100,
                Type = CardType.spell,
                Element = ElementType.water,
                IsLocked = false
            };

            Card card2 = new Card()
            {
                Id = "efgh",
                Name = "testcard2",
                Damage = 200,
                Type = CardType.spell,
                Element = ElementType.fire,
                IsLocked = false
            };

            // Act
            Card? result = _battleService.CompareCardDamage(card1, card2);

            // Assert
            Assert.That(result, Is.EqualTo(card2));

        }


    }
}
