using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowState.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScoringFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Importance",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSnoozedAt",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SnoozeCount",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_tasks_Status",
                table: "tasks",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tasks_Status",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "Importance",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "LastSnoozedAt",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "SnoozeCount",
                table: "tasks");
        }
    }
}
