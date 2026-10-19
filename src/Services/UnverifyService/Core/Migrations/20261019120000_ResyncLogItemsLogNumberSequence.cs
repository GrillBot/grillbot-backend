using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnverifyService.Core.Migrations
{
    /// <inheritdoc />
    public partial class ResyncLogItemsLogNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Moves the identity sequence past the highest existing value (never backwards),
            // so inserts stop colliding with rows that were written with explicit values.
            migrationBuilder.Sql("""
                SELECT setval('logs."LogItems_LogNumber_seq"', GREATEST(
                    (SELECT CASE WHEN is_called THEN last_value + 1 ELSE last_value END FROM logs."LogItems_LogNumber_seq"),
                    (SELECT COALESCE(MAX("LogNumber"), 0) + 1 FROM logs."LogItems")
                ), false);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
