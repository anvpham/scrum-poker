using scrum_poker_server.Data.Repositories;
using System.Threading.Tasks;

namespace scrum_poker_server.Data
{
    public interface IUnitOfWork
    {
        public Task SaveChangesAsync();
        public IUserRepository UserRepository { get; }
        public IUserRoomRepository UserRoomRepository { get; }
        public IRoomRepository RoomRepository { get; }
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private IUserRepository _userRepository;
        private IUserRoomRepository _userRoomRepository;
        private IRoomRepository _roomRepository;

        public IUserRepository UserRepository { get => _userRepository ??= new UserRepository(_dbContext); }
        public IUserRoomRepository UserRoomRepository { get => _userRoomRepository ??= new UserRoomRepository(_dbContext); }
        public IRoomRepository RoomRepository { get => _roomRepository ??= new RoomRepository(_dbContext); }

        public UnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}