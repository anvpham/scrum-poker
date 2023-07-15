using Microsoft.EntityFrameworkCore;
using scrum_poker_server.Models;
using System.Threading.Tasks;

namespace scrum_poker_server.Data.Repositories
{
    public interface IUserRoomRepository
    {
        public void Add(UserRoom userRoom);
        public Task<UserRoom?> GetByUserIdAndRoomCodeAsync(int userId, string roomCode);
        public Task<UserRoom?> GetByUserIdAndRoomIdAsync(int userId, int roomId);
    }

    public class UserRoomRepository : IUserRoomRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRoomRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(UserRoom userRoom)
        {
            _dbContext.Add(userRoom);
        }

        public async Task<UserRoom?> GetByUserIdAndRoomCodeAsync(int userId, string roomCode)
        {
            var userRoom = await _dbContext.UserRooms
                .Include(ur => ur.Room)
                .ThenInclude(r => r.Stories)
                .ThenInclude(s => s.SubmittedPointByUsers).Include(ur => ur.User).AsSplitQuery()
                .FirstOrDefaultAsync(ur => ur.UserID == userId && ur.Room.Code == roomCode);

            return userRoom;
        }

        public async Task<UserRoom> GetByUserIdAndRoomIdAsync(int userId, int roomId)
        {
            var userRoom = await _dbContext.UserRooms
                .Include(ur => ur.Room)
                .ThenInclude(r => r.Stories)
                .ThenInclude(s => s.SubmittedPointByUsers).Include(ur => ur.User).AsSplitQuery()
                .FirstOrDefaultAsync(ur => ur.UserID == userId && ur.Room.Id == roomId);

            return userRoom;
        }
    }
}
