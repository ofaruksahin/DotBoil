using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotBoil.AuthGuard.Application.Migrations
{
    /// <inheritdoc />
    public partial class snapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleApiEndpoints",
                columns: table => new
                {
                    ApiEndpointId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleApiEndpoints", x => new { x.ApiEndpointId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_RoleApiEndpoints_ApiEndpoints_ApiEndpointId",
                        column: x => x.ApiEndpointId,
                        principalTable: "ApiEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleApiEndpoints_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RoleApiEndpoints_RoleId",
                table: "RoleApiEndpoints",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleApiEndpoints");
        }
    }
}
