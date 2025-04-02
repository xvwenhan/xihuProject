using System.Net.Http;
using System.Net.Http.Json;
using Shared.DTOs;

namespace VideoService.Services;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public WeatherService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }
 
    // 获取指定城市的天气信息
    public async Task<WeatherDTO?> GetWeatherAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            return null;  

        string chatServiceUrl = _configuration["ServiceUrls:ChatService"];
        var response = await _httpClient.GetAsync($"{chatServiceUrl}/api/weather/{city}");

        if (!response.IsSuccessStatusCode)
            return null; 

        return await response.Content.ReadFromJsonAsync<WeatherDTO>();
    }

    // 判断是否适合看电影
    // ?为可空类型 
    public async Task<(bool ShouldWatch, string? ErrorMessage)> IsShouldWatchAsync(string city)
    {
        var weather = await GetWeatherAsync(city);

        if (weather == null)
            return (false, $"未找到 {city} 的天气信息");

        bool shouldWatch = weather.Description == "晴天";

        return (shouldWatch, null);
    }
}
