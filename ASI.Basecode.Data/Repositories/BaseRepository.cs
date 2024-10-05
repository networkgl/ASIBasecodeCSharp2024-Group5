using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T>
        where T : class
    {
        private DbContext _dbContext;
        private DbSet<T> _table;

        public BaseRepository()
        {
            _dbContext = new TicketingSystemDBContext();
            _table = _dbContext.Set<T>();
        }

        public DbSet<T> Table { get { return _table; } }
        public ErrorCode Create(T t)
        {
            try
            {
                _table.Add(t);
                _dbContext.SaveChanges();
                return ErrorCode.Success;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
            {
                return ErrorCode.Duplicate;
            }
            catch (Exception)
            {
                return ErrorCode.Error;
            }
        }

        public ErrorCode Delete(object id)
        {
            try
            {
                var user = Get(id);
                _table.Remove(user);
                _dbContext.SaveChanges();
                return ErrorCode.Success;
            }
            catch (Exception)
            {
                return ErrorCode.Error;
                throw;
            }
        }

        public T Get(object id)
        {
            return _table.Find(id);
        }

        public List<T> GetAll()
        {
            return _table.ToList();
        }

        public ErrorCode Update(object id, T t)
        {
            try
            {
                var user = Get(id);
                _dbContext.Entry(user).CurrentValues.SetValues(t);
                _dbContext.SaveChanges();
                return ErrorCode.Success;
            }
            catch (Exception)
            {
                return ErrorCode.Error;
            }
        }

        List<T> IBaseRepository<T>.GetAll()
        {
            throw new System.NotImplementedException();
        }
    }
}
