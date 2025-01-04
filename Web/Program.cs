using IdGen;
using IdGen.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Web.Authorization;
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
        builder.Services.AddDbContext<DriverDatabaseContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DriverDatabaseContext") ?? throw new InvalidOperationException("Connection string 'DriverDatabaseContext' not found."))
                .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
                .EnableDetailedErrors(builder.Environment.IsDevelopment()));

        builder.Services.AddIdGen(1);
        builder.Services.AddSingleton<GenerateId>(sp => () => new(sp.GetRequiredService<IIdGenerator<long>>().CreateId()));
        builder.Services.AddDriverDatabaseIdentity(builder.Configuration);
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new SnowflakeJsonConverter());
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.MapType<Snowflake>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" }); // JavaScript can't properly handle 64-bit integers, so convert snowflake IDs to string
        });

        var app = builder.Build();

        if (builder.Configuration.GetValue("Database:MigrateOnStartup", false))
        {
            MigrateDatabase<DriverDatabaseContext>(app.Services);
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    // See https://gist.github.com/Tim-Hodge/eea0601a14177c199fe60557eeeff31e
    // and https://medium.com/@floyd.may/ef-core-app-migrate-on-startup-d046afdba258
    static void MigrateDatabase<TContext>(IServiceProvider appServices) where TContext : DbContext
    {
        using var scope = appServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        var dbServices = dbContext.GetInfrastructure();

        var modelDiffer = dbServices.GetRequiredService<IMigrationsModelDiffer>();
        var migrationsAssembly = dbServices.GetRequiredService<IMigrationsAssembly>();

        var modelInitializer = dbServices.GetRequiredService<IModelRuntimeInitializer>();
        var sourceModel = modelInitializer.Initialize(migrationsAssembly.ModelSnapshot!.Model);

        var designTimeModel = dbServices.GetRequiredService<IDesignTimeModel>();
        var readOptimizedModel = designTimeModel.Model;

        var diffsExist = modelDiffer.HasDifferences(
            sourceModel.GetRelationalModel(),
            readOptimizedModel.GetRelationalModel());

        if (diffsExist)
        {
            throw new InvalidOperationException("There are differences between the current database model and the most recent migration.");
        }

        dbContext.Database.Migrate();
    }
}
