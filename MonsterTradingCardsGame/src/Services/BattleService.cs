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

        int round = 1;

        while (round <= 100)
        {
            // Round start timestamp
            _battleLog.Add($"---------------------------------------------------");
            _battleLog.Add($"Round {round})");
            _battleLog.Add($"---------------------------------------------------");

            if (user1.Deck.Count == 0)
            {
                _battleLog.Add($"{user1.Username} has no cards left!");
                break;
            }
            else if (user2.Deck.Count == 0)
            {
                _battleLog.Add($"{user2.Username} has no cards left!");
                break;
            }

            Card card1 = SelectRandomCard(user1.Deck);
            Card card2 = SelectRandomCard(user2.Deck);

            // Check if card1 is locked
            if (card1.IsLocked)
            {
                _battleLog.Add($"{user1.Username}'s card {card1.Name} is locked and cannot be played!");
                continue;
            }

            // Check if card2 is locked
            if (card2.IsLocked)
            {
                _battleLog.Add($"{user2.Username}'s card {card2.Name} is locked and cannot be played!");
                continue;
            }

            _battleLog.Add($"- {user1.Username} ({user1.Deck.Count} cards remaining) vs {user2.Username} ({user2.Deck.Count} cards remaining)");
            _battleLog.Add($"- {user1.Username}'s \"{card1.Name}\" (Damage: {card1.Damage}) vs {user2.Username}'s \"{card2.Name}\" (Damage: {card2.Damage})");

            // Handle special interactions
            Card? winnerCard = HandleSpecialInteractions(card1, card2, user1, user2);
            if (winnerCard != null)
            {
                _battleLog.Add($"  > Special Interaction: \"{winnerCard.Name}\" won!");
            }

            // If no special interaction determines the winner, proceed with the battle
            if (winnerCard == null)
            {
                // Apply spell effect and adjust damage
                ApplySpellEffectAndAdjustDamage(card1, card2);

                if (card1.Damage > card2.Damage)
                {
                    winnerCard = card1;
                    TransferCard(user1, user2, card2);
                }
                else if (card1.Damage < card2.Damage)
                {
                    winnerCard = card2;
                    TransferCard(user2, user1, card1);
                }
            }

            // Log the result of the round
            if (winnerCard != null)
            {
                string loserUsername = winnerCard == card1 ? user2.Username : user1.Username;
                string winnerUsername = winnerCard == card1 ? user1.Username : user2.Username;
                Card lostCard = winnerCard == card1 ? card2 : card1;
                _battleLog.Add($"- Result: \"{winnerCard.Name}\" won! ({winnerUsername} gains \"{lostCard.Name}\" from {loserUsername})");
            }
            else
            {
                _battleLog.Add($"- Result: Draw!");
            }

            _battleLog.Add($"---------------------------------------------------\n");

            round++;
        }


        return string.Join(Environment.NewLine, _battleLog);
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

    private bool AdjustDamageBasedOnElement(Card card, ElementType opponentElement)
    {
        var interactionKey = (card.Element, opponentElement);
        if (_elementalEffectiveness.TryGetValue(interactionKey, out double multiplier))
        {
            card.Damage *= multiplier;
            return true;
        }
        return false;
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
                TransferCard(user2, user1, card1);
                return card2;
            }
            else if (card2.Name.Contains(interaction.Key.Item1) && card1.Name.Contains(interaction.Key.Item2))
            {
                _battleLog.Add($"Special Interaction: {interaction.Value}");
                _battleLog.Add($"{user1.Username}'s {card1.Name} wins against {user2.Username}'s {card2.Name}.");
                TransferCard(user1, user2, card2);
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
            loser.Deck.Remove(originalCard); // Remove the original card
            winner.Deck.Add(originalCard); // Add the original card to the winner's deck
            _cardRepository.ChangeCardOwner(winner, originalCard);
            _userRepository.AddWin(winner);
            _userRepository.AddLoss(loser);
        }

    }


    private Card SelectRandomCard(List<Card> deck)
    {
        int randomIndex = _random.Next(0, deck.Count);
        Card originalCard = deck[randomIndex];

        Card battleCard = originalCard.Clone();

        return battleCard;
    }


}