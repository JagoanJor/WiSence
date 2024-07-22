using API.Responses;
using System;
using System.Threading.Tasks;

namespace API.Services
{
    public interface IServiceAsync<T>
    {
        Task<ListResponse<T>> GetAllAsync(Int32 limit, Int32 page, Int32 total, String search, String sort, String filter, String date);
        Task<T> GetByIdAsync(Int64 id);
        Task<T> CreateAsync(T data);
        Task<T> EditAsync(T data);
        Task<bool> DeleteAsync(Int64 id, String userID);
    }
}