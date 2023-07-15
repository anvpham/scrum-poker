using Microsoft.EntityFrameworkCore;
using scrum_poker_server.Models;
using System.Threading.Tasks;

namespace scrum_poker_server.Data.Repositories
{
    public interface IRoomRepository
    {
        public Task<Room?> GetByIdAsync(int id);
        public Task<Room?> GetByRoomCodeAsync(string roomCode);
        public Task<Room?> GetByUserIdAsync(int userId);
    }

    public class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _dbContext;

        public RoomRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Room?> GetByIdAsync(int id)
        {
            return await _dbContext.Rooms
                .Include(r => r.Stories)
                .ThenInclude(s => s.SubmittedPointByUsers)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Room?> GetByRoomCodeAsync(string roomCode)
        {
            return await _dbContext.Rooms.FirstOrDefaultAsync(r => r.Code == roomCode);
        }

        public async Task<Room?> GetByUserIdAsync(int userId)
        {
            return await _dbContext.Rooms.FirstOrDefaultAsync(r => r.UserId == userId);
        }
    }
}
