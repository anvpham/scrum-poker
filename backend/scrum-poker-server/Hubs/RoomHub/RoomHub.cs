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
        private readonly PokingRoomManager _roomHubManager;

        public RoomHub(PokingRoomManager roomHubManager)
        {
            _roomHubManager = roomHubManager;
        }

        public async Task Combine(string roomCode, int role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            var room = _roomHubManager.FindRoom(roomCode);
            var userName = Context.User.FindFirst(ClaimTypes.Name).Value;
            var userId = int.Parse(Context.User.FindFirst("UserId").Value);

            if (room == null)
            {
                room = new PokingRoom(roomCode, new RoomHubUser(userName, userId, "standBy", (Role)role, 0), "waiting");
                _roomHubManager.Add(room);
                var users = room.GetUsers();
                await Clients.Caller.SendAsync("joinRoom", new { users, roomState = room.State, currentStoryPoint = room.CurrentStoryPoint });
            }
            else
            {
                room.AddUser(new RoomHubUser(userName, int.Parse(Context.User.FindFirst("UserId").Value), "standBy", (Role)role, 0));
                var users = room.GetUsers();
                await Clients.GroupExcept(roomCode, Context.ConnectionId).SendAsync("newUserConnected", new { name = userName, id = userId, status = "standBy", point = 0, role });
                await Clients.Caller.SendAsync("joinRoom", new { users, roomState = room.State, currentStoryPoint = room.CurrentStoryPoint });
                await Clients.Caller.SendAsync("currentStoryChanged", new { id = room.CurrentStoryId });
            }
        }

        public async Task ChangeUserStatus(string roomCode, string status, int point)
        {
            var userId = int.Parse(Context.User.FindFirst("UserId").Value);
            var room = _roomHubManager.FindRoom(roomCode);

            room.UpdateUserStatus(userId, status, point);

            await Clients.Group(roomCode).SendAsync("userStatusChanged", new { userId, status, point });
        }

        public async Task ChangeRoomState(string roomCode, string roomState)
        {
            var room = _roomHubManager.FindRoom(roomCode);
            room.State = roomState;

            if (roomState == "revealed")
            {
                var users = room.SetStatusForAllUsers("revealed");

                await Clients.Group(roomCode).SendAsync("roomStateChanged", new { roomState, users });

                var recommendedPoint = room.GetMostFrequentPoint();

                await Clients.Group(roomCode).SendAsync("currentStoryPointChanged", new { point = recommendedPoint });
            }
            else if (roomState == "waiting")
            {
                var users = room.SetStatusForAllUsers("standBy");

                await Clients.Group(roomCode).SendAsync("roomStateChanged", new { roomState, users });

                room.CurrentStoryPoint = -1;
                room.PointsFrequency.Clear();
            }
            else await Clients.Group(roomCode).SendAsync("roomStateChanged", new { roomState });
        }

        public async Task AddStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("storyAdded", new { id });
            var room = _roomHubManager.FindRoom(roomCode);
            room.AddStory(id);
        }

        public async Task DeleteStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("storyDeleted", new { id });
            var room = _roomHubManager.FindRoom(roomCode);
            room.RemoveStory(id);
        }

        public async Task UpdateStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("storyUpdated", new { id });
        }

        public async Task ChangeCurrentStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("currentStoryChanged", new { id });
            var room = _roomHubManager.FindRoom(roomCode);
            room.CurrentStoryId = id;
        }

        public async Task ChangeCurrentStoryPoint(string roomCode, int point)
        {
            await Clients.Group(roomCode).SendAsync("currentStoryPointChanged", new { point });
            var room = _roomHubManager.FindRoom(roomCode);
            room.CurrentStoryPoint = point;
        }

        public async Task Reload(string roomCode)
        {
            await Clients.Group(roomCode).SendAsync("onStoriesImported");
        }

        public async Task RemoveFromChannel(string roomCode)
        {
            var userId = int.Parse(Context.User.FindFirst("UserId").Value);
            var room = _roomHubManager.FindRoom(roomCode);
            if(room.GetUsers().FirstOrDefault(u => u.Id == userId) == null)
            {
                return;
            }
            else
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
                await Clients.Group(roomCode).SendAsync("userLeft", new { userId });
                room.RemoveUser(userId);
            }
        }
    }
}