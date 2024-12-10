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
                name: "Accounts_Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PasswordSalt = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_Users", x => x.Id);
                    table.UniqueConstraint("AK_Accounts_Users_UserId", x => x.UserId);
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
                    table.ForeignKey(
                        name: "FK_Accounts_RolePermissions_Accounts_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Accounts_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_UserRoles",
                columns: table => new
                {
                    RolesId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsersId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_UserRoles", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_Accounts_UserRoles_Accounts_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Accounts_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_UserRoles_Accounts_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Accounts_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_UserTeams",
                columns: table => new
                {
                    TeamsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsersId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_UserTeams", x => new { x.TeamsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_Accounts_UserTeams_Accounts_Teams_TeamsId",
                        column: x => x.TeamsId,
                        principalTable: "Accounts_Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_UserTeams_Accounts_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Accounts_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RolePermissions_RolesId",
                table: "Accounts_RolePermissions",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserRoles_UsersId",
                table: "Accounts_UserRoles",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserTeams_UsersId",
                table: "Accounts_UserTeams",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts_RolePermissions");

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
