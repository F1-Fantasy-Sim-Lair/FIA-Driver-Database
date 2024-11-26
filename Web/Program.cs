using IdGen;
using IdGen.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Web.Model.EF;
using Web.Model.Repository;
using Web.Types;

namespace Web;

public delegate Snowflake GenerateId();

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddScoped<IUnitOfWork, DefaultUnitOfWork>();
        builder.Services.AddDbContext<DriverDatabaseContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DriverDatabaseContext") ?? throw new InvalidOperationException("Connection string 'DriverDatabaseContext' not found.")));
        builder.Services.AddIdGen(1);
        builder.Services.AddSingleton<GenerateId>(sp => () => new(sp.GetRequiredService<IIdGenerator<long>>().CreateId()));

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
