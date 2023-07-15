using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using scrum_poker_server.Data;
using scrum_poker_server.DTOs;
using scrum_poker_server.Hubs;
using scrum_poker_server.Models;
using scrum_poker_server.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace scrum_poker_server.Controllers
{
    [ApiController]
    [Route("api/room"), Consumes("application/json")]
    public class RoomController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PokingRoomManager _pokingRoomManager;
        private readonly IRoomService _roomService;

        public RoomController(IUnitOfWork unitOfWork, PokingRoomManager pokingRoomManager,
            IRoomService roomService)
        {
            _unitOfWork = unitOfWork;
            _pokingRoomManager = pokingRoomManager;
            _roomService = roomService;
        }

        [Authorize(Policy = "OfficialUsers")]
        [HttpPost, Route("create")]
        public async Task<IActionResult> Create([FromBody] CreateRoomDTO data)
        {
            var roomCode = await _roomService.GenerateRoomCodeAsync();

            var user = await _unitOfWork.UserRepository.GetByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.Email).Value);

            var room = new Room
            {
                Owner = user,
                Code = roomCode,
                Name = data.RoomName,
                Description = data.Description
            };

            _unitOfWork.UserRoomRepository.Add(new UserRoom
            {
                User = user,
                Room = room,
                Role = Role.host
            });

            await _unitOfWork.SaveChangesAsync();

            return StatusCode(201, new { code = roomCode, roomId = room.Id });
        }

        [Authorize(Policy = "AllUsers")]
        [HttpGet, Route("{id}/stories")]
        public async Task<IActionResult> GetStories(int id)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(id);
            if (room == null)
                return NotFound(new { error = "The room doesn't exist" });

            int userId = int.Parse(HttpContext.User.FindFirst("UserId").Value);

            var userRoom = await _unitOfWork.UserRoomRepository.GetByUserIdAndRoomCodeAsync(id, room.Code);
            if (userRoom == null)
                return Forbid();

            var stories = new List<StoryDTO>();

            room.Stories.ToList().ForEach(s =>
            {
                var submittedPointByUsers = new List<DTOs.SubmittedPointByUser>();

                if (s.SubmittedPointByUsers != null)
                {
                    s.SubmittedPointByUsers.ToList().ForEach(s =>
                    {
                        submittedPointByUsers.Add(new DTOs.SubmittedPointByUser
                        {
                            UserId = s.UserId,
                            UserName = s.User.Name,
                            Point = s.Point
                        });
                    });
                }

                stories.Add(new StoryDTO
                {
                    Id = s.Id,
                    Title = s.Title,
                    Content = s.Content,
                    Assignee = s.Assignee,
                    Point = s.Point,
                    IsJiraStory = s.IsJiraStory,
                    JiraIssueId = s.JiraIssueId,
                    SubmittedPointByUsers = submittedPointByUsers,
                }
                );
            });

            return Ok(new { stories });
        }

        [Authorize(Policy = "AllUsers")]
        [HttpPost, Route("join")]
        public async Task<IActionResult> Join([FromBody] JoinRoomDTO data)
        {
            var room = await _unitOfWork.RoomRepository.GetByRoomCodeAsync(data.RoomCode);
            if (room == null)
                return NotFound(new { error = "The room doesn't exist" });

            var userId = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

            var userRoom = await _unitOfWork.UserRoomRepository.GetByUserIdAndRoomCodeAsync(userId, data.RoomCode);

            if (userRoom != null)
                return Ok(new { roomId = room.Id, roomCode = data.RoomCode, roomName = room.Name, description = room.Description, role = userRoom.Role, jiraDomain = room.JiraDomain });

            _unitOfWork.UserRoomRepository.Add(new UserRoom
            {
                User = user,
                Room = room,
                Role = Role.player
            });

            await _unitOfWork.SaveChangesAsync();

            return StatusCode(201, new { roomId = room.Id, roomCode = data.RoomCode, roomName = room.Name, description = room.Description, role = Role.player, jiraDomain = room.JiraDomain });
        }

        // This API is used to check the availability of a room (valid room code, full people)
        [Authorize(Policy = "OfficialUsers")]
        [HttpGet, Route("check/{roomCode}")]
        public async Task<IActionResult> CheckRoom(string roomCode)
        {
            var room = await _unitOfWork.RoomRepository.GetByRoomCodeAsync(roomCode);
            var userClaim = HttpContext.User.FindFirst("UserId");
            var userId = int.Parse(userClaim.Value);

            if (room == null)
            {
                return StatusCode(404);
            }
            else if (_pokingRoomManager.FindRoom(roomCode) == null)
            {
                return Ok();
            }
            else if (_pokingRoomManager.FindRoom(roomCode).Users.Count >= 6)
            {
                return StatusCode(403);
            }
            else if (_pokingRoomManager.FindRoom(roomCode).Users.Find(u => u.Id == userId) != null)
            {
                return StatusCode(409);
            }

            return Ok();
        }
    }
}