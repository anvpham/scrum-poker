using Microsoft.Extensions.DependencyInjection;
using scrum_poker_server.Data;
using scrum_poker_server.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IntegrationTests
{
    [Collection("TestWebAppFactoryCollection")]
    public class UserControllerTests : IDisposable
    {
        private readonly TestWebAppFactory _factory;

        public UserControllerTests(TestWebAppFactory factory)
        {
            _factory = factory;
        }

        // Cleanup in-memory db after each test
        public void Dispose()
        {
            var scope = _factory.Services.CreateScope(); // Dispose in-memory db
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureDeleted();
        }

        [Fact]
        public async Task login_email_password_not_valid_return_401()
        {
            // Arrange
            var client = _factory.CreateClient();

            var loginData = new LoginDTO
            {
                Email = "email@email.com",
                Password = "User123@"
            };

            // Act
            var response = await client.PostAsync("api/user/login", JsonContent.Create(loginData));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task sign_up_and_login_return_200()
        {
            // Arrange
            var client = _factory.CreateClient();

            var signupData = new SignUpDTO
            {
                Email = "email@email.com",
                Password = "User123@",
                UserName = "test",                
            };

            var loginData = new LoginDTO
            {
                Email = "email@email.com",
                Password = "User123@"
            };

            // Act
            var signUpResponse = await client.PostAsync("api/user/signup", JsonContent.Create(signupData));
            var loginResponse = await client.PostAsync("api/user/login", JsonContent.Create(loginData));

            // Assert
            Assert.Equal(HttpStatusCode.OK, signUpResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        }

        [Fact]
        public async Task sign_up_without_email_password_return_200()
        {
            // Arrange
            var client = _factory.CreateClient();

            var signupData = new SignUpDTO
            {
                UserName = "test",
            };

            // Act
            var response = await client.PostAsync("api/user/signup", JsonContent.Create(signupData));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
