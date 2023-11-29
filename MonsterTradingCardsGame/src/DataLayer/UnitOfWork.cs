using System;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Repositories;
using Npgsql;

namespace MonsterTradingCardsGame.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly string connString;
        private readonly NpgsqlConnection npgsqlConnection;
        private bool disposedValue;
        private NpgsqlTransaction? sqlTransaction;
         
        private UserRepository? userRepository;

        public UnitOfWork()
        {
            connString = "Server=127.0.0.1;Port=5432;Database=MTCG;User Id=postgres;Password=mtcgpw;";
            npgsqlConnection = new NpgsqlConnection(connString);
            npgsqlConnection.Open();
        }

        public NpgsqlConnection Connection
        {
            get { return npgsqlConnection; }
        }

        public NpgsqlTransaction? Transaction
        {
            get { return sqlTransaction; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (sqlTransaction != null)
                    {
                        Commit();
                    }
                    npgsqlConnection?.Close();
                    npgsqlConnection?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void CreateTransaction()
        {
            sqlTransaction = npgsqlConnection.BeginTransaction();
        }

        public void Commit()
        {
            sqlTransaction?.Commit();
            sqlTransaction = null; // Reset the transaction after committing
        }

        public void Rollback()
        {
            sqlTransaction?.Rollback();
            sqlTransaction = null; // Reset the transaction after rollback
        }

        public UserRepository UserRepository()
        {
            if (userRepository == null)
            {
                userRepository = new UserRepository(this);
            }
            return userRepository;
        }

    }
}
