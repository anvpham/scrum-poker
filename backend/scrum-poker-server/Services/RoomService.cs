using scrum_poker_server.Data;
using scrum_poker_server.Data.Repositories;
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
        private readonly IUnitOfWork _unitOfWork;

        public RoomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

                room = await _unitOfWork.RoomRepository.GetByRoomCodeAsync(roomCode);

                if (room == null) isRoomExisted = false;
            }

            return roomCode;
        }
    }
}