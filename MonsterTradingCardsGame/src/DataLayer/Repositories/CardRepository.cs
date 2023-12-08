using System.Data;
using Npgsql;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Repositories
{
    public class CardRepository : BaseRepository<Card>, ICardRepository
    {
        public CardRepository() : base() { }

        protected override void Fill(Card card, IDataRecord record)
        {
            // Fill card details from the record
            card.Uuid = record.GetString(record.GetOrdinal("uuid"));
            card.Name = record.GetString(record.GetOrdinal("Name"));
            card.Damage = record.GetDouble(record.GetOrdinal("Damage"));
        }

        public override List<Card> GetAll()
        {
            var cards = new List<Card>();
            using (var command = new NpgsqlCommand("SELECT * FROM Cards", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var card = new Card();
                    Fill(card, reader);
                    cards.Add(card);
                }
            }
            return cards;
        }

        public List<Card> GetCardsByUsername(string username)
        {
            var cards = new List<Card>();
            using (var command = new NpgsqlCommand("SELECT * FROM Cards WHERE username = @username", connection))
            {
                command.Parameters.AddWithValue("@username", username);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var card = new Card();
                        Fill(card, reader);
                        cards.Add(card);
                    }
                }
            }
            return cards;
        }

        public List<Card> GetDeckByUsername(string username)
        {
            try
            {

            var cards = new List<Card>();
            using (var command = new NpgsqlCommand("SELECT * FROM Cards WHERE username = @username AND indeck = true", connection))
            {
                command.Parameters.AddWithValue("@username", username);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var card = new Card();
                        Fill(card, reader);
                        cards.Add(card);
                    }
                }
            }
                return cards;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override void Save(Card card)
        {
            using (var command = new NpgsqlCommand("INSERT INTO Cards (Name, Type, Element, Damage) VALUES (@name, @type, @element, @damage)", connection))
            {
                command.Parameters.AddWithValue("@name", card.Name);
                command.Parameters.AddWithValue("@type", card.Type);
                command.Parameters.AddWithValue("@element", card.Element);
                command.Parameters.AddWithValue("@damage", card.Damage);

                command.ExecuteNonQuery();
            }
        }

        public Card GetCardById(int cardId)
        {
            throw new NotImplementedException();
        }
    }
}
