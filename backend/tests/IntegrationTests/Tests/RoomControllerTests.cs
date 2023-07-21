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
    public class RoomControllerTests : IDisposable
    {
        private readonly TestWebAppFactory _factory;
        private readonly IServiceScope _scope;

        public RoomControllerTests(TestWebAppFactory factory)
        {
            _factory = factory;
            _scope = _factory.Services.CreateScope();
        }

        // Cleanup in-memory db after each test
        public void Dispose()
        {
            var dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureDeleted();
        }

        [Fact]
        public async Task create_room_return_201()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = new User
            {
                Id = 1,
                Email = "user@email.com",
                Name = "user",
                Password = "asdqwezxc"
            };

            var unitOfWork = _scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            unitOfWork.UserRepository.Add(user);

            await unitOfWork.SaveChangesAsync();

            var jwtService = _scope.ServiceProvider.GetRequiredService<IJwtService>();
            string userToken = jwtService.GenerateToken(user);

            var createRoomData = new CreateRoomDTO
            {
                Description = "test description",
                RoomName = "test room name"
            };

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userToken}");

            // Act
            var response = await client.PostAsync("api/room", JsonContent.Create(createRoomData));

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task get_stories_room_not_found_return_404()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = new User
            {
                Id = 1,
                Email = "user@email.com",
                Name = "user",
                Password = "asdqwezxc"
            };

            var jwtService = _scope.ServiceProvider.GetRequiredService<IJwtService>();
            string userToken = jwtService.GenerateToken(user);

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userToken}");

            // Act
            var response = await client.GetAsync("api/room/1/stories");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
