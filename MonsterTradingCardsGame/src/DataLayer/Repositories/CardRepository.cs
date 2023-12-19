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
            if (cardName.ToLower().Contains("spell"))
            {
                return CardType.spell;
            }
            else
            {
                return CardType.monster;
            }
        }
        public ElementType GetCardElementFromName(string cardName)
        {
            if (cardName.ToLower().Contains("fire"))
            {
                return ElementType.fire;
            }
            else if (cardName.ToLower().Contains("water"))
            {
                return ElementType.water;
            }
            else
            {
                return ElementType.normal;
            }
        }


        public List<Card> GetCardsByUser(User user)
        {
            var cards = new List<Card>();
            try
            {
                using (var command = new NpgsqlCommand("SELECT c.* FROM Cards c INNER JOIN user_cards uc ON c.Id = uc.cardid WHERE uc.userid = @userid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetCardsByUser: " + ex.Message);
            }
            return cards;
        }

        public List<Card> GetDeckByUser(User user)
        {
            try
            {

                var cards = new List<Card>();
                using (var command = new NpgsqlCommand("SELECT c.name, c.damage, c.element, c.type, c.id FROM user_cards uc JOIN cards c ON uc.cardid = c.id WHERE userid = @userid AND indeck = true", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);
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
                Console.WriteLine("Error in GetDeckByUser: " + ex.Message);
                return new List<Card>();
            }
        }

        public bool DeletePackage(int packageId)
        {
            try
            {
                using (var command = new NpgsqlCommand("DELETE FROM package_cards WHERE packageid = @packageId", connection))
                {
                    command.Parameters.AddWithValue("@packageId", packageId);
                    command.ExecuteNonQuery();
                }
                using (var command = new NpgsqlCommand("DELETE FROM packages WHERE id = @packageId", connection))
                {
                    command.Parameters.AddWithValue("@packageId", packageId);
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in DeletePackage: " + ex.Message);
                return false;
            }
        }



        public List<Card> GetCardPackage()
        {
            try
            {

                //get random package from packages table
                //get all cards from package_cards table with packageid = packageid from packages table
                //return list of cards

                //get package count for random limit
                List<int> packageIds = new List<int>();
                using (var command = new NpgsqlCommand("SELECT id FROM packages", connection))
                {
                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        packageIds.Add((int)result);
                    }
                }

                if (packageIds.Count == 0)
                {
                    return new List<Card>();
                }

                //get random package from vector
                int randomPackageId = packageIds[new Random().Next(0, packageIds.Count)];

                //get cards from package
                var cards = new List<Card>();
                using (var command = new NpgsqlCommand("SELECT c.name, c.damage, c.element, c.type, c.id FROM package_cards pc JOIN cards c ON pc.cardid = c.id WHERE packageid = @packageid", connection))
                {
                    command.Parameters.AddWithValue("@packageid", randomPackageId);
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

                DeletePackage(randomPackageId);


                return cards;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetCardPackage: " + ex.Message);
                return new List<Card>();
            }
        }

        public bool LockCardInTrade(User user, Card card)
        {
            try
            {
                using (var command = new NpgsqlCommand("UPDATE user_cards SET lockedintrade = true WHERE userid = @userid AND cardid = @cardid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@cardid", card.Id);
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in LockCardInTrade: " + ex.Message);
                return false;
            }
        }

        public bool UnlockCard(User user, Card card)
        {
            try
            {
                using (var command = new NpgsqlCommand("UPDATE user_cards SET lockedintrade = false WHERE userid = @userid AND cardid = @cardid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@cardid", card.Id);
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UnlockCard: " + ex.Message);
                return false;
            }
        }

        public bool CheckIfCardIsOwnedByUser(User user, Card card)
        {
            try
            {
                using (var command = new NpgsqlCommand("SELECT * FROM user_cards WHERE userid = @userid AND cardid = @cardid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@cardid", card.Id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in checkIfCardIsOwnedByUser: " + ex.Message);
                return false;
            }
        }

        public bool SavePackage(List<Card> cards)
        {
            try
            {
                int packageId;
                using (var command = new NpgsqlCommand("INSERT INTO packages(packagename, packagecost) VALUES ('defaultPackage', 5) RETURNING id", connection))
                {
                    packageId = (int)command.ExecuteScalar();
                }

                //save cards to cards tableq
                foreach (var card in cards)
                {
                    using (var command = new NpgsqlCommand("INSERT INTO cards(name, damage, element, type, id) VALUES (@name, @damage, @element, @type, @id)", connection))
                    {
                        command.Parameters.AddWithValue("@name", card.Name);
                        command.Parameters.AddWithValue("@damage", card.Damage);
                        command.Parameters.AddWithValue("@element", card.Element.ToString());
                        command.Parameters.AddWithValue("@type", card.Type.ToString());
                        command.Parameters.AddWithValue("@id", card.Id);

                        command.ExecuteNonQuery();
                    }
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

        public override bool Save(Card card)
        {
            try
            {
                using (var command = new NpgsqlCommand("INSERT INTO Cards (name, damage, element, type, id) VALUES (@name, @damage, @element, @type, @id)", connection))
                {
                    command.Parameters.AddWithValue("@name", card.Name);
                    command.Parameters.AddWithValue("@damage", card.Damage);
                    command.Parameters.AddWithValue("@element", card.Element.ToString());
                    command.Parameters.AddWithValue("@type", card.Type.ToString());
                    command.Parameters.AddWithValue("@id", card.Id);

                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Card Save: " + ex.Message);
                return false;
            }
        }

        public Card? GetCardById(string cardId)
        {
            try
            {
                using (var command = new NpgsqlCommand("SELECT * FROM Cards WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", cardId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var card = new Card();
                            Fill(card, reader);
                            return card;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetCardById: " + ex.Message);
                return null;
            }
        }

        public void SavePackageToUser(User user, List<Card> package)
        {
            try
            {
                foreach (var card in package)
                {
                    using (var command = new NpgsqlCommand("INSERT INTO user_cards(userid, cardid, indeck) VALUES (@userid, @cardid, 'false')", connection))
                    {
                        command.Parameters.AddWithValue("@userid", user.Id);
                        command.Parameters.AddWithValue("@cardid", card.Id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SavePackageToUser: " + ex.Message);
            }
        }


        public void ChangeCardOwner(User user, Card card)
        {
            try
            {
                using (var command = new NpgsqlCommand("UPDATE user_cards SET userid = @userid WHERE cardid = @cardid", connection))
                {
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@cardid", card.Id);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in ChangeCardOwner: " + ex.Message);
            }
        }

    }
}
