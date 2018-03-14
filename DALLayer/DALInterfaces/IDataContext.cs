using System.Data.Common;

namespace DALLayer.DALInterfaces
{
    public interface IDataContext
    {
        DbConnection Connection { get; }

        void Dispose();
    }
}
