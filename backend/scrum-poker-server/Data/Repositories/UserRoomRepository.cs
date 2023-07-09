using scrum_poker_server.Models;

namespace scrum_poker_server.Data.Repositories
{
    public interface IUserRoomRepository
    {
        public void Add(UserRoom userRoom);
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
    }
}