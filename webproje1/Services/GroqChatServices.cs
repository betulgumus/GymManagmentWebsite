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
                throw new InvalidOperationException(
                    "Groq API Key bulunamadı. User Secrets kontrol et.");
        }

        public Task<string> AskAsync(string prompt)
        {
            return SendRequestAsync(prompt, CancellationToken.None);
        }

        public Task<string> GetRecommendationAsync(
            string prompt,
            CancellationToken cancellationToken)
        {
            return SendRequestAsync(prompt, cancellationToken);
        }

        private async Task<string> SendRequestAsync(
            string prompt,
            CancellationToken cancellationToken)
        {
            try
            {
                var requestBody = new
                {
                    model = _options.Model,
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "Sen bir fitness danışmanısın. Yanıtlarını Türkçe, kısa ve net ver."
                        },
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    temperature = 0.7,
                    max_tokens = 1000
                };

                var request = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

                var jsonBody = JsonSerializer.Serialize(requestBody);
                Console.WriteLine($"[GROQ] Request Body: {jsonBody}");

                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                Console.WriteLine($"[GROQ] Status: {response.StatusCode}");
                Console.WriteLine($"[GROQ] Response: {responseBody}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Groq API Error {response.StatusCode}: {responseBody}");
                }

                using var doc = JsonDocument.Parse(responseBody);

                if (!doc.RootElement.TryGetProperty("choices", out var choices) ||
                    choices.GetArrayLength() == 0)
                {
                    throw new InvalidOperationException($"Empty response: {responseBody}");
                }

                var content = choices[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return content?.Trim() ?? "Yanıt alınamadı";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GROQ ERROR] {ex.Message}");
                throw;
            }
        }
    }
}