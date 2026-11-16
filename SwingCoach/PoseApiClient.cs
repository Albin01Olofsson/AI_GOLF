using System.Text.Json;

namespace SwingCoach
{
    public class PoseApiClient
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "http://127.0.0.1:8000/analyze-swing";

        public PoseApiClient(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(2);
        }

        public async Task<List<SwingFrameData>> AnalyzeVideoAsync(string videoFilePath)
        {
            if (!File.Exists(videoFilePath))
            {
                throw new FileNotFoundException("Videofilen hittades inte.", videoFilePath);
            }

            using var content = new MultipartFormDataContent();
            await using var fileStream = File.OpenRead(videoFilePath);
            using var fileContent = new StreamContent(fileStream);

            content.Add(fileContent, "file", Path.GetFileName(videoFilePath));

            var response = await _httpClient.PostAsync(ApiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<SwingFrameData>>(responseJson);

            return result ?? new List<SwingFrameData>();
        }
    }
}
