using System.Net.Http;
using System.Threading.Tasks;

namespace scrum_poker_server.Services
{
    public interface IJiraService
    {
        public Task<bool> IsJiraTokenValidAsync(string jiraDomain, string jiraToken);
    }

    public class JiraService : IJiraService
    {
        private readonly IHttpClientFactory _clientFactory;

        public JiraService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<bool> IsJiraTokenValidAsync(string jiraDomain, string jiraToken)
        {
            var client = _clientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{jiraDomain}/rest/api/3/myself");
            request.Headers.Add("Authorization", $"Basic {jiraToken}");

            var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}
