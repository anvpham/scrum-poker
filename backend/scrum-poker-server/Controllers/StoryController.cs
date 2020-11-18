﻿using Microsoft.AspNetCore.Mvc;
using scrum_poker_server.Data;
using scrum_poker_server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using scrum_poker_server.DTOs;

namespace scrum_poker_server.Controllers
{
    [Route("api/stories")]
    public class StoryController : ControllerBase
    {
        public AppDbContext _dbContext { get; set; }

        public StoryController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet, Route("get/{id}")]
        public IActionResult Get(int id)
        {
            if (ModelState.IsValid)
            {
                var story = _dbContext.Stories.FirstOrDefault(s => s.Id == id);
                if (story == null)
                {
                    return StatusCode(404, new { error = "The story is not existed" });
                }
                return Ok(new { id, title = story.Title, content = story.Content });
            }
            else return StatusCode(422);
        }

        [Consumes("application/json")]
        [HttpPost, Route("add")]
        public async Task<IActionResult> Add([FromBody] AddStoryDTO data)
        {
            if (ModelState.IsValid)
            {
                var room = _dbContext.Rooms.FirstOrDefault(r => r.Code == data.RoomCode);
                if (room == null)
                {
                    return StatusCode(422, new { error = "The room code is not existed" });
                }

                var story = new Story { Title = data.Title, Content = data.Content };

                if (room.Stories == null)
                {
                    room.Stories = new List<Story> { story };
                }
                else
                {
                    room.Stories.Add(story);
                }

                await _dbContext.SaveChangesAsync();

                return StatusCode(201, new { id = story.Id });
            }
            else return StatusCode(422);
        }

        [Consumes("application/json")]
        [HttpPost, Route("submit")]
        public async Task<IActionResult> SubmitPoint([FromBody] SubmitStoryDTO data)
        {
            if (ModelState.IsValid)
            {
                var story = _dbContext.Stories.FirstOrDefault(s => s.Room.Code == data.RoomCode && s.Id == data.StoryId);
                var user = _dbContext.UserRooms.FirstOrDefault(ur => ur.Room.Code == data.RoomCode && ur.User.Name == data.UserName).User;

                story.SubmittedPointByUsers.Add(new SubmittedPointByUser
                {
                    Point = data.Point,
                    User = user
                });

                await _dbContext.SaveChangesAsync();
                return StatusCode(201);
            }
            else return StatusCode(422);
        }

        [Consumes("application/json")]
        [HttpPost, Route("assign")]
        public async Task<IActionResult> Assign([FromBody] AssignStoryDTO data)
        {
            if (ModelState.IsValid)
            {
                var story = _dbContext.Stories.FirstOrDefault(s => s.Room.Code == data.RoomCode && s.Id == data.StoryId);
                var user = _dbContext.UserRooms.FirstOrDefault(ur => ur.Room.Code == data.RoomCode && ur.User.Name == data.UserName).User;
                story.Assignee = user;
                await _dbContext.SaveChangesAsync();
                return StatusCode(201, new { storyId = data.StoryId, data.UserName });
            }
            else return StatusCode(422);
        }
    }
}