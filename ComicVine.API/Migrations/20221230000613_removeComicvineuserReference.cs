using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComicVine.API.Migrations
{
    /// <inheritdoc />
    public partial class removeComicvineuserReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Threads_ComicvineUser_CreatorGalleryId",
                table: "Threads");

            migrationBuilder.DropTable(
                name: "ComicvineUser");

            migrationBuilder.DropIndex(
                name: "IX_Threads_CreatorGalleryId",
                table: "Threads");

            migrationBuilder.DropColumn(
                name: "CreatorGalleryId",
                table: "Threads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorGalleryId",
                table: "Threads",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ComicvineUser",
                columns: table => new
                {
                    GalleryId = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComicvineUser", x => x.GalleryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Threads_CreatorGalleryId",
                table: "Threads",
                column: "CreatorGalleryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Threads_ComicvineUser_CreatorGalleryId",
                table: "Threads",
                column: "CreatorGalleryId",
                principalTable: "ComicvineUser",
                principalColumn: "GalleryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
