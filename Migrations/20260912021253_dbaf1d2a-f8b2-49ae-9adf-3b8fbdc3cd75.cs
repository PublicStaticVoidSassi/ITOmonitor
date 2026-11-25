using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sutnance.Migrations
{
    /// <inheritdoc />
    public partial class dbaf1d2af8b249ae9adf3b8fbdc3cd75 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Notes",
                table: "Notes");

            migrationBuilder.RenameTable(
                name: "Notes",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "notes",
                newName: "content");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notes",
                table: "notes",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_notes",
                table: "notes");

            migrationBuilder.RenameTable(
                name: "notes",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "Notes",
                newName: "Content");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notes",
                table: "Notes",
                column: "Id");
        }
    }
}
