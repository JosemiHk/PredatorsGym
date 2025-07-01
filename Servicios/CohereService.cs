using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace PredatorsGym.Servicios
{
    public class CohereService : ICohereService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public CohereService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task<string> GenerarTextoAsync(string prompt)
        {
            var apiKey = _config["Cohere:ApiKey"];

            var requestBody = new
            {
                model = "command", // ✅ modelo permitido con /v1/generate
                prompt = prompt,
                max_tokens = 2048,
                temperature = 0.7,
                stop_sequences = new string[] { }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders.Add("Cohere-Version", "2022-12-06");

            var response = await _httpClient.PostAsync("https://api.cohere.ai/v1/generate", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"[Cohere API Error {response.StatusCode}]: {responseJson}");
            }

            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("generations")[0].GetProperty("text").GetString();
        }
    }
}