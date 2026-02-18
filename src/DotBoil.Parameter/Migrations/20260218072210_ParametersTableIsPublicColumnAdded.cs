using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotBoil.Parameter.Migrations
{
    /// <inheritdoc />
    public partial class ParametersTableIsPublicColumnAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Parameters",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Parameters");
        }
    }
}
