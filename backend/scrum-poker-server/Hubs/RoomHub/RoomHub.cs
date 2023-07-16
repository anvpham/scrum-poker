using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using scrum_poker_server.HubModels;
using scrum_poker_server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace scrum_poker_server.Hubs
{
    [Authorize(Policy = "OfficialUsers")]
    public class RoomHub : Hub
    {
        private readonly IPokingRoomManager _pokingRoomManager;

        public RoomHub(IPokingRoomManager roomHubManager)
        {
            _pokingRoomManager = roomHubManager;
        }

        public async Task Combine(string roomCode, int role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            var room = await _pokingRoomManager.GetRoomAsync(roomCode);
            var userName = Context.User.FindFirst(ClaimTypes.Name).Value;
            var userId = int.Parse(Context.User.FindFirst("UserId").Value);

            if (room == null)
            {
                room = new PokingRoom
                {
                    RoomCode = roomCode,
                    Users = new List<RoomHubUser> { new RoomHubUser(userName, userId, "standBy", (Role)role, 0) },
                };

                await Clients.Caller.SendAsync("joinRoom", new { users = room.Users, roomState = room.State, currentStoryPoint = room.CurrentStoryPoint });
                await _pokingRoomManager.AddUpdateRoomAsync(room);
            }
            else
            {
                room.Users.Add(new RoomHubUser(userName, int.Parse(Context.User.FindFirst("UserId").Value), "standBy", (Role)role, 0));

                await Clients.GroupExcept(roomCode, Context.ConnectionId).SendAsync("newUserConnected", new { name = userName, id = userId, status = "standBy", point = 0, role });
                await Clients.Caller.SendAsync("joinRoom", new { users = room.Users, roomState = room.State, currentStoryPoint = room.CurrentStoryPoint });
                await Clients.Caller.SendAsync("currentStoryChanged", new { id = room.CurrentStoryId });
                await _pokingRoomManager.AddUpdateRoomAsync(room);
            }
        }

        public async Task ChangeUserStatus(string roomCode, string status, int point)
        {
            var userId = int.Parse(Context.User.FindFirst("UserId").Value);
            var room = await _pokingRoomManager.GetRoomAsync(roomCode);

            room.UpdateUserStatus(userId, status, point);

            await Clients.Group(roomCode).SendAsync("userStatusChanged", new { userId, status, point });
            await _pokingRoomManager.AddUpdateRoomAsync(room);
        }

        public async Task ChangeRoomState(string roomCode, string roomState)
        {
            var room = await _pokingRoomManager.GetRoomAsync(roomCode);
            room.State = roomState;

            if (roomState == "revealed")
            {
                room.UpdateStatusForAllUsers("revealed");

                await Clients.Group(roomCode).SendAsync("roomStateChanged", new { roomState, users = room.Users });

                var recommendedPoint = room.GetMostFrequentPoint();

                await Clients.Group(roomCode).SendAsync("currentStoryPointChanged", new { point = recommendedPoint });
            }
            else if (roomState == "waiting")
            {
                room.UpdateStatusForAllUsers("standBy");

                await Clients.Group(roomCode).SendAsync("roomStateChanged", new { roomState, users = room.Users });

                room.CurrentStoryPoint = -1;
                room.PointsFrequency.Clear();
            }
            else await Clients.Group(roomCode).SendAsync("roomStateChanged", new { roomState });

            await _pokingRoomManager.AddUpdateRoomAsync(room);
        }

        public async Task AddStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("storyAdded", new { id });

            var room = await _pokingRoomManager.GetRoomAsync(roomCode);
            room.StoryIds.Add(id);

            await _pokingRoomManager.AddUpdateRoomAsync(room);
        }

        public async Task DeleteStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("storyDeleted", new { id });

            var room = await _pokingRoomManager.GetRoomAsync(roomCode);
            room.StoryIds.Remove(id);

            await _pokingRoomManager.AddUpdateRoomAsync(room);
        }

        public async Task UpdateStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("storyUpdated", new { id });
        }

        public async Task ChangeCurrentStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("currentStoryChanged", new { id });

            var room = await _pokingRoomManager.GetRoomAsync(roomCode);
            room.CurrentStoryId = id;

            await _pokingRoomManager.AddUpdateRoomAsync(room);
        }

        public async Task ChangeCurrentStoryPoint(string roomCode, int point)
        {
            await Clients.Group(roomCode).SendAsync("currentStoryPointChanged", new { point });

            var room = await _pokingRoomManager.GetRoomAsync(roomCode);
            room.CurrentStoryPoint = point;

            await _pokingRoomManager.AddUpdateRoomAsync(room);
        }

        public async Task Reload(string roomCode)
        {
            await Clients.Group(roomCode).SendAsync("onStoriesImported");
        }

        public async Task RemoveFromChannel(string roomCode)
        {
            var userId = int.Parse(Context.User.FindFirst("UserId").Value);
            var room = await _pokingRoomManager.GetRoomAsync(roomCode);
            if (room.Users.FirstOrDefault(u => u.Id == userId) == null)
            {
                return;
            }
            else
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
                await Clients.Group(roomCode).SendAsync("userLeft", new { userId });

                room.Users.RemoveAll(u => u.Id == userId);

                await _pokingRoomManager.AddUpdateRoomAsync(room);
            }
        }
    }
}
