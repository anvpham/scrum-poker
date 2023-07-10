using scrum_poker_server.Models;

namespace scrum_poker_server.HubModels
{
    public class RoomHubUser
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public int Point { get; set; }

        public Role Role { get; set; }

        public RoomHubUser(string name, int id, string status, Role role, int point = 0)
        {
            Name = name;
            Status = status;
            Point = point;
            Role = role;
            Id = id;
        }
    }
}
