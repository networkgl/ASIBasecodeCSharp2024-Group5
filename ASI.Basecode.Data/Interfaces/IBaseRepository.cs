using System.Collections.Generic;

namespace ASI.Basecode.Data.Interfaces
{
    public enum ErrorCode
    {
        Success, Error, Duplicate, ForeignKeyConstraintError
    }
    public interface IBaseRepository<T>
    {
        T Get(object id);
        List<T> GetAll();
        ErrorCode Create(T t);
        ErrorCode Update(object id, T t);
        ErrorCode Delete(object id);
    }
}
