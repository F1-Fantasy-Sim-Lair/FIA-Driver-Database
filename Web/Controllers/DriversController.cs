using Microsoft.AspNetCore.Mvc;
using Web.Model;
using Web.Model.Repository;
using Web.Types;
using Web.Utils;

namespace Web.Controllers;

public record class DriverResponse(Snowflake Id, string Name);
public record class UpdateDriverRequest(string Name);

[Route("[controller]")]
public class DriversController(IUnitOfWork unitOfWork) : ControllerBase
{
    readonly IUnitOfWork unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> ListAllDrivers() => Ok(await unitOfWork.Repository<Driver>().Query().Select(d => new DriverResponse(d.DriverId, d.Name)).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> FindDriverById([FromRoute] Snowflake id)
    {
        var driver = await unitOfWork.Repository<Driver>().GetByIdAsync(id);
        if (driver == null)
            return NotFound();

        return Ok(new DriverResponse(driver.DriverId, driver.Name));
    }

    [HttpPost]
    public async Task<IActionResult> AddDriver([FromBody] UpdateDriverRequest updateDriverRequest, [FromServices] GenerateId generateId)
    {
        var driver = new Driver(generateId())
        {
            Name = updateDriverRequest.Name,
        };

        unitOfWork.Repository<Driver>().Add(driver);
        await unitOfWork.CompleteAsync();
        return CreatedAtAction(nameof(FindDriverById), new { id = driver.DriverId }, new DriverResponse(driver.DriverId, driver.Name));
    }
}
