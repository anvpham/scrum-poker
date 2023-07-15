using Azure.Core;
using scrum_poker_server.DTOs;
using scrum_poker_server.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace scrum_poker_server.Services
{
    public interface IJiraService
    {
        public Task<HttpResponseMessage> UpdateIssueAsync(User user, string issueId, JiraSubmitPointRequest payload);
        public Task<JiraIssueResponse> GetIssueAsync(User user, string issueId);
        public Task<HttpResponseMessage> GetStoriesAsync(User user, string jqlQuery);
        public Task<bool> IsUserJiraTokenValidAsync(User user);
        public Task<bool> IsUserJiraTokenValidAsync(string jiraDomain, string jiraEmail, string jiraApiToken);
        public Task<bool> IsJiraDomainValidAsync(string jiraDomain);
    }

    public class JiraService : IJiraService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public JiraService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient();
        }

        private static string GenerateBasicAuthToken(User user)
        {
            if (user == null
                || string.IsNullOrEmpty(user.JiraDomain)
                || string.IsNullOrEmpty(user.JiraEmail)
                || string.IsNullOrEmpty(user.JiraToken))
                throw new ApplicationException("Missing user data");

            var base64Token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user.JiraEmail}:{user.JiraToken}"));
            return base64Token;
        }

        public async Task<JiraIssueResponse> GetIssueAsync(User user, string issueId)
        {
            var basicAuthToken = GenerateBasicAuthToken(user);
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://{user.JiraDomain}/rest/api/3/issue/{issueId}?fields=description,summary,customfield_10026&expand=renderedFields");
            request.Headers.Add("Authorization", $"Basic {basicAuthToken}");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Get issueId {issueId} failed for JiraEmail {user.JiraEmail}");

            var content = await response.Content.ReadAsStringAsync();
            JiraIssueResponse issue = JsonSerializer.Deserialize<JiraIssueResponse>(content, _serializerOptions);

            return issue;
        }

        public async Task<HttpResponseMessage> GetStoriesAsync(User user, string jqlQuery)
        {
            var basicAuthToken = GenerateBasicAuthToken(user);
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{user.JiraDomain}/rest/api/3/issue/picker?query={jqlQuery}&currentJQL");
            request.Headers.Add("Authorization", $"Basic {basicAuthToken}");

            var response = await _httpClient.SendAsync(request);
            return response;
        }

        public async Task<bool> IsJiraDomainValidAsync(string jiraDomain)
        {
            var response = await _httpClient.GetAsync($"https://{jiraDomain}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> IsUserJiraTokenValidAsync(User user)
        {
            var basicAuthToken = GenerateBasicAuthToken(user);
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{user.JiraDomain}/rest/api/3/myself");
            request.Headers.Add("Authorization", $"Basic {basicAuthToken}");

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> IsUserJiraTokenValidAsync(string jiraDomain, string jiraEmail, string jiraApiToken)
        {
            var basicAuthToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{jiraEmail}:{jiraApiToken}"));
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{jiraDomain}/rest/api/3/myself");
            request.Headers.Add("Authorization", $"Basic {basicAuthToken}");

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<HttpResponseMessage> UpdateIssueAsync(User user, string issueId, JiraSubmitPointRequest payload)
        {
            var basicAuthToken = GenerateBasicAuthToken(user);
            var request = new HttpRequestMessage(HttpMethod.Put, $"https://{user.JiraDomain}/rest/api/3/issue/{issueId}");
            var jsonData = JsonSerializer.Serialize(payload);
            var requestBody = new StringContent(jsonData, Encoding.UTF8, "application/json");
            request.Content = requestBody;

            var response = await _httpClient.SendAsync(request);

            return response;
        }
    }
}
