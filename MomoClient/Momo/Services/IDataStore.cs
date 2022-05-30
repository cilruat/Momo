using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Momo.Services
{
    public interface IDataStore<T>
    {
        Task<bool> AddItemAsync(T item);
        Task<bool> UpdateItemAsync(T item, bool notExistAdd = true);
        Task<bool> DeleteItemAsync(string id);
        Task<bool> SortItemAsync();
        Task<T> GetItemAsync(string id);
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
        Task<bool> Clear();

        int GetCount();
    }
}
