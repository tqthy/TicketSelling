using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Secret;

public class Doppler
{
    [JsonPropertyName("DOPPLER_PROJECT")]
    public string DopplerProject { get; set; }

    [JsonPropertyName("DOPPLER_ENVIRONMENT")]
    public string DopplerEnvironment { get; set; }

    [JsonPropertyName("DOPPLER_CONFIG")]
    public string DopplerConfig { get; set; }

    private static HttpClient client = new HttpClient();

    public static async Task<Doppler> FetchSecretsAsync()
    {   
        var dopplerToken = Environment.GetEnvironmentVariable("DOPPLER_TOKEN");

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", dopplerToken);
        var streamTask = client.GetStreamAsync("https://api.doppler.com/v3/configs/config/secrets/download?format=json");
        var secrets = await JsonSerializer.DeserializeAsync<Doppler>(await streamTask);

        return secrets;
    }
}