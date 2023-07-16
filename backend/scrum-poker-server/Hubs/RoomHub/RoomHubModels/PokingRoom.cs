using System;
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

        public void UpdateUserStatus(int userId, string status, int point)
        {
            var user = Users.FirstOrDefault(u => u.Id == userId)
                ?? throw new ApplicationException($"UserId {userId} not found.");

            user.Status = status;
            user.Point = point;

            if (PointsFrequency.ContainsKey(point))
                PointsFrequency[point]++;
            else
                PointsFrequency[point] = 1;
        }

        public void UpdateStatusForAllUsers(string status)
        {
            Users.ForEach(user =>
            {
                user.Status = status;

                if (status == "standBy")
                    user.Point = -1;
            });

            if (status == "standBy")
            {
                CurrentStoryPoint = -1;
                PointsFrequency.Clear();
            }
        }

        public int GetMostFrequentPoint()
        {
            var mostFrequent = PointsFrequency.Values.Max();

            return PointsFrequency.FirstOrDefault(item => item.Value == mostFrequent).Key;
        }
    }
}
