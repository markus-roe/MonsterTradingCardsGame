namespace MonsterTradingCardsGame.Models
{
    public enum CardType { monster, spell }

    public enum ElementType { fire, water, normal }

    public class Card
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public CardType Type { get; set; }
        public ElementType Element { get; set; }
        public double Damage { get; set; }
        public bool IsLocked { get; set; }

        // Constructors
        public Card() { }

        public Card Clone()
        {
            return new Card
            {
                Id = this.Id,
                Name = this.Name,
                Type = this.Type,
                Element = this.Element,
                Damage = this.Damage,
                IsLocked = this.IsLocked

            };
        }

        public override string ToString()
        {
            return $"{Name} - Type: {Type}, Element: {Element}, Damage: {Damage}";
        }

    }
}
