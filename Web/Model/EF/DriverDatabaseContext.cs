using Microsoft.EntityFrameworkCore;

namespace Web.Model.EF;

public class DriverDatabaseContext(DbContextOptions options) : DbContext(options)
{
}
