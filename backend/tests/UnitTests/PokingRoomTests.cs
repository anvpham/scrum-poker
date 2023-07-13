using scrum_poker_server.HubModels;
using scrum_poker_server.Models;
using Xunit;

namespace UnitTests
{
    public class PokingRoomTests
    {
        [Fact]
        public void Add_User_List_Must_Contain_User()
        {
            // Arrange
            var user1 = new RoomHubUser("Test", 1, "ready", Role.host, 3);
            var user2 = new RoomHubUser("Test", 2, "ready", Role.player, 3);
            var pokingRoom = new PokingRoom("123", user1, "waiting");

            // Act
            pokingRoom.AddUser(user2);

            // Assert
            Assert.Equal(2, pokingRoom.Users.Count);
            Assert.Equal(user2.Name, pokingRoom.Users[1].Name);
            Assert.Equal(user2.Id, pokingRoom.Users[1].Id);
            Assert.Equal(user2.Status, pokingRoom.Users[1].Status);
            Assert.Equal(user2.Role, pokingRoom.Users[1].Role);
            Assert.Equal(user2.Point, pokingRoom.Users[1].Point);
        }

        [Fact]
        public void Add_Story_List_Must_Contain_Story()
        {
            // Arrange
            var user1 = new RoomHubUser("Test", 1, "ready", Role.host, 3);

            int storyId = 1;

            var pokingRoom = new PokingRoom("123", user1, "waiting");

            // Act
            pokingRoom.AddStory(storyId);

            // Assert
            Assert.Single(pokingRoom.StoryIds);
        }

        [Fact]
        public void Remove_Story_List_Must_Not_Contain_Story()
        {
            // Arrange
            var user1 = new RoomHubUser("Test", 1, "ready", Role.host, 3);

            int storyId = 1;

            var pokingRoom = new PokingRoom("123", user1, "waiting");

            // Act
            pokingRoom.AddStory(storyId);
            pokingRoom.RemoveStory(storyId);

            // Assert
            Assert.Empty(pokingRoom.StoryIds);
        }
    }
}