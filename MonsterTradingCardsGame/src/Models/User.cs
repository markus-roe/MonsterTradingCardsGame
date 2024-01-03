
using MonsterTradingCardsGame.Repositories;

namespace MonsterTradingCardsGame.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }
        public int Coins { get; set; }
        public int Elo { get; set; }
        public List<Card> Stack { get; set; }
        public List<Card> Deck { get; set; }
        public Session Session { get; set; }

        public User()
        {
            Id = 0;
            Username = string.Empty;
            Password = string.Empty;
            Name = string.Empty;
            Bio = string.Empty;
            Image = string.Empty;
            Coins = 20;
            Elo = 100;
            Stack = new List<Card>();
            Deck = new List<Card>();
        }

    }

}
