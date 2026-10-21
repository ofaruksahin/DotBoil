using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotBoil.UserConsents.Migrations
{
    /// <inheritdoc />
    public partial class UserConsentsClientInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "UserConsents",
                type: "varchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "UserConsents",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "UserConsents");
        }
    }
}
