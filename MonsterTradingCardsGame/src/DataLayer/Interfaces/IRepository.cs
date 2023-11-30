namespace MonsterTradingCardsGame.Interfaces
{
    public interface IRepository<T> where T : class
    {
        List<T> GetAll();
        T? Get(string str);
        void Save(T obj);
        void Delete(T obj);
    }
}
