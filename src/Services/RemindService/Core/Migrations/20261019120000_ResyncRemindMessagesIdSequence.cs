using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RemindService.Core.Migrations
{
    /// <inheritdoc />
    public partial class ResyncRemindMessagesIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Moves the identity sequence past the highest existing value (never backwards),
            // so inserts stop colliding with rows that were written with explicit values.
            migrationBuilder.Sql("""
                SELECT setval('"RemindMessages_Id_seq"', GREATEST(
                    (SELECT CASE WHEN is_called THEN last_value + 1 ELSE last_value END FROM "RemindMessages_Id_seq"),
                    (SELECT COALESCE(MAX("Id"), 0) + 1 FROM "RemindMessages")
                ), false);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
