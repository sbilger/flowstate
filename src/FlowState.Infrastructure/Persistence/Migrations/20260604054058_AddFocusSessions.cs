using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowState.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFocusSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "focus_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlannedDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Outcome = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_focus_sessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_focus_sessions_StartedAt",
                table: "focus_sessions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_focus_sessions_TaskId",
                table: "focus_sessions",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "focus_sessions");
        }
    }
}
