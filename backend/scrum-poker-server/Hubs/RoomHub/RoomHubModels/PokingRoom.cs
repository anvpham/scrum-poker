using System.Collections.Generic;
using System.Linq;

namespace scrum_poker_server.HubModels
{
    public class PokingRoom
    {
        public string RoomCode { get; set; }

        public int CurrentStoryId { get; set; }

        public int CurrentStoryPoint { get; set; }

        public Dictionary<int, int> PointsFrequency { get; set; }

        public string State { get; set; }

        public List<RoomHubUser> Users { get; set; }

        public List<int> StoryIds { get; set; }

        public PokingRoom(string roomCode, RoomHubUser host, string roomState)
        {
            RoomCode = roomCode;
            State = roomState;
            CurrentStoryPoint = -1;
            Users = new List<RoomHubUser>();
            StoryIds = new List<int>();
            PointsFrequency = new Dictionary<int, int>();
            Users.Add(host);
        }

        public void AddUser(RoomHubUser user)
        {
            Users.Add(user);
        }

        public void AddStory(int storyId)
        {
            StoryIds.Add(storyId);
        }

        public void RemoveStory(int storyId)
        {
            StoryIds.Remove(storyId);
        }

        public void RemoveUser(int id)
        {
            var user = Users.Find(u => u.Id == id);
            Users.RemoveAt(Users.IndexOf(user));
        }

        public RoomHubUser[] GetUsers()
        {
            return Users.ToArray();
        }

        public void UpdateUserStatus(int userId, string status, int point)
        {
            var user = Users.FirstOrDefault(u => u.Id == userId);
            user.Status = status;
            user.Point = point;

            if (PointsFrequency.ContainsKey(point))
            {
                PointsFrequency[point]++;
            }
            else
            {
                PointsFrequency[point] = 1;
            }
        }

        public int GetMostFrequentPoint()
        {
            var mostFrequent = PointsFrequency.Values.Max();

            return PointsFrequency.FirstOrDefault(item => item.Value == mostFrequent).Key;
        }

        public List<RoomHubUser> SetStatusForAllUsers(string status)
        {
            Users.ForEach(u =>
            {
                u.Status = status;

                if (status == "standBy")
                {
                    u.Point = -1;
                }
            });

            return Users;
        }
    }
}