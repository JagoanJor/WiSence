using System;
using System.Collections.Generic;

namespace API.Services
{
    public interface IService<T>
    {
        IEnumerable<T> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date);
        T GetById(Int64 id);
        T Create(T data);
        T Edit(T data);
        bool Delete(Int64 id, String userID);
    }
}