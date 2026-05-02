using Chronoscope.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chronoscope.Data;

public sealed class ChronoscopeDbContext(DbContextOptions<ChronoscopeDbContext> options) : DbContext(options)
{
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<Source> Sources => Set<Source>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChronoscopeDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
