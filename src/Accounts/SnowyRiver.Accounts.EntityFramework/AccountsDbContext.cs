using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.Domain.Entities;

namespace SnowyRiver.Accounts.EntityFramework;

public class AccountsDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Permission> Permissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        const string dbTablePrefix = "Accounts_";
        const string? dbSchema = null;


        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Permission>(b =>
        {
            b.ToTable(dbTablePrefix + "Permissions", dbSchema);
            b.HasKey(x => x.Id);
        });

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable(dbTablePrefix + "Users", dbSchema);
            b.HasKey(x => x.Id);
            b.HasAlternateKey(x => x.UserId);
            b.Property(x => x.UserId).ValueGeneratedOnAdd();
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.Property(x => x.Password).IsRequired().HasMaxLength(64);
            b.Property(x => x.PasswordSalt).IsRequired().HasMaxLength(64);
            b.HasMany(e => e.Roles).WithMany(e => e.Users)
                .UsingEntity(dbTablePrefix + "UserRoles");
        });

        modelBuilder.Entity<Role>(b =>
        {
            b.ToTable(dbTablePrefix + "Roles", dbSchema);
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.HasMany(e => e.Permissions).WithMany(e => e.Roles)
                .UsingEntity(dbTablePrefix + "RolePermissions");
        });

        modelBuilder.Entity<Team>(b =>
        {
            b.ToTable(dbTablePrefix + "Teams", dbSchema);
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.HasMany(e => e.Users).WithMany(e => e.Teams)
                .UsingEntity(dbTablePrefix + "UserTeams");
        });
    }
}
