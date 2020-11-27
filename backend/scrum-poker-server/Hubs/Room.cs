﻿using Microsoft.AspNetCore.SignalR;
using scrum_poker_server.HubModels;
using scrum_poker_server.HubServices;
using scrum_poker_server.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace scrum_poker_server.Hubs
{
    public class Room : Hub
    {
        public RoomService _roomService { get; set; }

        public Room(RoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task Combine(string roomCode, string name, int role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            var room = _roomService.FindRoom(roomCode);

            if (room == null)
            {
                room = new PokingRoom(roomCode, new User(name, "standBy", (Role)role, 0), "waiting");
                _roomService.Add(room);
                var users = room.GetUsers();
                await Clients.Caller.SendAsync("firstTimeJoin", new { users, roomState = room.State });
            }
            else
            {
                room.AddUser(new User(name, "standBy", (Role)role, 0));
                var users = room.GetUsers();
                await Clients.GroupExcept(roomCode, Context.ConnectionId).SendAsync("newUserConnected", new { name, status = "standBy", point = 0 });
                await Clients.Caller.SendAsync("firstTimeJoin", new { users, roomState = room.State });
                await Clients.Caller.SendAsync("currentStoryChanged", new { id = room.CurrentStoryId });
            }
        }

        public async Task ChangeUserStatus(string roomCode, string name, string status, int point)
        {
            var room = _roomService.FindRoom(roomCode);
            var user = room.Users.FirstOrDefault(u => u.Name == name);
            user.Status = status;
            user.Point = point;
            await Clients.Group(roomCode).SendAsync("userStatusChanged", new { name, status, point });
        }

        public async Task ChangeRoomState(string roomCode, string roomState)
        {
            var room = _roomService.FindRoom(roomCode);
            room.State = roomState;

            if (roomState == "revealed")
            {
                room.Users.ForEach(u => u.Status = "revealed");
                var users = room.GetUsers();
                await Clients.Group(roomCode).SendAsync("roomStateChanged", new { users, roomState });
            }
            else if (roomState == "waiting")
            {
                room.Users.ForEach(u =>
                {
                    u.Status = "standBy";
                    u.Point = -1;
                });
                var users = room.GetUsers();
                await Clients.Group(roomCode).SendAsync("roomStateChanged", new { users, roomState });
            }
            else await Clients.Group(roomCode).SendAsync("roomStateChanged", new { roomState });
        }

        public async Task AddStory(string roomCode, int id)
        {
            var room = _roomService.FindRoom(roomCode);
            room.AddStory(id);
            await Clients.Group(roomCode).SendAsync("storyAdded", new { id });
        }

        public async Task UpdateStory(string roomCode, int id)
        {
            await Clients.Group(roomCode).SendAsync("storyUpdated", new { id });
        }

        public async Task ChangeCurrentStory(string roomCode, int id)
        {
            var room = _roomService.FindRoom(roomCode);
            room.CurrentStoryId = id;
            await Clients.Group(roomCode).SendAsync("currentStoryChanged", new { id });
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }
    }
}
