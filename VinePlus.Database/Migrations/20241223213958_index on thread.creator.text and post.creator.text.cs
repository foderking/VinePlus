using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinePlus.Database.Migrations
{
    /// <inheritdoc />
    public partial class indexonthreadcreatortextandpostcreatortext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE INDEX ix_posts_creator_text ON posts ((creator->>'Text'));"
            );
            migrationBuilder.Sql(
                "CREATE INDEX ix_threads_creator_text ON threads ((creator->>'Text'));"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS ix_posts_creator_text;"
            );
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS ix_threads_creator_text;"
            );
        }
    }
}
