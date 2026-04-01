using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace DotBoil.AuthGuard.Application.Migrations
{
    /// <inheritdoc />
    public partial class RoleAppModulesTableCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleAppModules_AppModules_AppModuleId",
                table: "RoleAppModules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleAppModules",
                table: "RoleAppModules");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RoleAppModules",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "RoleAppModules",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreateUser",
                table: "RoleAppModules",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RoleAppModules",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifyUser",
                table: "RoleAppModules",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "RoleAppModules",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleAppModules",
                table: "RoleAppModules",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAppModules_RoleId",
                table: "RoleAppModules",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleAppModules_AppModules_AppModuleId",
                table: "RoleAppModules",
                column: "AppModuleId",
                principalTable: "AppModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleAppModules_AppModules_AppModuleId",
                table: "RoleAppModules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleAppModules",
                table: "RoleAppModules");

            migrationBuilder.DropIndex(
                name: "IX_RoleAppModules_RoleId",
                table: "RoleAppModules");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RoleAppModules");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "RoleAppModules");

            migrationBuilder.DropColumn(
                name: "CreateUser",
                table: "RoleAppModules");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RoleAppModules");

            migrationBuilder.DropColumn(
                name: "ModifyUser",
                table: "RoleAppModules");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "RoleAppModules");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleAppModules",
                table: "RoleAppModules",
                columns: new[] { "RoleId", "AppModuleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_RoleAppModules_AppModules_AppModuleId",
                table: "RoleAppModules",
                column: "AppModuleId",
                principalTable: "AppModules",
                principalColumn: "Id");
        }
    }
}
