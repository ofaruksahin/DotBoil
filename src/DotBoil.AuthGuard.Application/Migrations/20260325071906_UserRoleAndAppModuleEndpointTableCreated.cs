using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace DotBoil.AuthGuard.Application.Migrations
{
    /// <inheritdoc />
    public partial class UserRoleAndAppModuleEndpointTableCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppModuleEndpoints_AppModules_AppModuleId",
                table: "AppModuleEndpoints");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleApiEndpoints_ApiEndpoints_ApiEndpointId",
                table: "RoleApiEndpoints");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleApiEndpoints",
                table: "RoleApiEndpoints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppModuleEndpoints",
                table: "AppModuleEndpoints");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserRoles",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "UserRoles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreateUser",
                table: "UserRoles",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserRoles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifyUser",
                table: "UserRoles",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "UserRoles",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RoleApiEndpoints",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "RoleApiEndpoints",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreateUser",
                table: "RoleApiEndpoints",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RoleApiEndpoints",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifyUser",
                table: "RoleApiEndpoints",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "RoleApiEndpoints",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AppModuleEndpoints",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "AppModuleEndpoints",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreateUser",
                table: "AppModuleEndpoints",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppModuleEndpoints",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifyUser",
                table: "AppModuleEndpoints",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "AppModuleEndpoints",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleApiEndpoints",
                table: "RoleApiEndpoints",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppModuleEndpoints",
                table: "AppModuleEndpoints",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleApiEndpoints_RoleId",
                table: "RoleApiEndpoints",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppModuleEndpoints_ApiEndpointId",
                table: "AppModuleEndpoints",
                column: "ApiEndpointId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppModuleEndpoints_AppModules_AppModuleId",
                table: "AppModuleEndpoints",
                column: "AppModuleId",
                principalTable: "AppModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleApiEndpoints_ApiEndpoints_ApiEndpointId",
                table: "RoleApiEndpoints",
                column: "ApiEndpointId",
                principalTable: "ApiEndpoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppModuleEndpoints_AppModules_AppModuleId",
                table: "AppModuleEndpoints");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleApiEndpoints_ApiEndpoints_ApiEndpointId",
                table: "RoleApiEndpoints");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleApiEndpoints",
                table: "RoleApiEndpoints");

            migrationBuilder.DropIndex(
                name: "IX_RoleApiEndpoints_RoleId",
                table: "RoleApiEndpoints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppModuleEndpoints",
                table: "AppModuleEndpoints");

            migrationBuilder.DropIndex(
                name: "IX_AppModuleEndpoints_ApiEndpointId",
                table: "AppModuleEndpoints");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "CreateUser",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "ModifyUser",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RoleApiEndpoints");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "RoleApiEndpoints");

            migrationBuilder.DropColumn(
                name: "CreateUser",
                table: "RoleApiEndpoints");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RoleApiEndpoints");

            migrationBuilder.DropColumn(
                name: "ModifyUser",
                table: "RoleApiEndpoints");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "RoleApiEndpoints");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AppModuleEndpoints");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "AppModuleEndpoints");

            migrationBuilder.DropColumn(
                name: "CreateUser",
                table: "AppModuleEndpoints");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppModuleEndpoints");

            migrationBuilder.DropColumn(
                name: "ModifyUser",
                table: "AppModuleEndpoints");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "AppModuleEndpoints");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleApiEndpoints",
                table: "RoleApiEndpoints",
                columns: new[] { "RoleId", "ApiEndpointId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppModuleEndpoints",
                table: "AppModuleEndpoints",
                columns: new[] { "ApiEndpointId", "AppModuleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppModuleEndpoints_AppModules_AppModuleId",
                table: "AppModuleEndpoints",
                column: "AppModuleId",
                principalTable: "AppModules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleApiEndpoints_ApiEndpoints_ApiEndpointId",
                table: "RoleApiEndpoints",
                column: "ApiEndpointId",
                principalTable: "ApiEndpoints",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}
