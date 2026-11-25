using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sutnance.Migrations
{
    /// <inheritdoc />
    public partial class _833c75cd58af4db89eab55a2a2c5b066 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "alerts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "alerts");
        }
    }
}
