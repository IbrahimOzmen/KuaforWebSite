using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KuaforRandevuSistemi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkingHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkDate",
                table: "WorkingHours");

            migrationBuilder.AddColumn<int>(
                name: "DayOfWeek",
                table: "WorkingHours",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsWorkingDay",
                table: "WorkingHours",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "WorkingHours");

            migrationBuilder.DropColumn(
                name: "IsWorkingDay",
                table: "WorkingHours");

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkDate",
                table: "WorkingHours",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
