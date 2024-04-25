using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Web.Model;
using Web.Model.EF;

namespace Web.IntegrationTests.Model;
internal class DriverDatabaseContextTests
{
    SqliteConnection conn;
    DbContextOptions options;

    [SetUp]
    public void Setup()
    {
        conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        options = new DbContextOptionsBuilder<DriverDatabaseContext>()
            .UseSqlite(conn)
            .Options;
    }

    [TearDown]
    public void Teardown()
    {
        conn.Close();
        conn.Dispose();
    }

    [Test]
    public async Task DriverDatabaseContext_CanStoreDriver()
    {
        using var sut = new DriverDatabaseContext(options);
        sut.Database.EnsureCreated();
        sut.Add(new Driver(new(1233076034211942421)));
        await sut.SaveChangesAsync();
    }
}
