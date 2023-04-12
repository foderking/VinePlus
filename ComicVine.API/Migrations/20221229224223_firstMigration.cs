using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ComicVine.API.Migrations
{
    /// <inheritdoc />
    public partial class firstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComicvineUser",
                columns: table => new
                {
                    GalleryId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComicvineUser", x => x.GalleryId);
                });

            migrationBuilder.CreateTable(
                name: "Threads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ThreadTitle = table.Column<string>(type: "text", nullable: false),
                    ThreadLink = table.Column<string>(type: "text", nullable: false),
                    BoardName = table.Column<string>(type: "text", nullable: false),
                    BoardLink = table.Column<string>(type: "text", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    ThreadType = table.Column<string>(type: "text", nullable: false),
                    MostRecentPostNo = table.Column<int>(type: "integer", nullable: false),
                    MostRecentPostPage = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NoPost = table.Column<int>(type: "integer", nullable: false),
                    NoViews = table.Column<int>(type: "integer", nullable: false),
                    CreatorLink = table.Column<string>(type: "text", nullable: false),
                    CreatorGalleryId = table.Column<string>(type: "text", nullable: false),
                    CreatorName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Threads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Threads_ComicvineUser_CreatorGalleryId",
                        column: x => x.CreatorGalleryId,
                        principalTable: "ComicvineUser",
                        principalColumn: "GalleryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Threads_CreatorGalleryId",
                table: "Threads",
                column: "CreatorGalleryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Threads");

            migrationBuilder.DropTable(
                name: "ComicvineUser");
        }
    }
}
