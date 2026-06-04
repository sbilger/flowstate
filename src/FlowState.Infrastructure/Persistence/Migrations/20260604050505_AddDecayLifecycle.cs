using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowState.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDecayLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DecayState",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DormantSince",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastTouchedAt",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_tasks_DecayState",
                table: "tasks",
                column: "DecayState");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tasks_DecayState",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "DecayState",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "DormantSince",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "LastTouchedAt",
                table: "tasks");
        }
    }
}
