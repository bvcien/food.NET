
using System.Reflection;
using NETCORE.Data.Entities;
using NETCORE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace NETCORE.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        IEnumerable<EntityEntry> modified = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
        foreach (EntityEntry item in modified)
        {
            if (item.Entity is IDateTracking changedOrAddedItem)
            {
                if (item.State == EntityState.Added)
                {
                    changedOrAddedItem.CreateDate = DateTime.Now;
                }
                else
                {
                    changedOrAddedItem.LastModifiedDate = DateTime.Now;
                }
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Role>().ToTable("Roles").Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
        builder.Entity<User>().ToTable("Users").Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

        builder.Entity<Permission>()
                .HasKey(c => new { c.RoleId, c.FunctionId, c.CommandId });
        builder.Entity<Hoaqua>().ToTable("Hoaqua");
        builder.Entity<CartItem>().ToTable("CartItem");
    }
    public DbSet<Command> Commands { set; get; } = default!;
    public DbSet<ActivityLog> ActivityLogs { set; get; } = default!;
    public DbSet<Function> Functions { set; get; } = default!;
    public DbSet<Permission> Permissions { set; get; } = default!;
    public DbSet<UserSetting> UserSettings { set; get; } = default!;
    public DbSet<Hoaqua> Hoaqua { get; set; } = default!;
    public DbSet<CartItem> CartItem { get; set; } = default!;
}
