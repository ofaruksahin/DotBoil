using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace DotBoil.AuthGuard.Application.Migrations
{
    /// <inheritdoc />
    public partial class AccountDeactivationAndNotificationPreferencesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPassive",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PassiveDate",
                table: "Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Purpose",
                table: "OtpCodes",
                type: "int",
                nullable: false,
                defaultValue: 1); // Existing codes were all created by the forgot password flow.

            migrationBuilder.CreateTable(
                name: "UserNotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreateUser = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    ModifyUser = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCodes_UserId_Purpose",
                table: "OtpCodes",
                columns: new[] { "UserId", "Purpose" });

            // Composite index above already starts with UserId, so the FK keeps an index.
            migrationBuilder.DropIndex(
                name: "IX_OtpCodes_UserId",
                table: "OtpCodes");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_UserId_Channel",
                table: "UserNotificationPreferences",
                columns: new[] { "UserId", "Channel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserNotificationPreferences");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCodes_UserId",
                table: "OtpCodes",
                column: "UserId");

            migrationBuilder.DropIndex(
                name: "IX_OtpCodes_UserId_Purpose",
                table: "OtpCodes");

            migrationBuilder.DropColumn(
                name: "IsPassive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PassiveDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "OtpCodes");

        }
    }
}
