using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using scrum_poker_server.Data;
using scrum_poker_server.Utils.Jwt;
using System.Net.Http;
using System.Threading.Tasks;

namespace scrum_poker_server.Controllers
{
    [Route("api/authenticate")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        public AppDbContext _dbContext { get; set; }

        public JwtTokenGenerator JwtTokenGenerator { get; set; }

        private readonly IHttpClientFactory _clientFactory;

        public AuthenticateController(AppDbContext dbContext, JwtTokenGenerator jwtTokenGenerator, IHttpClientFactory clientFactory)
        {
            _dbContext = dbContext;
            JwtTokenGenerator = jwtTokenGenerator;
            _clientFactory = clientFactory;
        }

        [Authorize(Policy = "AllUsers")]
        public async Task<IActionResult> Authenticate()
        {
            var userId = HttpContext.User.FindFirst("UserId").Value;
            var room = await _dbContext.Rooms.FirstOrDefaultAsync(r => r.UserId.ToString() == userId);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            var jiraToken = user.JiraToken;
            var jiraDomain = user.JiraDomain;

            if (jiraToken != null && jiraDomain != null)
            {
                var client = _clientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://{jiraDomain}/rest/api/3/myself");
                request.Headers.Add("Authorization", $"Basic {jiraToken}");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    user.JiraToken = null;
                    room.JiraDomain = null;
                    await _dbContext.SaveChangesAsync();
                }
            }

            return Ok(new
            {
                name = user.Name,
                userId = user.Id,
                userRoomCode = room.Code,
                email = user.Email,
                jiraToken = user.JiraToken,
                jiraDomain = user.JiraDomain,
            }); ;
        }
    }
}
