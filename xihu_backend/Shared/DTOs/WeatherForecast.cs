namespace Shared.DTOs;

public class WeatherDTO
{
    public DateTime Date { get; set; }
    public int Temperature { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public WeatherDetails Details { get; set; } = new();
}

public class WeatherDetails
{
    public int Humidity { get; set; }
    public int WindSpeed { get; set; }
    public string WindDirection { get; set; } = string.Empty;
} 