using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Model;
using Web.Model.Repository;
using Web.Types;
using Web.Utils;

namespace Web.Controllers;

public record class TeamResponse(Snowflake Id, string Name);

[Route("teams")]
public class TeamsController(IUnitOfWork unitOfWork) : ControllerBase
{
    private readonly IUnitOfWork unitOfWork = unitOfWork;

    [HttpGet, Authorize]
    public async Task<IActionResult> GetAllTeams()
    {
        return Ok(await unitOfWork.Repository<Team>().Query().Select(t => new TeamResponse(t.TeamId, t.Name)).ToListAsync());
    }
}
