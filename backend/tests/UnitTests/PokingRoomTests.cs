using scrum_poker_server.HubModels;
using scrum_poker_server.Models;
using Xunit;

namespace UnitTests
{
    public class PokingRoomTests
    {
        [Fact]
        public void update_user_status_must_update()
        {
            // Arrange
            var pokingRoom = new PokingRoom
            {
                Users = new List<RoomHubUser>
                {
                    new RoomHubUser("user", 1, "asd", Role.host)
                }
            };

            // Act
            pokingRoom.UpdateUserStatus(1, "qwe", 10);

            // Assert
            Assert.Equal("qwe", pokingRoom.Users[0].Status);
            Assert.Equal(10, pokingRoom.Users[0].Point);
            Assert.Equal(10, pokingRoom.GetMostFrequentPoint());
            Assert.Equal(1, pokingRoom.PointsFrequency[10]);
        }

        [Fact]
        public void update_user_status_not_existed_must_throw_exception()
        {
            // Arrange
            var pokingRoom = new PokingRoom
            {
                Users = new List<RoomHubUser>
                {
                    new RoomHubUser("user", 1, "asd", Role.host)
                }
            };

            // Act
            var exception = Assert.Throws<ApplicationException>(() => pokingRoom.UpdateUserStatus(2, "qwe", 10));

            // Assert
            Assert.Equal("UserId 2 not found.", exception.Message);
        }

        [Fact]
        public void update_status_for_all_users_must_update()
        {
            // Arrange
            var pokingRoom = new PokingRoom
            {
                Users = new List<RoomHubUser>
                {
                    new RoomHubUser("user1", 1, "asd", Role.host),
                    new RoomHubUser("user2", 2, "asd", Role.player),
                }
            };
            pokingRoom.UpdateUserStatus(1, "qwe", 5);

            // Act
            pokingRoom.UpdateStatusForAllUsers("standBy");

            // Assert
            Assert.Equal("standBy", pokingRoom.Users[0].Status);
            Assert.Equal(-1, pokingRoom.Users[0].Point);
            Assert.Equal("standBy", pokingRoom.Users[1].Status);
            Assert.Equal(-1, pokingRoom.Users[1].Point);
            Assert.Equal(-1, pokingRoom.CurrentStoryPoint);
            Assert.Empty(pokingRoom.PointsFrequency);
        }

        [Fact]
        public void get_most_frequent_point_return_expected()
        {
            // Arrange
            var pokingRoom = new PokingRoom
            {
                Users = new List<RoomHubUser>
                {
                    new RoomHubUser("user1", 1, "asd", Role.host),
                    new RoomHubUser("user2", 2, "asd", Role.player),
                    new RoomHubUser("user3", 3, "asd", Role.player),
                }
            };

            pokingRoom.UpdateUserStatus(1, "asd", 5);
            pokingRoom.UpdateUserStatus(2, "asd", 8);
            pokingRoom.UpdateUserStatus(3, "asd", 8);

            // Act
            var mostFrequentPoint = pokingRoom.GetMostFrequentPoint();

            // Assert
            Assert.Equal(8, mostFrequentPoint);
        }
    }
}
