using Microsoft.EntityFrameworkCore;
using scrum_poker_server.Models;
using System.Threading.Tasks;

namespace scrum_poker_server.Data.Repositories
{
    public interface IStoryRepository
    {
        public void Delete(Story story);
        public Task<Story?> GetByIdAsync(int id);
        public Task<Story?> GetByRoomIdAndJiraIssueIdAsync(int roomId, string jiraIssueId);
    }

    public class StoryRepository : IStoryRepository
    {
        private readonly AppDbContext _dbContext;

        public StoryRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Delete(Story story)
        {
            _dbContext.Stories.Remove(story);
        }

        public async Task<Story?> GetByIdAsync(int id)
        {
            return await _dbContext.Stories
                .Include(s => s.SubmittedPointByUsers)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Story?> GetByRoomIdAndJiraIssueIdAsync(int roomId, string jiraIssueId)
        {
            return await _dbContext.Stories
                .FirstOrDefaultAsync(s => s.JiraIssueId == jiraIssueId && s.RoomId == roomId);
        }
    }
}
