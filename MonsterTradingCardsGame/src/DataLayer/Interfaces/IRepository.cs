namespace MonsterTradingCardsGame.Interfaces
{
    public interface IRepository<T> where T : class
    {
        List<T> GetAll();
        T GetById(int id);
        void Add(T entity);
        bool Update(T entity);
        void Delete(T entity);
    }

}
