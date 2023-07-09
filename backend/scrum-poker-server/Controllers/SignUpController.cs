using Microsoft.AspNetCore.Mvc;
using scrum_poker_server.Data;
using scrum_poker_server.DTOs;
using scrum_poker_server.Models;
using scrum_poker_server.Services;
using scrum_poker_server.Utils;
using scrum_poker_server.Utils.Jwt;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace scrum_poker_server.Controllers
{
    [ApiController]
    [Route("api/signup")]
    public class SignUpController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IRoomService _roomService;
        private readonly IUnitOfWork _unitOfWork;

        public SignUpController(
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            IRoomService roomService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _roomService = roomService;
        }

        [Consumes("application/json")]
        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO data)
        {
            if (String.IsNullOrEmpty(data.Email))
            {
                var anonymousUser = new User()
                {
                    Name = data.UserName
                };

                _unitOfWork.UserRepository.Add(anonymousUser);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { jwtToken = _jwtService.GenerateToken(anonymousUser), expiration = 131399, name = data.UserName, userId = anonymousUser.Id });
            }

            bool isEmailExisted = await _unitOfWork.UserRepository.GetByEmailAsync(data.Email) != null;
            if (isEmailExisted)
                return StatusCode(409, new { error = "The email is already existed" });

            // Compute hash of the password
            var bytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(data.Password));
            string hashedPassword = BitConverter.ToString(bytes).Replace("-", "").ToLower();

            var user = new User()
            {
                Email = data.Email,
                Password = hashedPassword,
                Name = data.UserName
            };

            // Create a room for user
            var roomCode = await _roomService.GenerateRoomCodeAsync();

            var room = new Room
            {
                Owner = user,
                Code = roomCode,
                Name = $"{user.Name}'s room",
                Description = "Change room description here"
            };

            _unitOfWork.UserRoomRepository.Add(new UserRoom
            {
                User = user,
                Room = room,
                Role = Role.host
            });

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { jwtToken = _jwtService.GenerateToken(user), email = data.Email, expiration = 29, name = data.UserName, userId = user.Id, userRoomCode = roomCode });
        }
    }
}