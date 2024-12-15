using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Web.Types;

namespace Web.Model.EF;

public class DriverDatabaseContext(DbContextOptions options) : IdentityDbContext(options)
{
    public DbSet<Driver> Drivers { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Snowflake>().HaveConversion<SnowflakeConverter>();
    }

    public class SnowflakeConverter() : ValueConverter<Snowflake, long>(v => v.Value, v => new(v));
}
