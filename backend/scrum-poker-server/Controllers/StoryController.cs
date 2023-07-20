using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using scrum_poker_server.Data;
using scrum_poker_server.DTOs;
using scrum_poker_server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace scrum_poker_server.Controllers
{
    [ApiController]
    [Route("api/story"), Consumes("application/json")]
    public class StoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public StoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Policy = "AllUsers")]
        [HttpGet, Route("get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var story = await _unitOfWork.StoryRepository.GetByIdAsync(id);
            if (story == null)
            {
                return StatusCode(404, new { error = "The story is not existed" });
            }

            var submittedPointByUsers = new List<DTOs.SubmittedPointByUser>();

            if (story.SubmittedPointByUsers != null)
            {
                story.SubmittedPointByUsers.ToList().ForEach(s =>
                {
                    submittedPointByUsers.Add(new DTOs.SubmittedPointByUser
                    {
                        UserId = s.UserId,
                        Point = s.Point,
                        UserName = s.User.Name,
                    });
                });
            }

            return Ok(new { id, title = story.Title, content = story.Content, point = story.Point, isJiraStory = story.IsJiraStory, jiraIssueId = story.JiraIssueId, submittedPointByUsers });
        }

        [Authorize(Policy = "OfficialUsers")]
        [HttpPost, Route("add")]
        public async Task<IActionResult> Add([FromBody] AddStoryDTO data)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(data.RoomId);
            if (room == null)
            {
                return StatusCode(422, new { error = "The room does not exist" });
            }

            if (room.Stories.Count >= 20)
            {
                return Forbid();
            }

            if (data.IsJiraStory)
            {
                var jiraStory = await _unitOfWork.StoryRepository.GetByRoomIdAndJiraIssueIdAsync(data.RoomId, data.JiraIssueId);
                if (jiraStory != null)
                {
                    return StatusCode(422, new { error = "You've already added this story" });
                }
            }

            var userId = Int32.Parse(HttpContext.User.FindFirst("UserId").Value);
            var userRoom = await _unitOfWork.UserRoomRepository.GetByUserIdAndRoomCodeAsync(userId, room.Code);

            if (userRoom == null) return Forbid();
            else if (userRoom.Role != Role.host) return Forbid();

            var story = new Story { Title = data.Title, Content = data.Content, Point = -1, IsJiraStory = data.IsJiraStory };
            if (data.IsJiraStory) story.JiraIssueId = data.JiraIssueId;

            room.Stories.Add(story);

            await _unitOfWork.SaveChangesAsync();

            return StatusCode(201, new { id = story.Id });
        }

        [HttpDelete, Route("delete/{id}"), Authorize(Policy = "OfficialUsers")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (id == 0)
            {
                return StatusCode(402);
            }

            var story = await _unitOfWork.StoryRepository.GetByIdAsync(id);

            if (story == null)
            {
                return StatusCode(404);
            }

            story.SubmittedPointByUsers.Clear();
            _unitOfWork.StoryRepository.Delete(story);

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { storyId = id });
        }

        [Authorize(Policy = "AllUsers")]
        [HttpPost, Route("submitpoint")]
        public async Task<IActionResult> SubmitPoint([FromBody] SubmitPointDTO data)
        {
            var story = await _unitOfWork.StoryRepository.GetByIdAsync(data.StoryId);
            if (story == null)
                return NotFound();

            var userId = Int32.Parse(HttpContext.User.FindFirst("UserId").Value);
            var userRoom = await _unitOfWork.UserRoomRepository.GetByUserIdAndRoomIdAsync(userId, story.RoomId);

            if (userRoom == null)
                return Forbid();

            if (data.IsFinalPoint)
            {
                if (userRoom.Role != Role.host)
                    return Forbid();

                story.Point = data.Point;
            }
            else
            {
                var submittedPoint = story.SubmittedPointByUsers.FirstOrDefault(i => i.UserId == userId);
                if (submittedPoint != null)
                {
                    submittedPoint.Point = data.Point;
                }
                else
                {
                    story.SubmittedPointByUsers.Add(new Models.SubmittedPointByUser
                    {
                        Point = data.Point,
                        User = userRoom.User
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return StatusCode(201, new { storyId = data.StoryId });
        }

        [Authorize(Policy = "OfficialUsers")]
        [HttpPost, Route("assign")]
        public async Task<IActionResult> Assign([FromBody] AssignStoryDTO data)
        {
            var story = await _unitOfWork.StoryRepository.GetByIdAsync(data.StoryId);
            if (story == null)
                return NotFound();

            var userId = Int32.Parse(HttpContext.User.FindFirst("UserId").Value);
            var userRoom = await _unitOfWork.UserRoomRepository.GetByUserIdAndRoomIdAsync(userId, story.RoomId);

            if (userRoom == null)
                return Forbid();
            else if (userRoom.Role != Role.host)
                return Forbid();

            var assignee = await _unitOfWork.UserRepository.GetByIdAsync(data.UserId);
            story.Assignee = assignee;

            await _unitOfWork.SaveChangesAsync();

            return StatusCode(201, new { storyId = data.StoryId });
        }
    }
}
