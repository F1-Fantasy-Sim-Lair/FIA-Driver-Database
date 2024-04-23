namespace Web;

public class WeatherForecast
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)Math.Round(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}
