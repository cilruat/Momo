using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Momo.Models;

namespace Momo.Services
{
    public class MockDataChatRoom : IDataStore<ChatRoom>
    {
        readonly List<ChatRoom> items;

        public MockDataChatRoom() { items = new List<ChatRoom>(); }


        public async Task<bool> AddItemAsync(ChatRoom item)
        {
            bool find = items.Any(i => i.Id == item.Id);
            if (!find)
                items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(ChatRoom item, bool notExistAdd = false)
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
            var oldItem = items.Where((ChatRoom arg) => arg.Id == id).FirstOrDefault();
            if (oldItem != null)
                items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<bool> SortItemAsync()
        {
            items.Sort(delegate (ChatRoom x, ChatRoom y)
            {
                DateTime x_time = DateTime.Parse(x.LastTime);
                DateTime y_time = DateTime.Parse(y.LastTime);
                
                return -1 * x_time.CompareTo(y_time);
            });

            return await Task.FromResult(true);
        }

        public async Task<ChatRoom> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<ChatRoom>> GetItemsAsync(bool forceRefresh = false)
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