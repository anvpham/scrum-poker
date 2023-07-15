using scrum_poker_server.Data.Caching;
using scrum_poker_server.HubModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace scrum_poker_server.Hubs
{
    public interface IPokingRoomManager
    {
        public Task<PokingRoom> AddRoomAsync(string roomCode, RoomHubUser host);
        public Task<PokingRoom> AddStoryAsync(string roomCode, int storyId);
        public Task<PokingRoom> AddUserAsync(string roomCode, RoomHubUser user);
        public Task RemoveStoryAsync(string roomCode, int storyId);
        public Task RemoveUserAsync(string roomCode, int userId);
        public Task UpdateUserStatusAsync(string roomCode, int userId, string status, int point);
        public Task UpdateRoomStateAsync(string roomCode, string roomState);
        public Task UpdateCurrentStoryAsync(string roomCode, int id);
        public Task UpdateCurrentStoryPointAsync(string roomCode, int point);
        public Task<List<RoomHubUser>> UpdateStatusForAllUsersAsync(string roomCode, string status);
        public Task<PokingRoom> GetRoomAsync(string roomCode);
        public Task<int> GetMostFrequentPointAsync(string roomCode);
    }

    public class PokingRoomManager : IPokingRoomManager
    {
        private readonly ICacheService _cacheService;

        public PokingRoomManager(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<PokingRoom> AddRoomAsync(string roomCode, RoomHubUser host)
        {
            var pokingRoom = new PokingRoom
            {
                RoomCode = roomCode,
                Users = new List<RoomHubUser> { host }
            };

            await _cacheService.SetByIdAsync(CacheKey.PokingRoom, roomCode, pokingRoom);

            return pokingRoom;
        }

        public async Task<PokingRoom> AddStoryAsync(string roomCode, int storyId)
        {
            var pokingRoom = await GetRoomAsync(roomCode);
            if (pokingRoom == null)
                throw new ApplicationException($"PokingRoom {roomCode} not found");

            pokingRoom.StoryIds.Add(storyId);

            await _cacheService.SetByIdAsync(CacheKey.PokingRoom, roomCode, pokingRoom);

            return pokingRoom;
        }

        public async Task<PokingRoom> AddUserAsync(string roomCode, RoomHubUser user)
        {
            var pokingRoom = await GetRoomAsync(roomCode);
            if (pokingRoom == null)
                throw new ApplicationException($"PokingRoom {roomCode} not found");

            pokingRoom.Users.Add(user);

            await _cacheService.SetByIdAsync(CacheKey.PokingRoom, roomCode, pokingRoom);

            return pokingRoom;
        }

        public async Task<int> GetMostFrequentPointAsync(string roomCode)
        {
            var pokingRoom = await GetRoomAsync(roomCode);
            if (pokingRoom == null)
                throw new ApplicationException($"PokingRoom {roomCode} not found");

            var mostFrequent = pokingRoom.PointsFrequency.Values.Max();

            return pokingRoom.PointsFrequency.FirstOrDefault(item => item.Value == mostFrequent).Key;
        }

        public async Task<PokingRoom> GetRoomAsync(string roomCode)
        {
            var pokingRoom = await _cacheService.GetByIdAsync<PokingRoom>(CacheKey.PokingRoom, roomCode);
            return pokingRoom ?? null;
        }

        public async Task RemoveStoryAsync(string roomCode, int storyId)
        {
            var pokingRoom = await GetRoomAsync(roomCode);
            if (pokingRoom == null)
                throw new ApplicationException($"PokingRoom {roomCode} not found");

            pokingRoom.StoryIds.Remove(storyId);

            await _cacheService.SetByIdAsync(CacheKey.PokingRoom, roomCode, pokingRoom);
        }

        public async Task RemoveUserAsync(string roomCode, int userId)
        {
            var pokingRoom = await GetRoomAsync(roomCode);
            if (pokingRoom == null)
                throw new ApplicationException($"PokingRoom {roomCode} not found");

            var user = pokingRoom.Users.Find(u => u.Id == userId);
            pokingRoom.Users.RemoveAt(pokingRoom.Users.IndexOf(user));

            await _cacheService.SetByIdAsync(CacheKey.PokingRoom, roomCode, pokingRoom);
        }

        public async Task UpdateCurrentStoryAsync(string roomCode, int id)
        {
            var pokingRoom = await GetRoomAsync(roomCode);
            if (pokingRoom == null)
                throw new ApplicationException($"PokingRoom {roomCode} not found");

            pokingRoom.CurrentStoryId = id;

            await _cacheService.SetByIdAsync(CacheKey.PokingRoom, roomCode, pokingRoom);
        }

        public async Task<List<RoomHubUser>> UpdateStatusForAllUsersAsync(string roomCode, string status)
        {
            var pokingRoom = await GetRoomAsync(roomCode);
            if (pokingRoom == null)
                throw new ApplicationException($"PokingRoom {roomCode} not found");

            pokingRoom.Users.ForEach(u =>
            {
                u.Status = status;

                if (status == "standBy")
                {
                    u.Point = -1;
                }
            });

            if (status == "standBy")
            {
                pokingRoom.CurrentStoryPoint = -1;
                pokingRoom.PointsFrequency.Clear();
            }

            await _cacheService.SetByIdAsync(CacheKey.PokingRoom, roomCode, pokingRoom);

            return pokingRoom.Users;
        }

        public async Task UpdateRoomStateAsync(string roomCode, string roomState)
        {
            var pokingRoom = await GetRoomAsync(roomCode);
            if (pokingRoom == null)
                throw new ApplicationException($"PokingRoom {roomCode} not found");

            pokingRoom.State = roomState;

            await _cacheService.SetByIdAsync(CacheKey.PokingRoom, roomCode, pokingRoom);
        }

        public async Task UpdateUserStatusAsync(string roomCode, int userId, string status, int point)
        {
            var pokingRoom = await GetRoomAsync(roomCode);
            if (pokingRoom == null)
                throw new ApplicationException($"PokingRoom {roomCode} not found");

            var user = pokingRoom.Users.FirstOrDefault(u => u.Id == userId);
            user.Status = status;
            user.Point = point;

            if (pokingRoom.PointsFrequency.ContainsKey(point))
            {
                pokingRoom.PointsFrequency[point]++;
            }
            else
            {
                pokingRoom.PointsFrequency[point] = 1;
            }

            await _cacheService.SetByIdAsync(CacheKey.PokingRoom, roomCode, pokingRoom);
        }

        public async Task UpdateCurrentStoryPointAsync(string roomCode, int point)
        {
            var pokingRoom = await GetRoomAsync(roomCode);
            if (pokingRoom == null)
                throw new ApplicationException($"PokingRoom {roomCode} not found");

            pokingRoom.CurrentStoryPoint = point;

            await _cacheService.SetByIdAsync(CacheKey.PokingRoom, roomCode, pokingRoom);
        }
    }
}
