using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComicVine.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumPost_Threads_ThreadId",
                table: "ForumPost");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ForumPost",
                table: "ForumPost");

            migrationBuilder.RenameTable(
                name: "ForumPost",
                newName: "Posts");

            migrationBuilder.RenameIndex(
                name: "IX_ForumPost_ThreadId",
                table: "Posts",
                newName: "IX_Posts_ThreadId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Posts",
                table: "Posts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Threads_ThreadId",
                table: "Posts",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Threads_ThreadId",
                table: "Posts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Posts",
                table: "Posts");

            migrationBuilder.RenameTable(
                name: "Posts",
                newName: "ForumPost");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_ThreadId",
                table: "ForumPost",
                newName: "IX_ForumPost_ThreadId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForumPost",
                table: "ForumPost",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ForumPost_Threads_ThreadId",
                table: "ForumPost",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
