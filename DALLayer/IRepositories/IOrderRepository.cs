using System.Collections.Generic;
using System.Data.Common;
using DALLayer.Entities;

namespace DALLayer.IRepositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        int UpdateOrderStatus(int id, DbTransaction sqlTransaction, StatusType type);
        IEnumerable<T> GetAllStoredProc<T>(params object[] procParams) where T : Entity;
    }
}