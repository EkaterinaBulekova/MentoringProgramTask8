using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DALLayer.DALInterfaces;
using DALLayer.Entities;
using DALLayer.Extentions;
using DALLayer.IRepositories;
using DALLayer.Recources;

namespace DALLayer.Repositories
{
    public abstract class BaseRepository<T, T1> : IRepository<T> where T : Entity, new() where T1 : Entity, new()
    {
        protected readonly DbConnection Conn;
        protected readonly IUnitOfWork Uow;


        /// <summary>
        /// Initialize the connection
        /// </summary>
        /// <param name="unitOfWork">UnitOfWork</param>
        protected BaseRepository(IUnitOfWork unitOfWork)
        {
            Uow = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            Conn = Uow.DataContext.Connection;
        }

        /// <summary>
        /// Base Method for Insert Data
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sqlTransaction"></param>
        /// <returns></returns>
        public int Insert(T entity, DbTransaction sqlTransaction)
        {
            int i;
            try
            {
                using (var cmd = Conn.CreateCommand())
                {
                    cmd.GetInsertCommand(entity);
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = sqlTransaction;
   
                    i = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new DataException(ExceptionMessage.InsertException,ex);
            }
            return i;
        }

        /// <summary>
        /// Base Method for Update Data
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sqlTransaction"></param>
        /// <returns></returns>
        public int Update(T entity, DbTransaction sqlTransaction)
        {
            var i = 0;
            if (!entity.IsValid(ValidateType.ForUpdate)) return i;
            using (var cmd = Conn.CreateCommand())
            {
                cmd.GetUpdateCommand(entity);
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = sqlTransaction;
                i = cmd.ExecuteNonQuery();
            }

            return i;
        }

        /// <summary>
        /// Base Method for Delete Data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sqlTransaction"></param>
        /// <returns></returns>
        public int Delete(object id, DbTransaction sqlTransaction)
        {
            int i;
            using (var cmd = Conn.CreateCommand())
            {
                cmd.GetDeleteCommand<T, T1>(id);
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = sqlTransaction;
                i = cmd.ExecuteNonQuery();
            }

            return i;
        }

        /// <summary>
        /// Base Method for Populate Data by key
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetById(object id)
        {
            using (var cmd = Conn.CreateCommand())
            {
                cmd.GetSelectByIdCommand<T, T1>(id);
                cmd.CommandType = CommandType.Text;
                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Map<T, T1>();
                }
            }
        }

        /// <summary>
        /// Base Method for Populate All Data
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetAll()
        {
            using (var cmd = Conn.CreateCommand())
            {
                cmd.GetSelectAllCommand<T>();
                cmd.CommandType = CommandType.Text;
                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Maps<T>();
                }
            }
        }
    }
}
