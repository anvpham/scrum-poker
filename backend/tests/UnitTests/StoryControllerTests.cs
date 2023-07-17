using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using scrum_poker_server.Controllers;
using scrum_poker_server.Data.Repositories;
using scrum_poker_server.Data;
using scrum_poker_server.Models;
using System.Security.Claims;
using Xunit;

namespace UnitTests
{
    public class StoryControllerTests
    {
        private readonly ControllerContext _controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim("UserId", "1") }))
            }
        };

        public StoryControllerTests()
        {

        }

        [Fact]
        public async Task get_story_null_return_404_notfound()
        {
            // Arrange
            var storyRepositoryMock = new Mock<IStoryRepository>();
            storyRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(default(Story));

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.StoryRepository).Returns(storyRepositoryMock.Object);

            var storyController = new StoryController(unitOfWorkMock.Object)
            {
                ControllerContext = _controllerContext
            };

            // Act
            var result = await storyController.Get(1);

            // Assert
            var notFoundResult = result as ObjectResult;

            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task get_story_not_null_return_200_ok()
        {
            // Arrange
            var storyRepositoryMock = new Mock<IStoryRepository>();
            storyRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Story());

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.StoryRepository).Returns(storyRepositoryMock.Object);

            var storyController = new StoryController(unitOfWorkMock.Object)
            {
                ControllerContext = _controllerContext
            };

            // Act
            var result = await storyController.Get(1);

            // Assert
            var okResult = result as OkObjectResult;

            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task add_story_room_null_return_422()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            roomRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(default(Room));

            var storyRepositoryMock = new Mock<IStoryRepository>();
            storyRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Story());

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.StoryRepository).Returns(storyRepositoryMock.Object);
            unitOfWorkMock
                .SetupGet(x => x.RoomRepository).Returns(roomRepositoryMock.Object);

            var storyController = new StoryController(unitOfWorkMock.Object)
            {
                ControllerContext = _controllerContext
            };

            // Act
            var result = await storyController.Add(new scrum_poker_server.DTOs.AddStoryDTO
            {
                RoomId = 1
            });

            // Assert
            var objectResult = result as ObjectResult;

            Assert.NotNull(objectResult);
            Assert.Equal(422, objectResult.StatusCode);
        }
    }
}
