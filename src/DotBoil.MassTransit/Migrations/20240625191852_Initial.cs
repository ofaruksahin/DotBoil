using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotBoil.MassTransit.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "MassTransit");

            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Inbox",
                schema: "MassTransit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    MessageId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inbox", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Outbox",
                schema: "MassTransit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    MessageType = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false),
                    QueueName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    Content = table.Column<string>(type: "varchar(8192)", maxLength: 8192, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Processed = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outbox", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inbox",
                schema: "MassTransit");

            migrationBuilder.DropTable(
                name: "Outbox",
                schema: "MassTransit");
        }
    }
}
