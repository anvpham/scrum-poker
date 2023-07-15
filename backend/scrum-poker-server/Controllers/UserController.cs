using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using scrum_poker_server.Data;
using scrum_poker_server.DTOs;
using scrum_poker_server.Models;
using scrum_poker_server.Services;
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace scrum_poker_server.Controllers
{
    [Route("api/[controller]"), Consumes("application/json")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IJiraService _jiraService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IRoomService _roomService;

        public UserController(
            IUnitOfWork unitOfWork,
            IJiraService jiraService,
            IJwtService jwtService,
            IRoomService roomService)
        {
            _unitOfWork = unitOfWork;
            _jiraService = jiraService;
            _jwtService = jwtService;
            _roomService = roomService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO data)
        {
            var bytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(data.Password));
            string hashedPassword = BitConverter.ToString(bytes).Replace("-", "").ToLower();

            var user = await _unitOfWork.UserRepository.GetByEmailAndPasswordAsync(data.Email, hashedPassword);

            if (user == null)
                return Unauthorized();

            var room = await _unitOfWork.RoomRepository.GetByUserIdAsync(user.Id);

            return Ok(new
            {
                jwtToken = _jwtService.GenerateToken(user),
                expiration = 29,
                name = user.Name,
                userId = user.Id,
                userRoomCode = room.Code,
                email = data.Email,
                jiraToken = user.JiraToken,
                jiraDomain = user.JiraDomain,
            });
        }

        [HttpPost("authenticate"), Authorize(Policy = "AllUsers")]
        public async Task<IActionResult> Authenticate()
        {
            var userId = int.Parse(HttpContext.User.FindFirst("UserId").Value);

            var room = await _unitOfWork.RoomRepository.GetByUserIdAsync(userId);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

            if (!string.IsNullOrEmpty(user.JiraDomain)
                || !string.IsNullOrEmpty(user.JiraToken)
                || !string.IsNullOrEmpty(user.JiraEmail))
            {
                bool tokenValid = await _jiraService.IsUserJiraTokenValidAsync(user.JiraDomain, user.JiraEmail, user.JiraToken);

                if (!tokenValid)
                {
                    user.JiraToken = null;
                    user.JiraEmail = null;

                    room.JiraDomain = null;

                    await _unitOfWork.SaveChangesAsync();
                }
            }

            return Ok(new
            {
                name = user.Name,
                userId = user.Id,
                userRoomCode = room.Code,
                email = user.Email,
                jiraToken = user.JiraToken,
                jiraDomain = user.JiraDomain,
            });
        }

        [HttpGet("refreshtoken"), Authorize(Policy = "AllUsers")]
        public async Task<IActionResult> RefreshToken()
        {
            var userId = int.Parse(HttpContext.User.FindFirst("UserId").Value);

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId)
                ?? throw new ApplicationException($"UserId {userId} not found");

            return Ok(new { jwtToken = _jwtService.GenerateToken(user), expiration = user.Email != null ? 29 : 131399, user.Email });
        }

        [HttpPost("signup")]
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

        [HttpPost("changename"), Authorize(Policy = "AllUsers")]
        public async Task<IActionResult> ChangeName([FromBody] ChangeNameDTO data)
        {
            var userId = int.Parse(HttpContext.User.FindFirst("UserId").Value);

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            user.Name = data.NewName;

            string email = null;
            var emailClaim = HttpContext.User.FindFirst(ClaimTypes.Email);
            if (emailClaim != null) email = emailClaim.Value;
            await _unitOfWork.SaveChangesAsync();

            return StatusCode(201, new { jwtToken = _jwtService.GenerateToken(user), name = data.NewName, userId = userId, expiration = email != null ? 29 : 131399, email });
        }
    }
}
