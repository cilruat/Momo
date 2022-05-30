using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Momo.Models;

namespace Momo.Services
{
    public class MockDataNotice : IDataStore<Notice>
    {
        readonly List<Notice> items;

        public MockDataNotice() { items = new List<Notice>(); }

        public async Task<bool> AddItemAsync(Notice item)
        {
            bool find = items.Any(i => i.Id == item.Id);
            if (!find)
                items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Notice item, bool notExistAdd = false)
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
            var oldItem = items.Where((Notice arg) => arg.Id == id).FirstOrDefault();
            if (oldItem != null)
                items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<bool> SortItemAsync()
        {
            items.Sort(delegate (Notice x, Notice y)
            {
                int x_id = int.Parse(x.Id);
                int y_id = int.Parse(y.Id);
                if (x_id > y_id) return -1;
                if (x_id < y_id) return 1;
                return 0;
            });

            return await Task.FromResult(true);
        }

        public async Task<Notice> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Notice>> GetItemsAsync(bool forceRefresh = false)
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