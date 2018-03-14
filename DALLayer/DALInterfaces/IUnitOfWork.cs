using System.Data.Common;

namespace DALLayer.DALInterfaces
{
    public interface IUnitOfWork
    {
        IDataContext DataContext { get; }

        DbTransaction Transaction { get; }

        DbTransaction BeginTransaction();

        void Commit();

        void Dispose();
    }
}
