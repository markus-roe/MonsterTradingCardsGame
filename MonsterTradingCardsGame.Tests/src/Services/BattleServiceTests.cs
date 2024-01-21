using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using Moq;

/*
 * 
 using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;

public class BattleService : IBattleService
{
    private readonly Random _random = new Random();
    private readonly Dictionary<(ElementType, ElementType), double> _elementalEffectiveness = new()
    {
        { (ElementType.water, ElementType.fire), 2.0 },
        { (ElementType.fire, ElementType.normal), 2.0 },
        { (ElementType.normal, ElementType.water), 2.0 },
        { (ElementType.fire, ElementType.water), 0.5 },
        { (ElementType.normal, ElementType.fire), 0.5 },
        { (ElementType.water, ElementType.normal), 0.5 },
    };
    private List<string> _battleLog = new List<string>();

    private ICardRepository _cardRepository;
    private IUserRepository _userRepository;

    public BattleService(ICardRepository cardRepository, IUserRepository userRepository)
    {
        _cardRepository = cardRepository;
        _userRepository = userRepository;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Starts a battle between two users and logs the battle progress.
    /// </summary>
    public string StartBattle(User user1, User user2)
    {
        InitializeBattle();
        int round = 1;
        while (round <= 100 && !CheckForEmptyDecks(user1, user2))
        {
            ExecuteRound(user1, user2, round);
            round++;
        }

        DetermineBattleOutcome(user1, user2);

        return string.Join(Environment.NewLine, _battleLog);
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Determines the outcome of the battle and updates the battle log accordingly.
    /// </summary>
    private void DetermineBattleOutcome(User user1, User user2)
    {
        if (user1.Deck.Count == 0 && user2.Deck.Count == 0)
        {
            _battleLog.Add($"The battle ended in a draw!");
        }
        else if (user1.Deck.Count == 0)
        {
            User winner = user2;
            User loser = user1;

            UpdateWinLossRecords(winner, loser);
            UpdateEloScores(winner, loser);

            _battleLog.Add($"{user2.Username} won the battle!");
            _battleLog.Add($"");
        }
        else if (user2.Deck.Count == 0)
        {
            User winner = user1;
            User loser = user2;

            UpdateWinLossRecords(winner, loser);
            UpdateEloScores(winner, loser);

            _battleLog.Add($"{user1.Username} won the battle!");
            _battleLog.Add($"");
        }
        else
        {
            _battleLog.Add($"The battle ended in a draw!");
            _battleLog.Add($"");
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Initializes battle by clearing the battle log.
    /// </summary>
    private void InitializeBattle()
    {
        _battleLog = new List<string>();
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Executes a single round of battle between two users.
    /// </summary>
    private void ExecuteRound(User user1, User user2, int round)
    {
        LogRoundStart(round);
        if (CheckForEmptyDecks(user1, user2)) return;

        Card card1 = SelectRandomCard(user1.Deck);
        Card card2 = SelectRandomCard(user2.Deck);

        LogPreBattleState(user1, card1, user2, card2);

        Card? winnerCard = DetermineRoundWinner(user1, card1, user2, card2);

        if (winnerCard != null)
        {
            HandleRoundResult(winnerCard, user1, card1, user2, card2);
        }
        else
        {
            _battleLog.Add($"- Result: Draw!");
        }

        _battleLog.Add($"---------------------------------------------------\\n");
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Logs the start of a battle round.
    /// </summary>
    private void LogRoundStart(int round)
    {
        _battleLog.Add($"---------------------------------------------------");
        _battleLog.Add($"Round {round})");
        _battleLog.Add($"---------------------------------------------------");
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Checks if either user has an empty deck, indicating the end of the battle.
    /// </summary>
    private bool CheckForEmptyDecks(User user1, User user2)
    {
        int notLockedCardsCountUser1 = user1.Deck.Count(card => !card.IsLocked);
        int notLockedCardsCountUser2 = user2.Deck.Count(card => !card.IsLocked);

        if (notLockedCardsCountUser1 == 0)
        {
            _battleLog.Add($"{user1.Username} has no playable cards left!");
            return true;
        }
        else if (notLockedCardsCountUser2 == 0)
        {
            _battleLog.Add($"{user2.Username} has no playable cards left!");
            return true;
        }

        return false;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Logs the state of the battle before the round begins.
    /// </summary>
    private void LogPreBattleState(User user1, Card card1, User user2, Card card2)
    {
        _battleLog.Add($"- {user1.Username} ({user1.Deck.Count} cards in deck) vs {user2.Username} ({user2.Deck.Count} cards in deck)");
        _battleLog.Add($"- {user1.Username}'s \"{card1.Name}\" (Damage: {card1.Damage}) vs {user2.Username}'s \"{card2.Name}\" (Damage: {card2.Damage})");
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Determines the winner of a battle round.
    /// </summary>
    private Card? DetermineRoundWinner(User user1, Card card1, User user2, Card card2)
    {
        Card? winnerCard = HandleSpecialInteractions(card1, card2, user1, user2);

        if (winnerCard == null)
        {
            ApplySpellEffectAndAdjustDamage(card1, card2);
            winnerCard = CompareCardDamage(card1, card2);
        }
        return winnerCard;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Handles the result of a battle round, updating user stats and transferring cards.
    /// </summary>
    private void HandleRoundResult(Card? winnerCard, User user1, Card card1, User user2, Card card2)
    {
        if (winnerCard == null)
        {
            _battleLog.Add("- Result: Draw!");
        }
        else
        {
            User winner = winnerCard == card1 ? user1 : user2;
            User loser = winnerCard == card1 ? user2 : user1;
            Card lostCard = winnerCard == card1 ? card2 : card1;

            TransferCard(winner, loser, lostCard);

            _battleLog.Add($"- Result: \"{winnerCard.Name}\" won! ({winner.Username} gains \"{lostCard.Name}\" from {loser.Username})");
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Updates the win/loss records of the users after a battle round.
    /// </summary>
    /// <param name="winner">The user who won the battle round.</param>
    /// <param name="loser">The user who lost the battle round.</param>
    private void UpdateWinLossRecords(User winner, User loser)
    {
        _userRepository.AddWin(winner);
        _userRepository.AddLoss(loser);
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Updates the Elo scores of the users after a battle round.
    /// The winner gains 3 Elo points, the loser loses 5 Elo points.
    /// </summary>
    /// <param name="winner">The user who won the battle round.</param>
    /// <param name="loser">The user who lost the battle round.</param>
    private void UpdateEloScores(User winner, User loser)
    {
        _userRepository.UpdateElo(winner, 3);
        _userRepository.UpdateElo(loser, -5);
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Selects a random card from a user's deck which is not locked.
    /// </summary>
    private Card SelectRandomCard(List<Card> deck)
    {
        List<Card> unlockedCards = deck.Where(card => !card.IsLocked).ToList();

        int randomIndex = _random.Next(0, unlockedCards.Count);

        Card originalCard = unlockedCards[randomIndex];

        Card battleCard = originalCard.Clone();

        return battleCard;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Handles special interactions between cards </summary>
    /// <returns>The winning card, or null if no special interaction was found</returns>
    private Card? HandleSpecialInteractions(Card card1, Card card2, User user1, User user2)
    {
        Dictionary<(string, string), string> specialInteractions = new Dictionary<(string, string), string>
        {
            { ("Goblin", "Dragon"), "Goblins are too afraid of Dragons to attack." },
            { ("Wizard", "Orc"), "Wizards can control Orcs, making them unable to attack." },
            { ("Knight", "WaterSpell"), "The heavy armor of Knights makes them drown instantly in Water Spells." },
            { ("Kraken", "Spell"), "Krakens are immune against all spells." },
            { ("FireElf", "Dragon"), "Fire Elves can evade attacks from Dragons due to their familiarity." }
        };

        foreach (var interaction in specialInteractions)
        {
            // Check if the special interaction applies
            if (card1.Name.Contains(interaction.Key.Item1) && card2.Name.Contains(interaction.Key.Item2))
            {
                _battleLog.Add($"Special Interaction: {interaction.Value}");
                _battleLog.Add($"{user2.Username}'s {card2.Name} wins against {user1.Username}'s {card1.Name}.");
                return card2;
            }
            else if (card2.Name.Contains(interaction.Key.Item1) && card1.Name.Contains(interaction.Key.Item2))
            {
                _battleLog.Add($"Special Interaction: {interaction.Value}");
                _battleLog.Add($"{user1.Username}'s {card1.Name} wins against {user2.Username}'s {card2.Name}.");
                return card1;
            }
        }

        // No special interaction found
        return null;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Transfers a card from the loser to the winner of a battle round.
    /// </summary>
    private void TransferCard(User winner, User loser, Card clonedCard)
    {
        // Find the original card in the loser's deck
        Card? originalCard = loser.Deck.FirstOrDefault(card => card.Id == clonedCard.Id);

        if (originalCard != null)
        {
            loser.Stack.Remove(originalCard);
            loser.Deck.Remove(originalCard);

            winner.Stack.Add(originalCard);
            winner.Deck.Add(originalCard);

            _cardRepository.ChangeCardOwner(winner, originalCard);
        }

    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Applies spell effects to cards and adjusts their damage accordingly.
    /// </summary>
    private void ApplySpellEffectAndAdjustDamage(Card card1, Card card2)
    {
        // Apply effects only if at least one of the cards is a spell
        if (card1.Type == CardType.spell || card2.Type == CardType.spell)
        {

            // Apply and log effect for card1 if it's a spell
            if (card1.Type == CardType.spell)
            {
                double originalDamage = card1.Damage;
                bool card1EffectApplied = AdjustDamageBasedOnElement(card1, card2.Element);

                if (card1EffectApplied)
                {
                    _battleLog.Add($"Spell effect applied to {card1.Name}: {card1.Element} vs {card2.Element}");
                    _battleLog.Add($" > Original Damage: {originalDamage}, Multiplier Applied, New Damage: {card1.Damage}");
                }

            }

            // Apply and log effect for card2 if it's a spell
            if (card2.Type == CardType.spell)
            {
                double originalDamage = card2.Damage;
                bool card2EffectApplied = AdjustDamageBasedOnElement(card2, card1.Element);

                if (card2EffectApplied)
                {
                    _battleLog.Add($"Spell effect applied to {card2.Name}: {card2.Element} vs {card1.Element}");
                    _battleLog.Add($" > Original Damage: {originalDamage}, Multiplier Applied, New Damage: {card2.Damage}");
                }

            }
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Adjusts the damage of a card based on its elemental interaction with another card.
    /// </summary>
    public bool AdjustDamageBasedOnElement(Card card, ElementType opponentElement)
    {
        var interactionKey = (card.Element, opponentElement);
        if (_elementalEffectiveness.TryGetValue(interactionKey, out double multiplier))
        {
            card.Damage *= multiplier;
            return true;
        }
        return false;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Compares the damage of two cards to determine the round winner.
    /// </summary>
    public Card? CompareCardDamage(Card card1, Card card2)
    {
        if (card1.Damage > card2.Damage)
        {
            return card1;
        }
        else if (card2.Damage > card1.Damage)
        {
            return card2;
        }
        else
        {
            return null;
        }
    }

}
*/

