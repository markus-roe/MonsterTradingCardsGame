namespace MonsterTradingCardsGame.Models
{
    public enum CardType { Monster, Spell }

    public enum ElementType { Fire, Water, Normal }

    public class Card
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CardType Type { get; set; }
        public ElementType Element { get; set; }
        public double Damage { get; set; }

        // Constructors
        public Card() {}

        public Card(string name, CardType type, ElementType element, double damage)
        {
            this.Name = name;
            this.Type = type;
            this.Element = element;
            this.Damage = damage;
        }
    }
}
