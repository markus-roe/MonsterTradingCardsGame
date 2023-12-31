using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;
using MonsterTradingCardsGame.Utils.UserStats;
using Moq;

namespace MonsterTradingCardsGame.Tests.Repositories
{

    [TestFixture]
    internal class CardRepositoryTests
    {

        private CardRepository _cardRepository;

        //Setup for the tests
        [SetUp]
        public void Setup()
        {
            _cardRepository = new CardRepository();
        }

        //GetCardTypeFromName
        [Test]
        public void GetCardTypeFromName_NameContainsSpell_ReturnsSpell()
        {
            // Arrange
            string cardName = "Spell";

            // Act
            CardType cardType = _cardRepository.GetCardTypeFromName(cardName);

            // Assert
            Assert.AreEqual(CardType.spell, cardType);
        }

        [Test]
        public void GetCardTypeFromName_NameDoesNotContainSpell_ReturnsMonster()
        {
            // Arrange
            string cardName = "Monster";

            // Act
            CardType cardType = _cardRepository.GetCardTypeFromName(cardName);

            // Assert
            Assert.AreEqual(CardType.monster, cardType);
        }

        //GetCardElementFromName
        [Test]
        public void GetCardElementFromName_NameContainsFire_ReturnsFire()
        {
            // Arrange
            string cardName = "Fire";

            // Act
            ElementType elementType = _cardRepository.GetCardElementFromName(cardName);

            // Assert
            Assert.AreEqual(ElementType.fire, elementType);
        }

        [Test]
        public void GetCardElementFromName_NameContainsWater_ReturnsWater()
        {
            // Arrange
            string cardName = "Water";

            // Act
            ElementType elementType = _cardRepository.GetCardElementFromName(cardName);

            // Assert
            Assert.AreEqual(ElementType.water, elementType);
        }

        [Test]
        public void GetCardElementFromName_NameDoesNotContainFireOrWater_ReturnsNormal()
        {
            // Arrange
            string cardName = "Normal";

            // Act
            ElementType elementType = _cardRepository.GetCardElementFromName(cardName);

            // Assert
            Assert.AreEqual(ElementType.normal, elementType);
        }


        //save package
        [Test]
        public void SavePackage_PackageSaved_ReturnsPackageId()
        {
            // Arrange
            List<Card> cards = new List<Card>();

            Card card1 = new Card()
            {
                Name = "Fire Ork",
                Damage = 10,
                Element = ElementType.fire,
                Type = CardType.monster,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card1);

            Card card2 = new Card()
            {
                Name = "Water Spell",
                Damage = 5,
                Element = ElementType.water,
                Type = CardType.spell,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card2);

            Card card3 = new Card()
            {
                Name = "Monster",
                Damage = 8,
                Element = ElementType.normal,
                Type = CardType.monster,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card3);

            Card card4 = new Card()
            {
                Name = "Water Spell",
                Damage = 7,
                Element = ElementType.water,
                Type = CardType.spell,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card4);

            Card card5 = new Card()
            {
                Name = "Fire Monster",
                Damage = 9,
                Element = ElementType.fire,
                Type = CardType.monster,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card5);

            // Act
            int? packageId = _cardRepository.SavePackage(cards);

            // Assert
            Assert.IsNotNull(packageId);

            //cleanup
            _cardRepository.DeletePackageById(packageId.Value);
        }

        [Test]
        public void SavePackage_PackageNotSaved_ReturnsNull()
        {
            // Act
            int? packageId = _cardRepository.SavePackage(null);

            // Assert
            Assert.IsNull(packageId);
        }

        //test delete package by id
        [Test]
        public void DeletePackageById_PackageDeleted_ReturnsTrue()
        {
            // Arrange
            List<Card> cards = new List<Card>();

            Card card1 = new Card()
            {
                Name = "Fire Ork",
                Damage = 10,
                Element = ElementType.fire,
                Type = CardType.monster,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card1);

            Card card2 = new Card()
            {
                Name = "Water Spell",
                Damage = 5,
                Element = ElementType.water,
                Type = CardType.spell,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card2);

            Card card3 = new Card()
            {
                Name = "Monster",
                Damage = 8,
                Element = ElementType.normal,
                Type = CardType.monster,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card3);

            Card card4 = new Card()
            {
                Name = "Water Spell",
                Damage = 7,
                Element = ElementType.water,
                Type = CardType.spell,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card4);

            Card card5 = new Card()
            {
                Name = "Fire Monster",
                Damage = 9,
                Element = ElementType.fire,
                Type = CardType.monster,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card5);

            int? packageId = _cardRepository.SavePackage(cards);

            // Act
            bool result = _cardRepository.DeletePackageById(packageId.Value);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void DeletePackageById_PackageNotDeleted_ReturnsFalse()
        {
            // Act
            bool result = _cardRepository.DeletePackageById(-1);

            // Assert
            Assert.IsFalse(result);
        }

        //test get card package
        [Test]
        public void GetCardPackage_PackageReturned_ReturnsListOfCards()
        {
            // Arrange
            List<Card> cards = new List<Card>();

            Card card1 = new Card()
            {
                Name = "Fire Ork",
                Damage = 10,
                Element = ElementType.fire,
                Type = CardType.monster,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card1);

            Card card2 = new Card()
            {
                Name = "Water Spell",
                Damage = 5,
                Element = ElementType.water,
                Type = CardType.spell,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card2);

            Card card3 = new Card()
            {
                Name = "Monster",
                Damage = 8,
                Element = ElementType.normal,
                Type = CardType.monster,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card3);

            Card card4 = new Card()
            {
                Name = "Water Spell",
                Damage = 7,
                Element = ElementType.water,
                Type = CardType.spell,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card4);

            Card card5 = new Card()
            {
                Name = "Fire Monster",
                Damage = 9,
                Element = ElementType.fire,
                Type = CardType.monster,
                Id = Guid.NewGuid().ToString()
            };
            cards.Add(card5);

            int? packageId = _cardRepository.SavePackage(cards);

            // Act
            List<Card> package = _cardRepository.GetCardPackage();

            // Assert
            Assert.IsNotNull(package);

            //cleanup
            _cardRepository.DeletePackageById(packageId.Value);
        }


        //test get card by id
        [Test]
        public void GetCardById_CardReturned_ReturnsCard()
        {
            // Arrange
            Card card = new Card()
            {
                Name = "Fire Ork",
                Damage = 10,
                Element = ElementType.fire,
                Type = CardType.monster,
                Id = Guid.NewGuid().ToString()
            };

            _cardRepository.SaveCard(card);

            // Act
            Card? cardFromDb = _cardRepository.GetCardById(card.Id);

            // Assert
            Assert.IsNotNull(cardFromDb);

        }


    }

}