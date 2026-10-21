using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotBoil.AuthGuard.Application.Migrations
{
    /// <inheritdoc />
    public partial class UserLastLoginDateAndActiveLanguageAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActiveLanguage",
                table: "Users",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginDate",
                table: "Users",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveLanguage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "Users");
        }
    }
}
