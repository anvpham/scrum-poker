using Microsoft.Extensions.DependencyInjection;
using scrum_poker_server.Data;
using scrum_poker_server.DTOs;
using scrum_poker_server.Models;
using scrum_poker_server.Services;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IntegrationTests.Tests
{
    [Collection("TestWebAppFactoryCollection")]
    public class StoryControllerTests : IDisposable
    {
        private readonly TestWebAppFactory _factory;

        public StoryControllerTests(TestWebAppFactory factory)
        {
            _factory = factory;
        }

        // Cleanup in-memory db after each test
        public void Dispose()
        {
            var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureDeleted();
        }

        [Fact]
        public async Task get_story_null_return_404()
        {
            // Arrange
            var client = _factory.CreateClient();

            var user = new User
            {
                Email = "user@email.com",
                Name = "user",
                Password = "asdqwezxc"
            };

            var scope = _factory.Services.CreateScope();
            var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
            string userToken = jwtService.GenerateToken(user);

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userToken}");

            // Act
            var response = await client.GetAsync("api/story/get/1");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task get_story_not_null_return_200()
        {
            // Arrange
            var client = _factory.CreateClient();

            var scope = _factory.Services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var user = new User
            {
                Email = "user@email.com",
                Name = "user",
                Password = "asdqwezxc"
            };

            unitOfWork.UserRoomRepository.Add(new UserRoom
            {
                User = user,
                Room = new Room
                {
                    Id = 1,
                    Stories = new List<Story> { new Story { Id = 1 } }
                },
                Role = Role.host,
            });

            await unitOfWork.SaveChangesAsync();

            var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
            string userToken = jwtService.GenerateToken(user);

            // Act

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userToken}");

            var response = await client.GetAsync("api/story/get/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task add_story_room_not_found_return_422()
        {
            // Arrange
            var client = _factory.CreateClient();

            var user = new User
            {
                Email = "user@email.com",
                Name = "user",
                Password = "asdqwezxc"
            };

            var scope = _factory.Services.CreateScope();

            var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
            string userToken = jwtService.GenerateToken(user);

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userToken}");

            var addStoryData = new AddStoryDTO
            {
                Content = "test",
                RoomId = 1,
                Title = "test title"
            };

            // Act
            var response = await client.PostAsync("api/story/add", JsonContent.Create(addStoryData));

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task delete_story_story_not_found_return_404()
        {
            // Arrange
            var client = _factory.CreateClient();

            var user = new User
            {
                Email = "user@email.com",
                Name = "user",
                Password = "asdqwezxc"
            };

            var scope = _factory.Services.CreateScope();

            var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
            string userToken = jwtService.GenerateToken(user);

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userToken}");

            // Act
            var response = await client.DeleteAsync("api/story/delete/1");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
