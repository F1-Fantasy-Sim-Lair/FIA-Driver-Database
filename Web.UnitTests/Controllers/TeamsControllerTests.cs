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

    [Test]
    public async Task GetTeam_ReturnsTeam()
    {
        // Arrange
        var uow = new FakeUnitOfWork();
        var team = new Team(1) { Name = "Team 1" };
        uow.Repository<Team>().Add(team);
        await uow.CompleteAsync();
        var controller = new TeamsController(uow);

        // Act
        var result = await controller.GetTeam(1);

        // Assert
        var teamResp = result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().NotBeNull()
            .And.BeOfType<TeamResponse>().Subject;
        teamResp.Name.Should().Be("Team 1");
    }

    [Test]
    public async Task CreateTeam_ReturnsCreatedTeam()
    {
        // Arrange
        var uow = new FakeUnitOfWork();
        var controller = new TeamsController(uow);

        // Act
        var result = await controller.CreateTeam(new CreateTeamRequest("Team 1"), () => 1);

        // Assert
        var teamResp = result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().NotBeNull()
            .And.BeOfType<TeamResponse>().Subject;
        teamResp.Name.Should().Be("Team 1");
    }

    [Test]
    public async Task UpdateTeam_ReturnsUpdatedTeam()
    {
        // Arrange
        var uow = new FakeUnitOfWork();
        var team = new Team(1) { Name = "Team 1" };
        uow.Repository<Team>().Add(team);
        await uow.CompleteAsync();
        var controller = new TeamsController(uow);

        // Act
        var result = await controller.UpdateTeam(1, new CreateTeamRequest("Team 2"));

        // Assert
        var teamResp = result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().NotBeNull()
            .And.BeOfType<TeamResponse>().Subject;
        teamResp.Name.Should().Be("Team 2");
    }
}
