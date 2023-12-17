using System.Data;
using Npgsql;
using MonsterTradingCardsGame.Interfaces;

namespace MonsterTradingCardsGame.Repositories
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly NpgsqlConnection connection;

        public BaseRepository()
        {
            var connectionString = "Server=127.0.0.1;Port=5432;Database=MTCG;User Id=postgres;Password=mtcgpw;";
            connection = new NpgsqlConnection(connectionString);
            connection.Open();
        }

        protected abstract void Fill(T entity, IDataRecord record);

        public virtual List<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(T entity)
        {
            throw new NotImplementedException();
        }

        public T GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(T entity)
        {
            throw new NotImplementedException();
        }

        public virtual bool Update(T entity)
        {
            throw new NotImplementedException();
        }
        public abstract bool Save(T obj);

    }
}
