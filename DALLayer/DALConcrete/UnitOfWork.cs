using System;
using System.Data.Common;
using DALLayer.DALInterfaces;
using DALLayer.Recources;

namespace DALLayer.DALConcrete
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IDataContextFactory _factory;
        private IDataContext _context;
        public DbTransaction Transaction { get; private set; }

        public UnitOfWork(IDataContextFactory factory) => _factory = factory;

        public void Commit()
        {
            if (Transaction != null)
            {
                try
                {
                    Transaction.Commit();
                }
                catch (Exception)
                {
                    Transaction.Rollback();
                }
                Transaction.Dispose();
                Transaction = null;
            }
            else
            {
                throw new NullReferenceException(ExceptionMessage.TryedCommitNotOpenedTransaction);
            }
        }

        /// <summary>
        /// Define a property of context class
        /// </summary>
        public IDataContext DataContext => _context ?? (_context = _factory.Context());

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        /// <returns>Transaction</returns>
        public DbTransaction BeginTransaction()
        {
            if (Transaction != null)
            {
                throw new NullReferenceException(ExceptionMessage.NotFinishedPreviousTransaction);
            }

            Transaction = _context.Connection.BeginTransaction();
            return Transaction;

        }


        public void Dispose()
        {
            Transaction?.Dispose();
            _context?.Dispose();
        }


    }
}
