using scrum_poker_server.HubModels;
using System.Collections.Generic;
using System.Linq;

namespace scrum_poker_server.Hubs
{
    public class PokingRoomManager
    {
        public List<PokingRoom> Rooms { get; set; }

        public PokingRoomManager()
        {
            Rooms = new List<PokingRoom>();
        }

        public void Add(PokingRoom newRoom)
        {
            Rooms.Add(newRoom);
        }

        public PokingRoom FindRoom(string roomCode)
        {
            var room = Rooms.FirstOrDefault(r => r.RoomCode == roomCode);
            return room ?? null;
        }
    }
}