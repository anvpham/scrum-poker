using Microsoft.Extensions.Caching.Distributed;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace scrum_poker_server.Data.Caching
{
    public interface ICacheService
    {
        public Task<T> GetAsync<T>(string key);
        public Task<T> GetByIdAsync<T>(string key, string id);
        public Task SetAsync(string key, object value);
        public Task SetByIdAsync(string key, string id, object value);
        public Task RemoveAsync(string key, object value);
        public Task RemoveByIdAsync(string key, string id, object value);
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        private async Task<byte[]?> GetBytesAsync(object value)
        {
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, value);
            var bytes = stream.ToArray();

            return bytes;
        }

        public async Task SetAsync(string key, object value)
        {
            var bytes = await GetBytesAsync(value);
            if (bytes != null)
                await _cache.SetAsync(key, bytes);
        }

        public async Task SetByIdAsync(string key, string id, object value)
        {
            var bytes = await GetBytesAsync(value);
            await _cache.SetAsync($"{key}:{id}", bytes);
        }

        public async Task RemoveAsync(string key, object value)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task RemoveByIdAsync(string key, string id, object value)
        {
            await _cache.RemoveAsync($"{key}:{id}");
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var bytes = await _cache.GetAsync(key);
            if (bytes == null)
                return default;

            return await JsonSerializer.DeserializeAsync<T>(new MemoryStream(bytes));
        }

        public async Task<T> GetByIdAsync<T>(string key, string id)
        {
            var bytes = await _cache.GetAsync($"{key}:{id}");
            if (bytes == null)
                return default;

            return await JsonSerializer.DeserializeAsync<T>(new MemoryStream(bytes));
        }
    }
}
