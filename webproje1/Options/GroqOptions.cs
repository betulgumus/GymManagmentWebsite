namespace webproje1.Options
{
    public class GroqOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "llama-3.1-70b-versatile";
        public string BaseUrl { get; set; }
            = "https://api.groq.com/openai/v1/chat/completions";
    }
}
