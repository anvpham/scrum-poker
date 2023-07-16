using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using scrum_poker_server.Controllers;
using scrum_poker_server.Data;
using scrum_poker_server.Data.Repositories;
using scrum_poker_server.Models;
using scrum_poker_server.Services;
using System.Security.Claims;
using Xunit;

namespace UnitTests
{
    public class UserControllerTests
    {
        private readonly ControllerContext _userControllerContext;
        private readonly IJwtService _jwtService;

        public UserControllerTests()
        {
            var claimsIdentity = new ClaimsIdentity(new List<Claim> { new Claim("UserId", "1") });
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            
            _userControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var jwtServiceMock = new Mock<IJwtService>();
            jwtServiceMock
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("token");

            _jwtService = jwtServiceMock.Object;
        }

        [Fact]
        public async Task Sign_Up_Test()
        {
            // Arrange
            var roomServiceMock = new Mock<IRoomService>();
            roomServiceMock
                .Setup(x => x.GenerateRoomCodeAsync())
                .ReturnsAsync("randomCode");

            var jiraServiceMock = new Mock<IJiraService>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.UserRepository).Returns(userRepositoryMock.Object);

            var userController = new UserController(unitOfWorkMock.Object, jiraServiceMock.Object, _jwtService, roomServiceMock.Object);
            userController.ControllerContext = _userControllerContext;

            // Act
            var result = await userController.SignUp(new scrum_poker_server.DTOs.SignUpDTO
            {
                UserName = "qwe"
            });

            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }
    }
}
