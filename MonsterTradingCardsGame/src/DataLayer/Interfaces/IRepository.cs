using System;

namespace MonsterTradingCardsGame.Interfaces
{
    public interface IRepository<T> where T : class
    {
        List<T>? GetAll();
        T? GetById(Guid id);
        T? Add(T obj);
        T? Update(T obj);
        bool Delete(T obj);
        bool Delete(Guid id);

    }

}
