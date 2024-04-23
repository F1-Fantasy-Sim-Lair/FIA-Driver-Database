using FluentAssertions;

namespace Web.UnitTests;

public class Tests
{
    [Test]
    public void WeatherForecast_CanBeCreated()
    {
        var _ = new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Today),
            TemperatureC = 20,
            Summary = "Room Temperature"
        };
    }

    [Test]
    public void WeatherForecast_ConvertsTemperatureToFahrenheit()
    {
        var sut = new WeatherForecast
        {
            TemperatureC = 20,
        };

        sut.TemperatureF.Should().Be(68);
    }
}