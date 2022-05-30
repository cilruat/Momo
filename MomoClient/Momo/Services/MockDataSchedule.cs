using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Momo.Models;

namespace Momo.Services
{
    public class MockDataSchedule : IDataStore<Schedule>
    {
        readonly List<Schedule> items;

        public MockDataSchedule() { items = new List<Schedule>(); }

        public async Task<bool> AddItemAsync(Schedule item)
        {
            bool find = items.Any(i => i.Id == item.Id);
            if (!find)
                items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Schedule item, bool notExistAdd = false)
        {
            var findIdx = items.FindIndex(arg => arg.Id == item.Id);
            if (findIdx == -1)
            {
                if (notExistAdd)
                    items.Insert(0, item);
            }
            else
            {
                items.RemoveAt(findIdx);
                items.Insert(findIdx, item);
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = items.Where((Schedule arg) => arg.Id == id).FirstOrDefault();
            if (oldItem != null)
                items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<bool> SortItemAsync()
        {
            return await Task.FromResult(true);
        }

        public async Task<Schedule> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Schedule>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }

        public async Task<bool> Clear()
        {
            items.Clear();
            return await Task.FromResult(true);
        }

        public int GetCount() { return items.Count; }
    }
}