using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotBoil.AuthGuard.Application.Migrations
{
    /// <inheritdoc />
    public partial class OtpCodesTableIsExpiredColumnAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExpired",
                table: "OtpCodes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExpired",
                table: "OtpCodes");
        }
    }
}
