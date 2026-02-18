using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotBoil.Cronos.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScheduledJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    TypeName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    CronExpression = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    TimeZoneId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastRunAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    NextRunAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastDurationMs = table.Column<long>(type: "bigint", nullable: false),
                    LastStatus = table.Column<string>(type: "longtext", nullable: false),
                    LastError = table.Column<string>(type: "longtext", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledJobs", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JobExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ScheduledJobId = table.Column<Guid>(type: "char(36)", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    FinishedAtUtc = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    Error = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobExecutions_ScheduledJobs_ScheduledJobId",
                        column: x => x.ScheduledJobId,
                        principalTable: "ScheduledJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutions_ScheduledJobId",
                table: "JobExecutions",
                column: "ScheduledJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobExecutions");

            migrationBuilder.DropTable(
                name: "ScheduledJobs");
        }
    }
}
