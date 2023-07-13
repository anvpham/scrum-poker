using Moq;
using scrum_poker_server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class UserControllerTests
    {
        [Fact]
        public async Task Sign_Up_Test()
        {
            // Arrange
            var roomServiceMock = new Mock<IRoomService>();
            roomServiceMock
                .Setup(x => x.GenerateRoomCodeAsync())
                .ReturnsAsync("randomCode");

        }
    }
}