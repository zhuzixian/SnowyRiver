using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.Domain.Entities;
using SnowyRiver.Domain.Shared.Entities;
using System;

namespace SnowyRiver.Accounts.EntityFramework;

public class AccountsDbContext<TUser, TRole, TTeam, TPermission,
    TUserHistory, TRoleHistory, TTeamHistory, TPermissionHistory>(DbContextOptions options) 
    : DbContext(options)
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
    where TUserHistory: UserHistoryBase<TUser, TRole, TTeam, TPermission>
    where TRoleHistory: RoleHistoryBase<TUser, TRole, TTeam, TPermission>
    where TTeamHistory: TeamHistoryBase<TUser, TRole, TTeam, TPermission>
    where TPermissionHistory: PermissionHistoryBase<TUser, TRole, TTeam, TPermission>
{
    protected virtual string DbTablePrefix => "Accounts_";
    protected virtual string? DbSchema => null;

    public DbSet<TUser> Users { get; set; }
    public DbSet<TRole> Roles { get; set; }
    public DbSet<TTeam> Teams { get; set; }
    public DbSet<TPermission> Permissions { get; set; }

    public DbSet<TUserHistory> UserHistories { get; set; }
    public DbSet<TRoleHistory> RoleHistories { get; set; }
    public DbSet<TTeamHistory> TeamHistories { get; set; }
    public DbSet<TPermissionHistory> PermissionHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TPermission>(b =>
        {
            b.ToTable(DbTablePrefix + "Permissions", DbSchema);
            b.HasKey(x => x.Id);
        });

        modelBuilder.Entity<TUser>(b =>
        {
            b.ToTable(DbTablePrefix + "Users", DbSchema);
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.Property(x => x.Password).IsRequired().HasMaxLength(64);
            b.Property(x => x.PasswordSalt).IsRequired().HasMaxLength(64);
            b.HasMany(e => e.Roles).WithMany(e => e.Users)
                .UsingEntity(DbTablePrefix + "UserRoles");
        });


        modelBuilder.Entity<TRole>(b =>
        {
            b.ToTable(DbTablePrefix + "Roles", DbSchema);
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.HasMany(e => e.Permissions).WithMany(e => e.Roles)
                .UsingEntity(DbTablePrefix + "RolePermissions");
        });

        modelBuilder.Entity<TTeam>(b =>
        {
            b.ToTable(DbTablePrefix + "Teams", DbSchema);
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.HasMany(e => e.Users).WithMany(e => e.Teams)
                .UsingEntity(DbTablePrefix + "UserTeams");
        });

        ConfigEntityHistory<TUser, TUserHistory>(modelBuilder, "UserHistories");
        ConfigEntityHistory<TRole, TRoleHistory>(modelBuilder, "RoleHistories");
        ConfigEntityHistory<TTeam, TTeamHistory>(modelBuilder, "TeamHistories");
        ConfigEntityHistory<TPermission, TPermissionHistory>(modelBuilder, "PermissionHistory");
    }

    protected virtual void ConfigEntityHistory<TEntity, TEntityHistory>(
        ModelBuilder modelBuilder, string tableName)
        where TEntityHistory : AccountEntityHistory<TEntity, Guid, TUser, TRole, TTeam, TPermission>
        where TEntity : IEntity<Guid>
    {
        modelBuilder.Entity<TEntityHistory>(b =>
        {
            b.ToTable(DbTablePrefix + tableName, DbSchema);
            b.HasKey(x => x.Id);
            b.Property(e => e.SnapShot).HasColumnType("json");
        });
    }
}
