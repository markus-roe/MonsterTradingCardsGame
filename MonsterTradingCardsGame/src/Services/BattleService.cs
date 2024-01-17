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

    private void DetermineBattleOutcome(User user1, User user2)
    {
        if (user1.Deck.Count == 0 && user2.Deck.Count == 0)
        {
            _battleLog.Add($"The battle ended in a draw!");
        }
        else if (user1.Deck.Count == 0)
        {
            _battleLog.Add($"{user2.Username} won the battle!");
        }
        else if (user2.Deck.Count == 0)
        {
            _battleLog.Add($"{user1.Username} won the battle!");
        }
        else
        {
            _battleLog.Add($"The battle ended in a draw!");
        }
    }

    private void InitializeBattle()
    {
        _battleLog = new List<string>();
    }

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

    private void LogRoundStart(int round)
    {
        _battleLog.Add($"---------------------------------------------------");
        _battleLog.Add($"Round {round})");
        _battleLog.Add($"---------------------------------------------------");
    }

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

    private void LogPreBattleState(User user1, Card card1, User user2, Card card2)
    {
        _battleLog.Add($"- {user1.Username} ({user1.Deck.Count} cards remaining, ELO: {user1.Elo}) vs {user2.Username} ({user2.Deck.Count} cards remaining, ELO: {user2.Elo})");
        _battleLog.Add($"- {user1.Username}'s \"{card1.Name}\" (Damage: {card1.Damage}) vs {user2.Username}'s \"{card2.Name}\" (Damage: {card2.Damage})");
    }


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
            UpdateWinLossRecords(winner, loser);
            UpdateEloScores(winner, loser);

            _battleLog.Add($"- Result: \"{winnerCard.Name}\" won! ({winner.Username} gains \"{lostCard.Name}\" from {loser.Username})");
        }
    }

    private void UpdateWinLossRecords(User winner, User loser)
    {
        _userRepository.AddWin(winner);
        _userRepository.AddLoss(loser);
    }

    private void UpdateEloScores(User winner, User loser)
    {
        _userRepository.UpdateElo(winner, 3);
        _userRepository.UpdateElo(loser, -5);
    }

    private Card SelectRandomCard(List<Card> deck)
    {
        List<Card> unlockedCards = deck.Where(card => !card.IsLocked).ToList();

        int randomIndex = _random.Next(0, unlockedCards.Count);

        Card originalCard = unlockedCards[randomIndex];

        Card battleCard = originalCard.Clone();

        return battleCard;
    }

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