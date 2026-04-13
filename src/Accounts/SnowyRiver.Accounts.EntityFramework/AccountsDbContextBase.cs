using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.Domain.Entities;
using SnowyRiver.Domain.Shared.Entities;
using SnowyRiver.EF;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnowyRiver.Accounts.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.EntityFramework;

public class AccountsDbContextBase<TUser, TRole, TTeam, TPermission,
    TUserHistory, TRoleHistory, TTeamHistory, TPermissionHistory>(DbContextOptions options) 
    : SnowyRiverDbContext(options)
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
    where TUserHistory: UserHistoryBase<TUser, TRole, TTeam, TPermission>
    where TRoleHistory: RoleHistoryBase<TUser, TRole, TTeam, TPermission>
    where TTeamHistory: TeamHistoryBase<TUser, TRole, TTeam, TPermission>
    where TPermissionHistory: PermissionHistoryBase<TUser, TRole, TTeam, TPermission>
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
            ConfigureAccountAuditForeignKeys(b);
        });


        modelBuilder.Entity<TRole>(b =>
        {
            b.ToTable(DbTablePrefix + "Roles", DbSchema);
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.HasMany(e => e.Permissions).WithMany(e => e.Roles)
                .UsingEntity(DbTablePrefix + "RolePermissions");
            ConfigureAccountAuditForeignKeys(b);
        });

        modelBuilder.Entity<TTeam>(b =>
        {
            b.ToTable(DbTablePrefix + "Teams", DbSchema);
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.HasMany(e => e.Users).WithMany(e => e.Teams)
                .UsingEntity(DbTablePrefix + "UserTeams");
            ConfigureAccountAuditForeignKeys(b);
        });

        ConfigEntityHistory<TUser, Guid, TUserHistory>(modelBuilder, "UserHistories");
        ConfigEntityHistory<TRole, Guid, TRoleHistory>(modelBuilder, "RoleHistories");
        ConfigEntityHistory<TTeam, Guid,  TTeamHistory>(modelBuilder, "TeamHistories");
        ConfigEntityHistory<TPermission, Guid, TPermissionHistory>(modelBuilder, "PermissionHistory");
    }

    protected new void ConfigEntityHistory<TEntity, TEntityId, TEntityHistory>(
        ModelBuilder modelBuilder, string tableName)
        where TEntityHistory: AccountEntityHistory<TEntity, TEntityId, TUser, TRole, TTeam, TPermission>
        where TEntity: IEntity<TEntityId>
    {
        base.ConfigEntityHistory<TEntity, TEntityId, TEntityHistory>(modelBuilder, tableName);

        modelBuilder.Entity<TEntityHistory>(b =>
        {
            b.HasOne(x => x.Team).WithMany()
                .HasForeignKey(x => x.TeamId);
            b.HasOne(x => x.User).WithMany()
                .HasForeignKey(x => x.UserId);
            ConfigureAccountAuditForeignKeys(b);
        });
    }

    private static void ConfigureAccountAuditForeignKeys<TEntity>(EntityTypeBuilder<TEntity> b)
        where TEntity : class, IAccountAuditedEntity<TUser,TTeam>
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
}