namespace MonsterTradingCardsGame.UnitTests.Services
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
            StringAssert.Contains($"{user1.Username} has no playable cards left!", battleLog);



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
            StringAssert.Contains($"{user1.Username} has no playable cards left!", battleLog);

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

        // test for StartBattle
        [Test]
        public void StartBattle_Card1HasHigherDamage_ReturnsCard1WinsMessage()
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
                        Damage = 200,
                        Type = CardType.monster,
                        Element = ElementType.water,
                        IsLocked = false
                    }
                }
            };

            var user2 = new User()
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
            StringAssert.Contains("\"testcard1\" won!", battleLog);

        }

        [Test]
        public void StartBattle_Card2HasHigherDamage_ReturnsCard2WinsMessage()
        {
            // Arrange
            var user1 = new User()
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
                        Type = CardType.monster,
                        Element = ElementType.water,
                        IsLocked = false
                    }
                }
            };

            var user2 = new User()
            {
                Username = "testuser2",
                Elo = 100,
                Deck = new List<Card>()
                {
                    new Card()
                    {
                        Id = "efgh",
                        Name = "testcard2",
                        Damage = 200,
                        Type = CardType.monster,
                        Element = ElementType.fire,
                        IsLocked = false
                    }
                }
            };

            // Act
            string battleLog = _battleService.StartBattle(user1, user2);

            // Assert
            StringAssert.Contains("\"testcard2\" won!", battleLog);

        }

        //test for battle outcome elo adjustment
        [Test]
        public void StartBattle_Card1Wins_EloAdjustment()
        {
            // Arrange
            var user1 = new User()
            {
                Username = "testuser1",
                Elo = 100,
                Deck = new List<Card>()
            };

            var user2 = new User()
            {
                Username = "testuser2",
                Elo = 100,
                Deck = new List<Card>()
            };

            Card card1 = new Card()
            {
                Id = "abcd",
                Name = "testcard1",
                Damage = 200,
                Type = CardType.monster,
                Element = ElementType.water,
                IsLocked = false
            };

            Card card2 = new Card()
            {
                Id = "efgh",
                Name = "testcard2",
                Damage = 100,
                Type = CardType.monster,
                Element = ElementType.fire,
                IsLocked = false
            };

            user1.Deck.Add(card1);
            user2.Deck.Add(card2);

            // Act
            string battleLog = _battleService.StartBattle(user1, user2);

            // Assert
            _mockUserRepository.Verify(x => x.UpdateElo(user1, 3), Times.Once);
            _mockUserRepository.Verify(x => x.UpdateElo(user2, -5), Times.Once);



        }


    }
}
