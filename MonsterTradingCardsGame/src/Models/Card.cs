namespace MonsterTradingCardsGame.Models
{
    public enum CardType { Monster, Spell }

    public enum ElementType { Fire, Water, Normal }

    public class Card
    {
        public string Uuid { get; set; }
        public string Name { get; set; }
        public CardType Type { get; set; }
        public ElementType Element { get; set; }
        public double Damage { get; set; }

        // Constructors
        public Card() {}

        public override string ToString()
        {
            return $"{Name} - Type: {Type}, Element: {Element}, Damage: {Damage}";
        }

    }
}
