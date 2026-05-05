using Chronoscope.Application;
using Chronoscope.Data;
using Chronoscope.Infrastructure;
using Chronoscope.Web;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddApplication();
builder.Services.AddData(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWeb();

var app = builder.Build();
var runMigrateAndExit = args.Any(argument =>
    string.Equals(argument, "--migrate-and-exit", StringComparison.OrdinalIgnoreCase));

var isEfDesignTime = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_EF_DESIGN_TIME"),
    "true",
    StringComparison.OrdinalIgnoreCase);

if (isEfDesignTime)
{
    return;
}

if (runMigrateAndExit)
{
    await ApplyMigrationsAsync(app.Services);
    return;
}

await VerifyDatabaseConnectivityAsync(app.Services);
await VerifyNoPendingMigrationsAsync(app.Services);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultControllerRoute();
app.Run();

return;

static async Task VerifyDatabaseConnectivityAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ChronoscopeDbContext>>();
    await using var dbContext = await dbContextFactory.CreateDbContextAsync();
    var canConnect = await dbContext.Database.CanConnectAsync();
    if (!canConnect)
    {
        throw new InvalidOperationException("Unable to connect to the Chronoscope database.");
    }
}

static async Task VerifyNoPendingMigrationsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ChronoscopeDbContext>>();
    await using var dbContext = await dbContextFactory.CreateDbContextAsync();
    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
    var pendingMigrationList = pendingMigrations.ToArray();

    if (pendingMigrationList.Length > 0)
    {
        throw new InvalidOperationException(
            $"Database schema is behind the application model. Pending migrations: {string.Join(", ", pendingMigrationList)}. " +
            "Run the app in migration mode (--migrate-and-exit) before normal startup.");
    }
}

static async Task ApplyMigrationsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ChronoscopeDbContext>>();
    await using var dbContext = await dbContextFactory.CreateDbContextAsync();
    await dbContext.Database.MigrateAsync();
}
