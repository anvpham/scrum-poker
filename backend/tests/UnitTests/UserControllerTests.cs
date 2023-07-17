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
        private readonly IJwtService _jwtService;
        private readonly IRoomService _roomService;
        private readonly ControllerContext _controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim("UserId", "1") }))
            }
        };

        public UserControllerTests()
        {
            var jwtServiceMock = new Mock<IJwtService>();
            jwtServiceMock
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("token");

            _jwtService = jwtServiceMock.Object;

            var roomServiceMock = new Mock<IRoomService>();
            roomServiceMock
                .Setup(x => x.GenerateRoomCodeAsync())
                .ReturnsAsync("randomCode");

            _roomService = roomServiceMock.Object;
        }

        [Fact]
        public async Task sign_up_email_valid_must_return_200_ok()
        {
            // Arrange
            var jiraServiceMock = new Mock<IJiraService>();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(default(User));

            var userRoomRepositoryMock = new Mock<IUserRoomRepository>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            unitOfWorkMock
                .SetupGet(x => x.UserRepository).Returns(userRepositoryMock.Object);

            unitOfWorkMock
                .SetupGet(x => x.UserRoomRepository).Returns(userRoomRepositoryMock.Object);

            var userController = new UserController(unitOfWorkMock.Object, jiraServiceMock.Object, _jwtService, _roomService)
            {
                ControllerContext = _controllerContext
            };

            // Act
            var result = await userController.SignUp(new scrum_poker_server.DTOs.SignUpDTO
            {
                Email = "validemail@email.com",
                UserName = "qwe",
                Password = "User123@"
            });

            var okResult = result as ObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task sign_up_email_null_must_return_200_ok()
        {
            // Arrange
            var jiraServiceMock = new Mock<IJiraService>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.UserRepository).Returns(userRepositoryMock.Object);

            var userController = new UserController(unitOfWorkMock.Object, jiraServiceMock.Object, _jwtService, _roomService)
            {
                ControllerContext = _controllerContext
            };

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

        [Fact]
        public async Task sign_up_email_exists_must_return_409_conflict()
        {
            // Arrange
            var jiraServiceMock = new Mock<IJiraService>();
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new User
                {
                    
                });

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.UserRepository).Returns(userRepositoryMock.Object);

            var userController = new UserController(unitOfWorkMock.Object, jiraServiceMock.Object, _jwtService, _roomService)
            {
                ControllerContext = _controllerContext
            };

            // Act
            var result = await userController.SignUp(new scrum_poker_server.DTOs.SignUpDTO
            {
                Email = "existingemail@email.com",
                UserName = "qwe",
                Password = "User123@"
            });

            var conflictResult = result as ObjectResult;

            // Assert
            Assert.NotNull(conflictResult);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact]
        public async Task login_username_password_not_valid_return_401_unauthorized()
        {
            // Arrange
            var jiraServiceMock = new Mock<IJiraService>();
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.GetByEmailAndPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(default(User));

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.UserRepository).Returns(userRepositoryMock.Object);

            var userController = new UserController(unitOfWorkMock.Object, jiraServiceMock.Object, _jwtService, _roomService)
            {
                ControllerContext = _controllerContext
            };

            // Act
            var result = await userController.Login(new scrum_poker_server.DTOs.LoginDTO
            {
                Email = "email@email.com",
                Password = "User123@"
            });

            var conflictResult = result as UnauthorizedResult;

            // Assert
            Assert.NotNull(conflictResult);
            Assert.Equal(401, conflictResult.StatusCode);
        }

        [Fact]
        public async Task login_username_password_valid_return_200_ok()
        {
            // Arrange
            var jiraServiceMock = new Mock<IJiraService>();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.GetByEmailAndPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new User { Id = 1 });

            var roomRepositoryMock = new Mock<IRoomRepository>();
            roomRepositoryMock
                .Setup(x => x.GetByUserIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Room());

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.UserRepository).Returns(userRepositoryMock.Object);
            unitOfWorkMock
                .SetupGet(x => x.RoomRepository).Returns(roomRepositoryMock.Object);


            var userController = new UserController(unitOfWorkMock.Object, jiraServiceMock.Object, _jwtService, _roomService)
            {
                ControllerContext = _controllerContext
            };

            // Act
            var result = await userController.Login(new scrum_poker_server.DTOs.LoginDTO
            {
                Email = "email@email.com",
                Password = "User123@"
            });

            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }
    }
}
