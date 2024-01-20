using System.Data;
using Npgsql;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Repositories
{
    public class CardRepository : BaseRepository, ICardRepository
    {
        public CardRepository() : base() { }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Fills a Card object with data from a database record. </summary>
        /// <param name="card">The Card object to fill.</param>
        /// <param name="record">The data record containing card information.</param>
        protected void Fill(Card card, IDataRecord record)
        {
            // Fill card details from the record
            card.Id = record.GetString(record.GetOrdinal("Id"));
            card.Name = record.GetString(record.GetOrdinal("Name"));
            card.Damage = record.GetDouble(record.GetOrdinal("Damage"));
            card.Element = (ElementType)Enum.Parse(typeof(ElementType), record.GetString(record.GetOrdinal("Element")));
            card.Type = (CardType)Enum.Parse(typeof(CardType), record.GetString(record.GetOrdinal("Type")));
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Determines the CardType from the card name. </summary>
        /// <param name="cardName">The name of the card.</param>
        /// <returns>The CardType based on the card name.</returns>
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Determines the ElementType from the card name. </summary>
        /// <param name="cardName">The name of the card.</param>
        /// <returns>The ElementType based on the card name.</returns>
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


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Retrieves all cards owned by a specific user. </summary>
        /// <param name="user">The user whose cards are to be retrieved.</param>
        /// <returns>A list of Cards owned by the user.</returns>
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Retrieves the deck of a specific user. </summary>
        /// <param name="user">The user whose deck is to be retrieved.</param>
        /// <returns>A list of Cards in the user's deck.</returns>
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Deletes a package of cards by its ID. </summary>
        /// <param name="packageId">The ID of the package to delete.</param>
        /// <returns>True if the deletion is successful, false otherwise.</returns>
        public bool DeletePackageById(int packageId)
        {
            try
            {
                using (var command = new NpgsqlCommand("DELETE FROM package_cards WHERE packageid = @packageId", connection))
                {
                    command.Parameters.AddWithValue("@packageId", packageId);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return false;
                    }
                }
                using (var command = new NpgsqlCommand("DELETE FROM packages WHERE id = @packageId", connection))
                {
                    command.Parameters.AddWithValue("@packageId", packageId);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in DeletePackage: " + ex.Message);
                return false;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Retrieves a random package of cards. </summary>
        /// <returns>A list of Cards in the retrieved package.</returns>
        public List<Card> GetCardPackage()
        {
            try
            {
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

                DeletePackageById(randomPackageId);


                return cards;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetCardPackage: " + ex.Message);
                return new List<Card>();
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Locks a card in a trade, preventing it from being used elsewhere. </summary>
        /// <param name="user">The owner of the card.</param>
        /// <param name="card">The card to lock in trade.</param>
        /// <returns>True if the lock is successful, false otherwise.</returns>
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Unlocks a card from a trade, allowing it to be used again. </summary>
        /// <param name="user">The owner of the card.</param>
        /// <param name="card">The card to unlock from trade.</param>
        /// <returns>True if the unlock is successful, false otherwise.</returns>
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Checks if a card is owned by a specific user. </summary>
        /// <param name="user">The user to check ownership against.</param>
        /// <param name="card">The card to check.</param>
        /// <returns>True if the user owns the card, false otherwise.</returns>
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Saves a package of cards to the database. </summary>
        /// <param name="cards">The list of Cards to be saved in the package.</param>
        /// <returns>The ID of the saved package, null if the operation fails.</returns>
        public int? SavePackage(List<Card> cards)
        {
            try
            {
                int? packageId;
                using (var command = new NpgsqlCommand("INSERT INTO packages(packagename, packagecost) VALUES ('defaultPackage', 5) RETURNING id", connection))
                {
                    packageId = (int?)command.ExecuteScalar();
                }

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
                return packageId;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SavePackage: " + ex.Message);
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Saves a card to the database. </summary>
        /// <param name="card">The Card object to be saved.</param>
        /// <returns>True if the card is successfully saved, false otherwise.</returns>
        public bool SaveCard(Card card)
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Retrieves a card by its ID. </summary>
        /// <param name="cardId">The ID of the card to retrieve.</param>
        /// <returns>The Card object if found, null otherwise.</returns>
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Saves a package of cards to a user. </summary>
        /// <param name="user">The user to whom the package is to be saved.</param>
        /// <param name="package">The list of Cards in the package.</param>
        /// <returns>True if the package is successfully saved to the user, false otherwise.</returns>
        public bool SavePackageToUser(User user, List<Card> package)
        {
            try
            {
                foreach (var card in package)
                {
                    using (var command = new NpgsqlCommand("INSERT INTO user_cards(userid, cardid, indeck, lockedintrade) VALUES (@userid, @cardid, 'false', 'false')", connection))
                    {
                        command.Parameters.AddWithValue("@userid", user.Id);
                        command.Parameters.AddWithValue("@cardid", card.Id);
                        command.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SavePackageToUser: " + ex.Message);
                return false;
            }
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary> Changes the owner of a card. </summary>
        /// <param name="user">The new owner of the card.</param>
        /// <param name="card">The card whose ownership is to be changed.</param>
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
