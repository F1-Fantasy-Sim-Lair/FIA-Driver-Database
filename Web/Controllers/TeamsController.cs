using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("teams")]
public class TeamsController : ControllerBase
{
    [HttpGet, Authorize]
    public async Task<IActionResult> GetAllTeams()
    {
        return Ok();
    }
}
