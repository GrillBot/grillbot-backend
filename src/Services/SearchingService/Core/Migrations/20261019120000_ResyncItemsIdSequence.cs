using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchingService.Core.Migrations
{
    /// <inheritdoc />
    public partial class ResyncItemsIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Moves the identity sequence past the highest existing value (never backwards),
            // so inserts stop colliding with rows that were written with explicit values.
            migrationBuilder.Sql("""
                SELECT setval('"Items_Id_seq"', GREATEST(
                    (SELECT CASE WHEN is_called THEN last_value + 1 ELSE last_value END FROM "Items_Id_seq"),
                    (SELECT COALESCE(MAX("Id"), 0) + 1 FROM "Items")
                ), false);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
