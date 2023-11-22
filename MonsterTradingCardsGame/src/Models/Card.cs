using System;

namespace MonsterTradingCardsGame
{
    public enum CardType { Monster, Spell }

    public enum ElementType { Fire, Water, Normal }

    public class Card
    {
        public string Name { get; set; }
        public CardType Type { get; set; }
        public ElementType Element { get; set; }
        public int Damage { get; set; }

        public Card(string name, CardType type, ElementType element, int damage)
        {
            this.Name = name;
            this.Type = type;
            this.Element = element;
            this.Damage = damage;
        }
    }
}
