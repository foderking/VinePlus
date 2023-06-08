using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comicvine.Database.Migrations
{
    /// <inheritdoc />
    public partial class minorchangeinthreadschema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "comments",
                table: "threads",
                newName: "posts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "posts",
                table: "threads",
                newName: "comments");
        }
    }
}
