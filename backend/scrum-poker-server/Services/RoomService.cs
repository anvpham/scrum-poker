using Microsoft.EntityFrameworkCore;
using scrum_poker_server.Data;
using scrum_poker_server.Models;
using System;
using System.Threading.Tasks;

namespace scrum_poker_server.Services
{
    public interface IRoomService
    {
        public Task<string> GenerateRoomCodeAsync();
    }

    public class RoomService : IRoomService
    {
        private readonly AppDbContext _dbContext;

        public RoomService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateRoomCodeAsync()
        {
            var random = new Random();
            Room room = null;

            bool isRoomExisted = true;
            string randomResult, prefix, roomCode = "";

            while (isRoomExisted)
            {
                randomResult = random.Next(0, 999999).ToString();

                prefix = new string('0', 6 - randomResult.Length);

                roomCode = prefix + randomResult;

                room = await _dbContext.Rooms.FirstOrDefaultAsync(r => r.Code == roomCode);

                if (room == null) isRoomExisted = false;
            }

            return roomCode;
        }
    }
}