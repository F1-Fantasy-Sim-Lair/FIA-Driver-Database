using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Authorization;
using Web.Model;
using Web.Model.Repository;
using Web.Types;
using Web.Utils;

namespace Web.Controllers;

public record class TeamResponse(Snowflake Id, string Name)
{
    public TeamResponse(Team team) : this(team.TeamId, team.Name) { }
}
public record class CreateTeamRequest(string Name);

[Route("teams")]
public class TeamsController(IUnitOfWork unitOfWork) : ControllerBase
{
    private readonly IUnitOfWork unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> GetAllTeams()
    {
        return Ok(await unitOfWork.Repository<Team>().Query().Select(t => new TeamResponse(t)).ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeam(Snowflake id)
    {
        var team = await unitOfWork.Repository<Team>().GetByIdAsync(id);
        if (team is null)
            return NotFound();

        return Ok(new TeamResponse(team.TeamId, team.Name));
    }

    [HttpPost, Authorize(Policies.IsDirector)]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest createTeamRequest, [FromServices] GenerateId generateId)
    {
        var team = new Team(generateId())
        {
            Name = createTeamRequest.Name
        };
        unitOfWork.Repository<Team>().Add(team);
        await unitOfWork.CompleteAsync();
        return CreatedAtAction(nameof(GetTeam), new { id = team.TeamId }, new TeamResponse(team));
    }

    [HttpPut("{id}"), Authorize(Policies.IsDirector)]
    public async Task<IActionResult> UpdateTeam(Snowflake id, [FromBody] CreateTeamRequest createTeamRequest)
    {
        var team = await unitOfWork.Repository<Team>().GetByIdAsync(id);
        if (team is null)
            return NotFound();

        team.Name = createTeamRequest.Name;
        await unitOfWork.CompleteAsync();
        return Ok(new TeamResponse(team));
    }
}
