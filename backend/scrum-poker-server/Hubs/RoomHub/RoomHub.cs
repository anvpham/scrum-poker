using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using scrum_poker_server.HubModels;
using scrum_poker_server.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace scrum_poker_server.Hubs
{
    [Authorize(Policy = "OfficialUsers")]
    public class RoomHub : Hub
    {
        private readonly IPokingRoomManager _pokingRoomManager;

        public RoomHub(IPokingRoomManager pokingRoomManager)
        {
            _pokingRoomManager = pokingRoomManager;
        }

        public async Task Combine(string roomCode, int role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            var room = await _pokingRoomManager.GetRoomAsync(roomCode);
            var userName = Context.User.FindFirst(ClaimTypes.Name).Value;
            var userId = int.Parse(Context.User.FindFirst("UserId").Value);

            if (room == null)
            {
                var roomHubUser = new RoomHubUser(userName, userId, "standBy", (Role)role, 0);
                room = await _pokingRoomManager.AddRoomAsync(roomCode, roomHubUser);
                await Clients.Caller.SendAsync("joinRoom", new { users = room.Users, roomState = room.State, currentStoryPoint = room.CurrentStoryPoint });
            }
            else
            {
                room = await _pokingRoomManager.AddUserAsync(room.RoomCode, new RoomHubUser(userName, int.Parse(Context.User.FindFirst("UserId").Value), "standBy", (Role)role, 0));
                var users = room.Users;
                await Clients.GroupExcept(roomCode, Context.ConnectionId).SendAsync("newUserConnected", new { name = userName, id = userId, status = "standBy", point = 0, role });
                await Clients.Caller.SendAsync("joinRoom", new { users, roomState = room.State, currentStoryPoint = room.CurrentStoryPoint });
                await Clients.Caller.SendAsync("currentStoryChanged", new { id = room.CurrentStoryId });
            }
        }

        public async Task ChangeUserStatus(string roomCode, string status, int point)
        {
            var userId = int.Parse(Context.User.FindFirst("UserId").Value);

            await _pokingRoomManager.UpdateUserStatusAsync(roomCode, userId, status, point);
            await Clients.Group(roomCode).SendAsync("userStatusChanged", new { userId, status, point });
        }

        public async Task ChangeRoomState(string roomCode, string roomState)
        {
            await _pokingRoomManager.UpdateRoomStateAsync(roomCode, roomState);

            if (roomState == "revealed")
            {
                var users = await _pokingRoomManager.UpdateStatusForAllUsersAsync(roomCode, "revealed");

                await Clients.Group(roomCode).SendAsync("roomStateChanged", new { roomState, users });

                var recommendedPoint = await _pokingRoomManager.GetMostFrequentPointAsync(roomCode);

                await Clients.Group(roomCode).SendAsync("currentStoryPointChanged", new { point = recommendedPoint });
            }
            else if (roomState == "waiting")
            {
                var users = await _pokingRoomManager.UpdateStatusForAllUsersAsync(roomCode, "standBy");

                await Clients.Group(roomCode).SendAsync("roomStateChanged", new { roomState, users });
            }
            else
                await Clients.Group(roomCode).SendAsync("roomStateChanged", new { roomState });
        }

        public async Task AddStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("storyAdded", new { id });
            await _pokingRoomManager.AddStoryAsync(roomCode, id);
        }

        public async Task DeleteStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("storyDeleted", new { id });
            await _pokingRoomManager.RemoveStoryAsync(roomCode, id);
        }

        public async Task UpdateStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("storyUpdated", new { id });
        }

        public async Task ChangeCurrentStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("currentStoryChanged", new { id });
            await _pokingRoomManager.UpdateCurrentStoryAsync(roomCode, id);
        }

        public async Task ChangeCurrentStoryPoint(string roomCode, int point)
        {
            await Clients.Group(roomCode).SendAsync("currentStoryPointChanged", new { point });
            await _pokingRoomManager.UpdateCurrentStoryPointAsync(roomCode, point);
        }

        public async Task Reload(string roomCode)
        {
            await Clients.Group(roomCode).SendAsync("onStoriesImported");
        }

        public async Task RemoveFromChannel(string roomCode)
        {
            var userId = int.Parse(Context.User.FindFirst("UserId").Value);
            var room = await _pokingRoomManager.GetRoomAsync(roomCode);
            if(room.Users.FirstOrDefault(u => u.Id == userId) == null)
            {
                return;
            }
            else
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
                await Clients.Group(roomCode).SendAsync("userLeft", new { userId });
                await _pokingRoomManager.RemoveUserAsync(room.RoomCode, userId);
            }
        }
    }
}
