using MonsterTradingCardsGame.Repositories;
using Npgsql;

namespace MonsterTradingCardsGame.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        NpgsqlConnection Connection { get; }
        NpgsqlTransaction? Transaction { get; }

        void CreateTransaction();
        void Commit();
        void Rollback();

        UserRepository UserRepository();

    }
}
