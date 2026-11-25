using Microsoft.EntityFrameworkCore.Migrations;
using sutnance.hacking;

#nullable disable

namespace sutnance.Migrations
{
    /// <inheritdoc />
    public partial class _3ff6e5042bbc42548fc3f5c5d51c9cb4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddHistoriqueView();
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
