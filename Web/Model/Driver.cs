using Web.Types;

namespace Web.Model;

public class Driver(Snowflake driverId)
{
    public Snowflake DriverId { get; set; } = driverId;
    public string Name { get; set; } = string.Empty;
}
