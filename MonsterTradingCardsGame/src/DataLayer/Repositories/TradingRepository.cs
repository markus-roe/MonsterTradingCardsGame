using Npgsql;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Interfaces;
using System.Data;

namespace MonsterTradingCardsGame.Repositories
{

    public class TradingRepository : BaseRepository<TradingDeal>, ITradingRepository
    {
        private readonly IUserRepository _userRepository;

        public TradingRepository(IUserRepository userRepository) : base()
        {
            _userRepository = userRepository;
        }

        public TradingDeal? GetTradingDealById(string id)
        {
            try
            {
                using (var command = new NpgsqlCommand("SELECT * FROM tradings WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string cardToTrade = reader.GetString(reader.GetOrdinal("cardToTrade"));
                            string type = reader.GetString(reader.GetOrdinal("type"));
                            float minimumDamage = reader.GetFloat(reader.GetOrdinal("minimumdamage"));

                            TradingDeal tradingDeal = new TradingDeal
                            {
                                Id = id,
                                CardToTrade = cardToTrade,
                                Type = type,
                                MinimumDamage = minimumDamage
                            };
                            return tradingDeal;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in getTradingDeal: " + ex.Message);
                return null;
            }
        }

        public List<TradingDeal>? GetTradingDeals()
        {
            try
            {
                List<TradingDeal> tradingDeals = new List<TradingDeal>();

                using (var command = new NpgsqlCommand("SELECT * FROM tradings", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            string id = reader.GetString(reader.GetOrdinal("id"));
                            string cardToTrade = reader.GetString(reader.GetOrdinal("cardToTrade"));
                            string type = reader.GetString(reader.GetOrdinal("type"));
                            float minimumDamage = reader.GetFloat(reader.GetOrdinal("minimumdamage"));

                            TradingDeal tradingDeal = new TradingDeal
                            {
                                Id = id,
                                CardToTrade = cardToTrade,
                                Type = type,
                                MinimumDamage = minimumDamage
                            };
                            tradingDeals.Add(tradingDeal);
                        }
                    }
                }
                return tradingDeals;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetTradingDeals: " + ex.Message);
                return null;
            }
        }

        public TradingDeal? createTradingDeal(TradingDeal tradingDeal, User user)
        {
            try
            {
                using (var command = new NpgsqlCommand("INSERT INTO tradings (id, cardToTrade, type, minimumdamage, userid) VALUES (@id, @cardToTrade, @type, @minimumdamage, @userId)", connection))
                {
                    command.Parameters.AddWithValue("id", tradingDeal.Id);
                    command.Parameters.AddWithValue("cardToTrade", tradingDeal.CardToTrade);
                    command.Parameters.AddWithValue("type", tradingDeal.Type);
                    command.Parameters.AddWithValue("minimumdamage", tradingDeal.MinimumDamage);
                    command.Parameters.AddWithValue("userId", user.Id);

                    command.ExecuteNonQuery();
                }
                return tradingDeal;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in createTradingDeal: " + ex.Message);
                return null;
            }
        }

        public int GetTradingDealUserId(TradingDeal tradingDeal)
        {
            try
            {
                using (var command = new NpgsqlCommand("SELECT userid FROM tradings WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("id", tradingDeal.Id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userId = reader.GetInt32(reader.GetOrdinal("userid"));
                            return userId;
                        }
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in TradingDealUserId: " + ex.Message);
                return 0;
            }
        }

        public bool DeleteTradingDeal(TradingDeal tradingDeal)
        {
            try
            {
                using (var command = new NpgsqlCommand("DELETE FROM tradings WHERE id = @tradingDealId", connection))
                {
                    command.Parameters.AddWithValue("tradingDealId", tradingDeal.Id);

                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in deleteTradingDeal: " + ex.Message);
                return false;
            }
        }

        protected override void Fill(TradingDeal entity, IDataRecord record)
        {
            throw new NotImplementedException();
        }

        public override bool Save(TradingDeal obj)
        {
            throw new NotImplementedException();
        }
    }

}