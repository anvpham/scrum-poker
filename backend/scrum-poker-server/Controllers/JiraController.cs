using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using scrum_poker_server.Data;
using scrum_poker_server.DTOs;
using System;
using System.Threading.Tasks;
using scrum_poker_server.Models;
using System.Net;
using System.Linq;
using scrum_poker_server.Services;

namespace scrum_poker_server.Controllers
{
    [Route("api/jira"), Consumes("application/json")]
    [ApiController]
    public class JiraController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJiraService _jiraService;

        public JiraController(IUnitOfWork unitOfWork, IJiraService jiraService)
        {
            _unitOfWork = unitOfWork;
            _jiraService = jiraService;
        }

        [HttpPost, Route("addtoken"), Authorize(Policy = "OfficialUsers")]
        public async Task<IActionResult> AddToken([FromBody] JiraUserCredentials data)
        {
            if (data.JiraDomain.Contains("http") || !data.JiraDomain.Contains('.'))
                return StatusCode(404, new { error = "The domain is not valid" });
            if (data.JiraDomain.Length > 50 || data.JiraEmail.Length > 50 || data.APIToken.Length > 50)
                return StatusCode(409, new { error = "Fields are too long" });

            bool isDomainValid = false;
            try
            {
                isDomainValid = await _jiraService.IsJiraDomainValidAsync(data.JiraDomain);
            }
            catch (Exception)
            {
                return StatusCode(404, new { error = "The domain is not valid" });
            }

            if (!isDomainValid)
                return StatusCode(404, new { error = "The domain is not valid" });

            bool isJiraTokenValid = await _jiraService.IsUserJiraTokenValidAsync(data.JiraDomain, data.JiraEmail, data.APIToken);

            if (!isJiraTokenValid)
                return StatusCode(404, new { error = "The email or API token is not valid" });

            var userId = int.Parse(HttpContext.User.FindFirst("UserId").Value);

            var userRoom = await _unitOfWork.UserRoomRepository.GetByUserIdAndRoomCodeAsync(userId, data.RoomCode);

            if (userRoom.User.JiraToken != null)
            {
                userRoom.Room.Stories.ToList().ForEach(s =>
                {
                    s.SubmittedPointByUsers = null;
                });

                userRoom.Room.Stories.Clear();
            }

            userRoom.User.JiraToken = data.APIToken;
            userRoom.User.JiraDomain = data.JiraDomain;
            userRoom.Room.JiraDomain = data.JiraDomain;
            userRoom.User.JiraEmail = data.JiraEmail;

            await _unitOfWork.SaveChangesAsync();

            return StatusCode(201, new { jiraToken = data.APIToken, jiraDomain = data.JiraDomain });
        }

        [HttpPost, Route("fetchstories"), Authorize(Policy = "OfficialUsers")]
        public async Task<IActionResult> FetchStories([FromBody] FetchJiraStories data)
        {
            if (String.IsNullOrEmpty(data.Query))
            {
                return Ok(new { status = "NotOk" });
            }

            var userId = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

            var response = await _jiraService.GetStoriesAsync(user, data.Query);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(401);
            }

            var content = await response.Content.ReadAsStringAsync();

            return Ok(new { content, status = "Ok" });
        }

        [HttpPost, Route("addstory"), Authorize(Policy = "OfficialUsers")]
        public async Task<IActionResult> AddStory([FromBody] AddJiraStory data)
        {
            if (String.IsNullOrEmpty(data.IssueId))
            {
                return StatusCode(422);
            }

            var room = await _unitOfWork.RoomRepository.GetByIdAsync(data.RoomId);
            if (room == null)
            {
                return StatusCode(422, new { error = "The room does not exist" });
            }

            if (room.Stories.Count >= 10)
            {
                return Forbid();
            }

            var jiraStory = await _unitOfWork.StoryRepository.GetByRoomIdAndJiraIssueIdAsync(data.RoomId, data.IssueId);
            if (jiraStory != null)
            {
                return StatusCode(422, new { error = "You've already added this story" });
            }

            var userId = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

            JiraIssueResponse issue = await _jiraService.GetIssueAsync(user, data.IssueId);

            int point;

            if (issue.Fields.Customfield_10026 == null)
            {
                point = -1;
            }
            else
            {
                point = issue.Fields.Customfield_10026 > 0 ? (int)issue.Fields.Customfield_10026 : -1;
            }

            var story = new Story
            {
                Title = issue.Fields.Summary,
                Content = issue.RenderedFields.Description,
                Point = point,
                IsJiraStory = true,
                JiraIssueId = data.IssueId,
            };

            room.Stories.Add(story);
            await _unitOfWork.SaveChangesAsync();

            return StatusCode(201, new { storyId = story.Id, issueId = data.IssueId });
        }

        [HttpPost, Route("submitpoint"), Authorize(Policy = "OfficialUsers")]
        public async Task<IActionResult> SubmitPoint([FromBody] JiraSubmitPoint data)
        {
            if (data.IssueId == null || data.JiraDomain == null || data.JiraToken == null)
            {
                if (data.JiraDomain == null || data.JiraToken == null)
                {
                    return StatusCode(401);
                }

                return StatusCode(422);
            }
            var userId = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

            var response = await _jiraService
                .UpdateIssueAsync(user, data.IssueId, new JiraSubmitPointRequest { fields = new fields { customfield_10026 = data.Point } });

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return StatusCode(400);
                }

                return StatusCode(401);
            }

            return StatusCode(201);
        }
    }
}
