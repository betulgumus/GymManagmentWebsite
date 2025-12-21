using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using webproje1.Options;

namespace webproje1.Services
{
    public class GroqChatService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqOptions _options;

        public GroqChatService(
            HttpClient httpClient,
            IOptions<GroqOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new Exception("Groq API KEY BOŞ GELİYOR!");
        }

        public async Task<string> AskAsync(string prompt)
        {
            var requestBody = new
            {
                model = _options.Model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                _options.BaseUrl);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer", _options.ApiKey);

            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(json);

            using var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;
        }

        internal async Task<string> GetRecommendationAsync(string prompt, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
