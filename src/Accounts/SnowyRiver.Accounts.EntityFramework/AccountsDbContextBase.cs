using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.Domain.Entities;
using SnowyRiver.Domain.Shared.Entities;
using SnowyRiver.EF;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SnowyRiver.Accounts.EntityFramework;

public class AccountsDbContextBase<TUser, TRole, TTeam, TPermission,
    TUserHistory, TRoleHistory, TTeamHistory, TPermissionHistory>(DbContextOptions options) 
    : SnowyRiverDbContext(options)
    where TTeam : Team<TUser, TRole, TTeam, TPermission> 
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission> 
    where TUserHistory: AccountEntityHistory<TUser, TUser, TTeam> 
    where TRoleHistory: AccountEntityHistory<TRole, TUser, TTeam>
    where TTeamHistory: AccountEntityHistory<TTeam, TUser, TTeam>
    where TPermissionHistory: AccountEntityHistory<TPermission, TUser, TTeam>
{
    protected override string DbTablePrefix => "Accounts_";

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

        modelBuilder.Entity<TPermission>(ConfigPermission);
        modelBuilder.Entity<TUser>(ConfigUser);
        modelBuilder.Entity<TRole>(ConfigRole);
        modelBuilder.Entity<TTeam>(ConfigTeam);

        modelBuilder.Entity<TUserHistory>(b =>
        {
            ConfigAccountEntityHistory<TUserHistory, TUser, Guid>(
                b, "UserHistories");
        });

        modelBuilder.Entity<TRoleHistory>(b =>
        {
            ConfigAccountEntityHistory<TRoleHistory, TRole, Guid>(
                b, "RoleHistories");
        });

        modelBuilder.Entity<TTeamHistory>(b =>
        {
            ConfigAccountEntityHistory<TTeamHistory, TTeam, Guid>(
                b, "TeamHistories");
        });

        modelBuilder.Entity<TPermissionHistory>(b =>
        {
            ConfigAccountEntityHistory<TPermissionHistory, TPermission, Guid>(
                b, "PermissionHistories");
        });
    }

    protected virtual void ConfigPermission(EntityTypeBuilder<TPermission> b)
    {
        b.ToTable(DbTablePrefix + "Permissions", DbSchema);
        b.HasKey(x => x.Id);

        ConfigureAccountAudit(b);
    }

    protected virtual void ConfigUser(EntityTypeBuilder<TUser> b)
    {
        b.ToTable(DbTablePrefix + "Users", DbSchema);
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(64);
        b.Property(x => x.Password).IsRequired().HasMaxLength(64);
        b.Property(x => x.PasswordSalt).IsRequired().HasMaxLength(64);
        b.HasMany(e => e.Roles).WithMany(e => e.Users)
            .UsingEntity(DbTablePrefix + "UserRoles");
        ConfigureAccountAudit(b);
    }

    protected virtual void ConfigRole(EntityTypeBuilder<TRole> b)
    {
        b.ToTable(DbTablePrefix + "Roles", DbSchema);
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(64);
        b.HasMany(e => e.Permissions).WithMany(e => e.Roles)
            .UsingEntity(DbTablePrefix + "RolePermissions");
        ConfigureAccountAudit(b);
    }

    protected virtual void ConfigTeam(EntityTypeBuilder<TTeam> b)
    {
        b.ToTable(DbTablePrefix + "Teams", DbSchema);
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(64);
        b.HasMany(e => e.Users).WithMany(e => e.Teams)
            .UsingEntity(DbTablePrefix + "UserTeams");
        ConfigureAccountAudit(b);
    }


    protected static void ConfigureAccountAudit<TEntity>(
        EntityTypeBuilder<TEntity> b)
        where TEntity : class, IAudited<TUser, TTeam>
    {
        b.HasOne(x => x.LastModifierTeam).WithMany()
            .HasForeignKey(x => x.LastModifierTeamId);
        b.HasOne(x => x.LastModifierUser).WithMany()
            .HasForeignKey(x => x.LastModifierUserId);
        b.HasOne(x => x.CreatorTeam).WithMany()
            .HasForeignKey(x => x.CreatorTeamId);
        b.HasOne(x => x.CreatorUser).WithMany()
            .HasForeignKey(x => x.CreatorUserId);
    }

    protected virtual void ConfigAccountEntityHistory< TEntityHistory, TEntity, TEntityId>(
        EntityTypeBuilder<TEntityHistory> b, string tableName)
        where TEntityHistory : AccountEntityHistory<TEntity, TUser, TTeam>,
        IAudited<TUser, TTeam>, IEntity<Guid>
        where TEntity : IEntity<Guid>
    {
        b.ToTable(DbTablePrefix + tableName, DbSchema);
        b.HasKey(x => x.Id);
//        b.Property(e => e.SnapShot).HasColumnType("json");

        b.HasOne(x => x.Team).WithMany()
            .HasForeignKey(x => x.TeamId);
        b.HasOne(x => x.User).WithMany()
            .HasForeignKey(x => x.UserId);
        ConfigureAccountAudit(b);
    }

    protected virtual void EnsureAutoHistory<TEntity, TEntityId, TEntityHistory>()
        where TEntity : class, IEntity<TEntityId>
        where TEntityHistory : class, IEntityHistory<TEntity, TEntityId, TUser, TTeam>, new()
    {
        var entries = ChangeTracker.Entries<TEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified
                or EntityState.Deleted)
            .ToList();

        foreach (var entityEntry in entries)
        {
            var entity = entityEntry.Entity;
            var history = new TEntityHistory
            {
                EntityId = entity.Id,
                Action = entityEntry.State.ToString(),
                SnapShot = entity,
                CreationTime = DateTime.Now,
            };
            Set<TEntityHistory>().Add(history);
        }
    }
}
