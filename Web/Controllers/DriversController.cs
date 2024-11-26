using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public record class DriverResponse(long Id, string Name);
public record class UpdateDriverRequest(string Name);

[Route("[controller]")]
public class DriversController([FromKeyedServices(CollectionNames.Drivers)] List<string> drivers) : ControllerBase
{
    readonly List<string> drivers = drivers;

    [HttpGet]
    public IActionResult ListAllDrivers() => Ok(drivers.Select((name, idx) => new DriverResponse(idx + 1, name)).ToList());

    [HttpGet("{id}")]
    public IActionResult FindDriverById(int id) => drivers.Count < id ? NotFound() : Ok(new DriverResponse(id, drivers[id - 1]));

    [HttpPost]
    public IActionResult AddDriver([FromBody] UpdateDriverRequest updateDriverRequest)
    {
        lock (drivers)
        {
            drivers.Add(updateDriverRequest.Name);
            return CreatedAtAction(nameof(FindDriverById), new { id = drivers.Count }, new DriverResponse(drivers.Count, drivers.Last()));
        }
    }
}
