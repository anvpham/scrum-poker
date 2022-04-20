namespace scrum_poker_server.DTOs.Incoming
{
    public class FetchJiraStories
    {
        public string Query { get; set; }

        public string JiraToken { get; set; }

        public string JiraDomain { get; set; }
    }
}
