using scrum_poker_server.Data.Caching;
using scrum_poker_server.HubModels;
using System;
using System.Threading.Tasks;

namespace scrum_poker_server.Hubs
{
    public interface IPokingRoomManager
    {
        public Task<PokingRoom> AddUpdateRoomAsync(PokingRoom pokingRoom);
        public Task<PokingRoom> GetRoomAsync(string roomCode);
    }

    public class PokingRoomManager : IPokingRoomManager
    {
        private readonly ICacheService _cacheService;

        public PokingRoomManager(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<PokingRoom> AddUpdateRoomAsync(PokingRoom pokingRoom)
        {
            await _cacheService.SetByIdAsync(CacheKey.PokingRoom, pokingRoom.RoomCode, pokingRoom);

            return pokingRoom;
        }

        public async Task<PokingRoom> GetRoomAsync(string roomCode)
        {
            var pokingRoom = await _cacheService.GetByIdAsync<PokingRoom>(CacheKey.PokingRoom, roomCode);
            return pokingRoom ?? null;
        }
    }
}
