using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnowyRiver.Accounts.Manager.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts_PermissionHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatorTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    SnapShotId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_PermissionHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatorTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    SortId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Alias = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Permissions_Accounts_Permissions_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Accounts_Permissions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Accounts_RoleHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatorTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    SnapShotId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_RoleHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_RolePermissions",
                columns: table => new
                {
                    PermissionsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RolesId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_RolePermissions", x => new { x.PermissionsId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_Accounts_RolePermissions_Accounts_Permissions_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "Accounts_Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatorTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Alias = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Scope = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_TeamHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatorTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    SnapShotId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_TeamHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatorTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Teams_Accounts_Teams_CreatorTeamId",
                        column: x => x.CreatorTeamId,
                        principalTable: "Accounts_Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_Teams_Accounts_Teams_LastModifierTeamId",
                        column: x => x.LastModifierTeamId,
                        principalTable: "Accounts_Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Accounts_Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatorTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PasswordSalt = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Users_Accounts_Teams_CreatorTeamId",
                        column: x => x.CreatorTeamId,
                        principalTable: "Accounts_Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_Users_Accounts_Teams_LastModifierTeamId",
                        column: x => x.LastModifierTeamId,
                        principalTable: "Accounts_Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_Users_Accounts_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "Accounts_Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_Users_Accounts_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalTable: "Accounts_Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Accounts_UserHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatorTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifierTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    SnapShotId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_UserHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_UserHistories_Accounts_Teams_CreatorTeamId",
                        column: x => x.CreatorTeamId,
                        principalTable: "Accounts_Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_UserHistories_Accounts_Teams_LastModifierTeamId",
                        column: x => x.LastModifierTeamId,
                        principalTable: "Accounts_Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_UserHistories_Accounts_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Accounts_Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_UserHistories_Accounts_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "Accounts_Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_UserHistories_Accounts_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalTable: "Accounts_Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_UserHistories_Accounts_Users_SnapShotId",
                        column: x => x.SnapShotId,
                        principalTable: "Accounts_Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_UserHistories_Accounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts_Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Accounts_UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_Accounts_UserRoles_Accounts_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Accounts_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_UserRoles_Accounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_UserTeams",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_UserTeams", x => new { x.UserId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_Accounts_UserTeams_Accounts_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Accounts_Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_UserTeams_Accounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PermissionHistories_CreatorTeamId",
                table: "Accounts_PermissionHistories",
                column: "CreatorTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PermissionHistories_CreatorUserId",
                table: "Accounts_PermissionHistories",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PermissionHistories_LastModifierTeamId",
                table: "Accounts_PermissionHistories",
                column: "LastModifierTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PermissionHistories_LastModifierUserId",
                table: "Accounts_PermissionHistories",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PermissionHistories_SnapShotId",
                table: "Accounts_PermissionHistories",
                column: "SnapShotId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PermissionHistories_TeamId",
                table: "Accounts_PermissionHistories",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PermissionHistories_UserId",
                table: "Accounts_PermissionHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Permissions_CreatorTeamId",
                table: "Accounts_Permissions",
                column: "CreatorTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Permissions_CreatorUserId",
                table: "Accounts_Permissions",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Permissions_LastModifierTeamId",
                table: "Accounts_Permissions",
                column: "LastModifierTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Permissions_LastModifierUserId",
                table: "Accounts_Permissions",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Permissions_ParentId",
                table: "Accounts_Permissions",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleHistories_CreatorTeamId",
                table: "Accounts_RoleHistories",
                column: "CreatorTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleHistories_CreatorUserId",
                table: "Accounts_RoleHistories",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleHistories_LastModifierTeamId",
                table: "Accounts_RoleHistories",
                column: "LastModifierTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleHistories_LastModifierUserId",
                table: "Accounts_RoleHistories",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleHistories_SnapShotId",
                table: "Accounts_RoleHistories",
                column: "SnapShotId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleHistories_TeamId",
                table: "Accounts_RoleHistories",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleHistories_UserId",
                table: "Accounts_RoleHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RolePermissions_RolesId",
                table: "Accounts_RolePermissions",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Roles_CreatorTeamId",
                table: "Accounts_Roles",
                column: "CreatorTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Roles_CreatorUserId",
                table: "Accounts_Roles",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Roles_LastModifierTeamId",
                table: "Accounts_Roles",
                column: "LastModifierTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Roles_LastModifierUserId",
                table: "Accounts_Roles",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TeamHistories_CreatorTeamId",
                table: "Accounts_TeamHistories",
                column: "CreatorTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TeamHistories_CreatorUserId",
                table: "Accounts_TeamHistories",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TeamHistories_LastModifierTeamId",
                table: "Accounts_TeamHistories",
                column: "LastModifierTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TeamHistories_LastModifierUserId",
                table: "Accounts_TeamHistories",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TeamHistories_SnapShotId",
                table: "Accounts_TeamHistories",
                column: "SnapShotId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TeamHistories_TeamId",
                table: "Accounts_TeamHistories",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TeamHistories_UserId",
                table: "Accounts_TeamHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Teams_CreatorTeamId",
                table: "Accounts_Teams",
                column: "CreatorTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Teams_CreatorUserId",
                table: "Accounts_Teams",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Teams_LastModifierTeamId",
                table: "Accounts_Teams",
                column: "LastModifierTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Teams_LastModifierUserId",
                table: "Accounts_Teams",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserHistories_CreatorTeamId",
                table: "Accounts_UserHistories",
                column: "CreatorTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserHistories_CreatorUserId",
                table: "Accounts_UserHistories",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserHistories_LastModifierTeamId",
                table: "Accounts_UserHistories",
                column: "LastModifierTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserHistories_LastModifierUserId",
                table: "Accounts_UserHistories",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserHistories_SnapShotId",
                table: "Accounts_UserHistories",
                column: "SnapShotId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserHistories_TeamId",
                table: "Accounts_UserHistories",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserHistories_UserId",
                table: "Accounts_UserHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserRoles_RoleId",
                table: "Accounts_UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Users_CreatorTeamId",
                table: "Accounts_Users",
                column: "CreatorTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Users_CreatorUserId",
                table: "Accounts_Users",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Users_LastModifierTeamId",
                table: "Accounts_Users",
                column: "LastModifierTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Users_LastModifierUserId",
                table: "Accounts_Users",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserTeams_TeamId",
                table: "Accounts_UserTeams",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_PermissionHistories_Accounts_Permissions_SnapShotId",
                table: "Accounts_PermissionHistories",
                column: "SnapShotId",
                principalTable: "Accounts_Permissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_PermissionHistories_Accounts_Teams_CreatorTeamId",
                table: "Accounts_PermissionHistories",
                column: "CreatorTeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_PermissionHistories_Accounts_Teams_LastModifierTeamId",
                table: "Accounts_PermissionHistories",
                column: "LastModifierTeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_PermissionHistories_Accounts_Teams_TeamId",
                table: "Accounts_PermissionHistories",
                column: "TeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_PermissionHistories_Accounts_Users_CreatorUserId",
                table: "Accounts_PermissionHistories",
                column: "CreatorUserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_PermissionHistories_Accounts_Users_LastModifierUserId",
                table: "Accounts_PermissionHistories",
                column: "LastModifierUserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_PermissionHistories_Accounts_Users_UserId",
                table: "Accounts_PermissionHistories",
                column: "UserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Permissions_Accounts_Teams_CreatorTeamId",
                table: "Accounts_Permissions",
                column: "CreatorTeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Permissions_Accounts_Teams_LastModifierTeamId",
                table: "Accounts_Permissions",
                column: "LastModifierTeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Permissions_Accounts_Users_CreatorUserId",
                table: "Accounts_Permissions",
                column: "CreatorUserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Permissions_Accounts_Users_LastModifierUserId",
                table: "Accounts_Permissions",
                column: "LastModifierUserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_RoleHistories_Accounts_Roles_SnapShotId",
                table: "Accounts_RoleHistories",
                column: "SnapShotId",
                principalTable: "Accounts_Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_RoleHistories_Accounts_Teams_CreatorTeamId",
                table: "Accounts_RoleHistories",
                column: "CreatorTeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_RoleHistories_Accounts_Teams_LastModifierTeamId",
                table: "Accounts_RoleHistories",
                column: "LastModifierTeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_RoleHistories_Accounts_Teams_TeamId",
                table: "Accounts_RoleHistories",
                column: "TeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_RoleHistories_Accounts_Users_CreatorUserId",
                table: "Accounts_RoleHistories",
                column: "CreatorUserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_RoleHistories_Accounts_Users_LastModifierUserId",
                table: "Accounts_RoleHistories",
                column: "LastModifierUserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_RoleHistories_Accounts_Users_UserId",
                table: "Accounts_RoleHistories",
                column: "UserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_RolePermissions_Accounts_Roles_RolesId",
                table: "Accounts_RolePermissions",
                column: "RolesId",
                principalTable: "Accounts_Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Roles_Accounts_Teams_CreatorTeamId",
                table: "Accounts_Roles",
                column: "CreatorTeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Roles_Accounts_Teams_LastModifierTeamId",
                table: "Accounts_Roles",
                column: "LastModifierTeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Roles_Accounts_Users_CreatorUserId",
                table: "Accounts_Roles",
                column: "CreatorUserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Roles_Accounts_Users_LastModifierUserId",
                table: "Accounts_Roles",
                column: "LastModifierUserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_TeamHistories_Accounts_Teams_CreatorTeamId",
                table: "Accounts_TeamHistories",
                column: "CreatorTeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_TeamHistories_Accounts_Teams_LastModifierTeamId",
                table: "Accounts_TeamHistories",
                column: "LastModifierTeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_TeamHistories_Accounts_Teams_SnapShotId",
                table: "Accounts_TeamHistories",
                column: "SnapShotId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_TeamHistories_Accounts_Teams_TeamId",
                table: "Accounts_TeamHistories",
                column: "TeamId",
                principalTable: "Accounts_Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_TeamHistories_Accounts_Users_CreatorUserId",
                table: "Accounts_TeamHistories",
                column: "CreatorUserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_TeamHistories_Accounts_Users_LastModifierUserId",
                table: "Accounts_TeamHistories",
                column: "LastModifierUserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_TeamHistories_Accounts_Users_UserId",
                table: "Accounts_TeamHistories",
                column: "UserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Teams_Accounts_Users_CreatorUserId",
                table: "Accounts_Teams",
                column: "CreatorUserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Teams_Accounts_Users_LastModifierUserId",
                table: "Accounts_Teams",
                column: "LastModifierUserId",
                principalTable: "Accounts_Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_Accounts_Teams_CreatorTeamId",
                table: "Accounts_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_Accounts_Teams_LastModifierTeamId",
                table: "Accounts_Users");

            migrationBuilder.DropTable(
                name: "Accounts_PermissionHistories");

            migrationBuilder.DropTable(
                name: "Accounts_RoleHistories");

            migrationBuilder.DropTable(
                name: "Accounts_RolePermissions");

            migrationBuilder.DropTable(
                name: "Accounts_TeamHistories");

            migrationBuilder.DropTable(
                name: "Accounts_UserHistories");

            migrationBuilder.DropTable(
                name: "Accounts_UserRoles");

            migrationBuilder.DropTable(
                name: "Accounts_UserTeams");

            migrationBuilder.DropTable(
                name: "Accounts_Permissions");

            migrationBuilder.DropTable(
                name: "Accounts_Roles");

            migrationBuilder.DropTable(
                name: "Accounts_Teams");

            migrationBuilder.DropTable(
                name: "Accounts_Users");
        }
    }
}
