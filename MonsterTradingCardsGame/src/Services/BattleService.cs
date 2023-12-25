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

    public string StartBattle(User user1, User user2)
    {

        int round = 1;

        while (round <= 100)
        {

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

            // Handle special interactions
            Card? winnerCard = HandleSpecialInteractions(card1, card2, user1, user2);


            if (winnerCard == null) //if no winnerCard was found yet
            {
                //apply spell effect
                ApplySpellEffect(card1, card2);

                //fight between card1 and card2
                if (card1.Damage > card2.Damage)
                {
                    //card1 wins
                    winnerCard = card1;
                    TransferCard(user1, user2, card2);
                    
                }
                else if (card1.Damage < card2.Damage)
                {
                    //card2 wins
                    winnerCard = card2;
                    TransferCard(user2, user1, card1);
                }
                else
                {
                    //draw, no action
                }
            }

            // Log the details of this round
            if (winnerCard != null)
                _battleLog.Add($"Round {round}: {card1.Name} vs {card2.Name} - {winnerCard.Name} won!");
            else
                _battleLog.Add($"Round {round}: {card1.Name} vs {card2.Name} - Draw!");

            round++;

        }
        return string.Join(Environment.NewLine, _battleLog);
    }

    private void ApplySpellEffect(Card card1, Card card2)
    {
        // Apply effects only if at least one of the cards is a spell
        if (card1.Type == CardType.spell || card2.Type == CardType.spell)
        {
            AdjustDamageBasedOnElement(card1, card2.Element);
            AdjustDamageBasedOnElement(card2, card1.Element);
        }
    }

    private void AdjustDamageBasedOnElement(Card card, ElementType opponentElement)
    {
        if (card.Type != CardType.spell) return;

        var interactionKey = (card.Element, opponentElement);

        if (_elementalEffectiveness.TryGetValue(interactionKey, out double multiplier))
        {
            card.Damage *= multiplier;
            _battleLog.Add($"Damage adjusted based on element: {card.Name} vs {opponentElement}");
        }
    }
    
    private void ApplySpellEffect(Card card1, Card card2)
    {
        // Apply effects only if at least one of the cards is a spell
        if (card1.Type == CardType.spell || card2.Type == CardType.spell)
        {
            AdjustDamageBasedOnElement(card1, card2.Element);
            AdjustDamageBasedOnElement(card2, card1.Element);
        }

        if (card1.Type == CardType.spell && card2.Type == CardType.spell)
        {
            _battleLog.Add($"Spell effect applied: {card1.Name} ({card1.Element}) vs {card2.Name} ({card2.Element})");
        }
        else if (card1.Type == CardType.spell)
        {
            _battleLog.Add($"Spell effect applied: {card1.Name} ({card1.Element}) vs {card2.Name} (Not a spell)");
        }
        else if (card2.Type == CardType.spell)
        {
            _battleLog.Add($"Spell effect applied: {card1.Name} (Not a spell) vs {card2.Name} ({card2.Element})");
        }
    }

    private Card? HandleSpecialInteractions(Card card1, Card card2, User user1, User user2)
    {
        // Goblins are too afraid of Dragons to attack.
        if (card1.Name.Contains("Goblin") && card2.Name.Contains("Dragon"))
        {
            TransferCard(user2, user1, card1);
            return card2;
        }
        else if (card2.Name.Contains("Goblin") && card1.Name.Contains("Dragon"))
        {
            TransferCard(user1, user2, card2);
            return card1;
        }

        // Wizard can control Orcs so they are not able to damage them.
        if (card1.Name.Contains("Wizard") && card2.Name.Contains("Orc"))
        {
            TransferCard(user1, user2, card2);
            return card1;
        }
        else if (card2.Name.Contains("Wizard") && card1.Name.Contains("Orc"))
        {
            TransferCard(user2, user1, card1);
            return card2;
        }

        // The armor of Knights is so heavy that WaterSpells make them drown instantly.
        if (card1.Name.Contains("Knight") && card2.Name.Contains("WaterSpell"))
        {
            TransferCard(user1, user2, card2);
            return card1;
        }
        else if (card2.Name.Contains("Knight") && card1.Name.Contains("WaterSpell"))
        {
            TransferCard(user2, user1, card1);
            return card2;
        }

        // The Kraken is immune against spells.
        if (card1.Name.Contains("Kraken") && card2.Type == CardType.spell)
        {
            TransferCard(user1, user2, card2);
            return card1;
        }
        else if (card2.Name.Contains("Kraken") && card1.Type == CardType.spell)
        {
            TransferCard(user2, user1, card1);
            return card2;
        }

        // The FireElves know Dragons since they were little and can evade their attacks.
        if (card1.Name.Contains("FireElf") && card2.Name.Contains("Dragon"))
        {
            TransferCard(user1, user2, card2);
            return card1;
        }
        else if (card2.Name.Contains("FireElf") && card1.Name.Contains("Dragon"))
        {
            TransferCard(user2, user1, card1);
            return card2;
        }

        // No special interaction found
        return null;
    }

    // Helper method to transfer a card from loser to winner
    private void TransferCard(User winner, User loser, Card card)
    {
        loser.Deck.Remove(card);
        winner.Deck.Add(card);
        _userRepo.ChangeCardOwner(winner, card)
    }


    private Card SelectRandomCard(List<Card> deck)
    {

        int randomIndex = _random.Next(0, deck.Count);

        Card randomCard = deck[randomIndex];

        return randomCard;
    }


    private void UpdateUserStats(User user1, User user2)
    {
        //TODO logic to update user stats
    }

}