using System.Collections.Generic;
using System.Linq;

namespace scrum_poker_server.HubModels
{
    public class PokingRoom
    {
        public string RoomCode { get; set; }
        public int CurrentStoryId { get; set; }
        public int CurrentStoryPoint { get; set; } = -1;
        public Dictionary<int, int> PointsFrequency { get; set; } = new Dictionary<int, int>();
        public string State { get; set; } = "waiting";
        public List<RoomHubUser> Users { get; set; }
        public List<int> StoryIds { get; set; } = new List<int>();
    }
}
