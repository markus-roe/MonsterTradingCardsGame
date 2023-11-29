
namespace MonsterTradingCardsGame.Models
{
    public class User
    {
        public string Username { get; set; }
        public int Coins { get; set; }
        public int Elo { get; set; }
        public List<Card> Stack { get; set; }
        public List<Card> Deck { get; set; }

        public User()
        {
            this.Username = "defaultUserName";
            this.Coins = 20;
            this.Elo = 100;
            this.Stack = new List<Card>();
            this.Deck = new List<Card>();
        }

    }

}
