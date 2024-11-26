using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;

namespace Web.UnitTests.Controllers;
internal class DriversControllerTests
{
    [Test]
    public void ListAllDrivers_ReturnsAllDriversInList()
    {
        var sut = new DriversController(["Driver 1", "Driver 2"]);
        sut.ListAllDrivers().Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeAssignableTo<IEnumerable<DriverResponse>>()
            .Which.Should().HaveCount(2);
    }

    [Test]
    public void FindDriverById_ReturnsDriverFromList()
    {
        var sut = new DriversController(["Driver 1"]);
        sut.FindDriverById(1).Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<DriverResponse>()
            .Which.Should().BeEquivalentTo(new DriverResponse(1, "Driver 1"));
    }

    [Test]
    public void FindDriverById_ReturnsNotFoundWhenIdIsOutOfRange()
    {
        var sut = new DriversController(["Driver 1"]);
        sut.FindDriverById(2).Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public void AddDriver_AddsDriverToList()
    {
        List<string> driverNames = [];
        var sut = new DriversController(driverNames);
        var result = sut.AddDriver(new("Driver 1")).Should().BeOfType<CreatedAtActionResult>().Subject;
        result.RouteValues.Should().HaveCount(1);
        result.Value.Should().BeEquivalentTo(new DriverResponse(1, "Driver 1"));
        driverNames.Should().HaveCount(1);
    }
}
