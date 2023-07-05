namespace scrum_poker_server.DTOs
{
    public class FetchJiraStories
    {
        public string Query { get; set; }

        public string JiraToken { get; set; }

        public string JiraDomain { get; set; }
    }
}
