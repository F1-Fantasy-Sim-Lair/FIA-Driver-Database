using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;
using Web.Model;
using Web.UnitTests.Fakes;

namespace Web.UnitTests.Controllers;
internal class TeamsControllerTests
{
    [Test]
    public async Task GetAllTeams_ReturnsAllTeams()
    {
        // Arrange
        var uow = new FakeUnitOfWork();
        uow.Repository<Team>().AddRange([
            new Team(1) { Name = "Team 1" },
            new Team(2) { Name = "Team 1" },
            new Team(3) { Name = "Team 1" }]);
        await uow.CompleteAsync();

        var controller = new TeamsController(uow);
        // Act
        var result = await controller.GetAllTeams();
        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().NotBeNull()
            .And.BeAssignableTo<IEnumerable<TeamResponse>>()
            .Which.Should().HaveCount(3);
    }
}
