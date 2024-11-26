using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;
using Web.Model;
using Web.UnitTests.Fakes;

namespace Web.UnitTests.Controllers;
internal class DriversControllerTests
{
    [Test]
    public async Task ListAllDrivers_ReturnsAllDriversInList()
    {
        var uow = new FakeUnitOfWork();
        uow.Repository<Driver>().AddRange([new(1) { Name = "Driver 1" }, new(2) { Name = "Driver 2" }]);
        var sut = new DriversController(uow);
        (await sut.ListAllDrivers()).Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeAssignableTo<IEnumerable<DriverResponse>>()
            .Which.Should().HaveCount(2);
    }

    [Test]
    public async Task FindDriverById_ReturnsDriverFromList()
    {
        var uow = new FakeUnitOfWork();
        uow.Repository<Driver>().AddRange([new(1) { Name = "Driver 1" }]);
        var sut = new DriversController(uow);
        (await sut.FindDriverById(1)).Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<DriverResponse>()
            .Which.Should().BeEquivalentTo(new DriverResponse(1, "Driver 1"));
    }

    [Test]
    public async Task FindDriverById_ReturnsNotFoundWhenIdIsOutOfRange()
    {
        var uow = new FakeUnitOfWork();
        uow.Repository<Driver>().AddRange([new(1) { Name = "Driver 1" }]);
        var sut = new DriversController(uow);
        (await sut.FindDriverById(2)).Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task AddDriver_AddsDriverToList()
    {
        var uow = new FakeUnitOfWork();
        var sut = new DriversController(uow);
        var result = (await sut.AddDriver(new("Driver 1"), () => 1)).Should().BeOfType<CreatedAtActionResult>().Subject;
        result.RouteValues.Should().HaveCount(1);
        result.Value.Should().BeEquivalentTo(new DriverResponse(1, "Driver 1"));
        uow.Repository<Driver>().Data.Should().HaveCount(1);
    }
}
