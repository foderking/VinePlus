using System;
using Comicvine.Core;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinePlus.Database.Migrations
{
    /// <inheritdoc />
    public partial class addpostentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "posts",
                table: "threads");

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_comment = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_mod_comment = table.Column<bool>(type: "boolean", nullable: false),
                    post_no = table.Column<int>(type: "integer", nullable: false),
                    creator = table.Column<Parsers.Link>(type: "jsonb", nullable: true),
                    is_edited = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    thread_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posts", x => x.id);
                    table.ForeignKey(
                        name: "fk_posts_threads_thread_id",
                        column: x => x.thread_id,
                        principalTable: "threads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_posts_thread_id",
                table: "posts",
                column: "thread_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.AddColumn<int[]>(
                name: "posts",
                table: "threads",
                type: "integer[]",
                nullable: true);
        }
    }
}
