﻿using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        User GetUserByUsername(string username);
        bool SetCardDeck(User user, List<Card> cards);
        bool Save(User user);

    }
}
