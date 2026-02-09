using Microsoft.EntityFrameworkCore;
using Forest.Infrastructure.Persistence.Entities;

namespace Forest.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<NodeEntity> Nodes => Set<NodeEntity>();
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<NodeEntity>(entity =>
        {
            entity.ToTable("Nodes");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(x => x.ParentId);
            entity.HasIndex(x => x.Name);

            entity.HasOne(x => x.Parent)
                .WithMany()
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid deleting subtrees (for now)
        });
    }
}
