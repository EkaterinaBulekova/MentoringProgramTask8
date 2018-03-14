using System.Collections.Generic;
using System.Data.Common;
using DALLayer.Entities;

namespace DALLayer.IRepositories
{
    public interface IRepository<T> where T : Entity
    {
        int Insert(T entity, DbTransaction sqlTransaction);
        int Update(T entity, DbTransaction sqlTransaction);
        int Delete(object id, DbTransaction sqlTransaction);
        T GetById(object id);
        IEnumerable<T> GetAll();
    }
}
