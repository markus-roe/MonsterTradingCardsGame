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
            card.Id = record.GetString(record.GetOrdinal("Id"));
            card.Name = record.GetString(record.GetOrdinal("Name"));
            card.Damage = record.GetDouble(record.GetOrdinal("Damage"));
            card.Element = (ElementType)Enum.Parse(typeof(ElementType), record.GetString(record.GetOrdinal("Element")));
            card.Type = (CardType)Enum.Parse(typeof(CardType), record.GetString(record.GetOrdinal("Type")));
        }

        public CardType GetCardTypeFromName(string cardName)
        {
            if (cardName.Contains("Spell"))
            {
                return CardType.Spell;
            }
            else
            {
                return CardType.Monster;
            }
        }
        public ElementType GetCardElementFromName(string cardName)
        {
            if (cardName.Contains("Fire"))
            {
                return ElementType.Fire;
            }
            else if (cardName.Contains("Water"))
            {
                return ElementType.Water;
            }
            else
            {
                return ElementType.Normal;
            }
        }

        // public List<Card> BuyPackage()
        // {

        // }

        public bool SavePackage(User user, List<Card> cards)
        {
            try
            {
                int packageId;
                using (var command = new NpgsqlCommand("INSERT INTO packages(packagename, packagecost) VALUES ('defaultPackage', 5) RETURNING id", connection))
                {
                    packageId = (int)command.ExecuteScalar();
                }

                foreach (var card in cards)
                {
                    using (var command = new NpgsqlCommand("INSERT INTO package_cards(packageid, cardid) VALUES (@packageId, @cardId)", connection))
                    {
                        command.Parameters.AddWithValue("@packageId", packageId);
                        command.Parameters.AddWithValue("@cardId", card.Id);
                        command.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SavePackage: " + ex.Message);
                return false;
            }
        }

        //edit method:
        // cards from user is now in separate table user_cards: userid (int), cardid (string)


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
