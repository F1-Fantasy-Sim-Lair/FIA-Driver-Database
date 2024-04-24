using FluentAssertions;
using Web.Controllers;

namespace Web.UnitTests.Controllers;
internal class WeatherForecastControllerTests
{
    [Test]
    public void Get_ReturnsNext5DaysOfForecast()
    {
        var sut = new WeatherForecastController();
        var forecasts = sut.Get();
        var today = DateOnly.FromDateTime(DateTime.Today);
        forecasts.Should().HaveCount(5)
            .And.AllSatisfy(fc => fc.Date.Should().BeAfter(today));
    }
}
