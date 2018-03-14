using System;
using System.Data;
using System.Data.Common;
using DALLayer.DALInterfaces;
using DALLayer.Entities;
using DALLayer.IRepositories;
using System.Collections.Generic;
using DALLayer.Extentions;

namespace DALLayer.Repositories
{
    public class OrderRepository : BaseRepository<Order, OrderDetail>, IOrderRepository
    {
        private readonly string _update = "UPDATE Orders SET {0}='{1}' WHERE OrderID={2}";
        public OrderRepository(IUnitOfWork uow) : base(uow) { }

        public int UpdateOrderStatus(int id, DbTransaction sqlTransaction, StatusType type)
        {
            int i;
            using (var cmd = Conn.CreateCommand())
            {

                var column = type != StatusType.New && type == StatusType.InWork ? "OrderDate" : "ShippedDate";
                cmd.CommandText =string.Format(_update, column, DateTime.Now, id);
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = sqlTransaction;
                i = cmd.ExecuteNonQuery();
            }
            return i;
        }

        /// <summary>
        /// Base Method for Populate All Data
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetAllStoredProc<T>(params object[] procParams) where T : Entity
        {
            using (var cmd = Conn.CreateCommand())
            {
                cmd.GetStoredProcCommand<T>(procParams);
                cmd.CommandType = CommandType.StoredProcedure;
                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Maps<T>();
                }
            }
        }
    }
}
