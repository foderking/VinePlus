using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinePlus.Database.Migrations
{
    /// <inheritdoc />
    public partial class indexonthreadcreatedandpostcreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "created",
            //    table: "threads",
            //    type: "timestamp without time zone",
            //    nullable: false,
            //    oldClrType: typeof(DateTime),
            //    oldType: "timestamp with time zone");

            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "created",
            //    table: "posts",
            //    type: "timestamp without time zone",
            //    nullable: false,
            //    oldClrType: typeof(DateTime),
            //    oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "ix_threads_created",
                table: "threads",
                column: "created",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_posts_created",
                table: "posts",
                column: "created",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_threads_created",
                table: "threads");

            migrationBuilder.DropIndex(
                name: "ix_posts_created",
                table: "posts");

            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "created",
            //    table: "threads",
            //    type: "timestamp with time zone",
            //    nullable: false,
            //    oldClrType: typeof(DateTime),
            //    oldType: "timestamp without time zone");

            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "created",
            //    table: "posts",
            //    type: "timestamp with time zone",
            //    nullable: false,
            //    oldClrType: typeof(DateTime),
            //    oldType: "timestamp without time zone");
        }
    }
}
