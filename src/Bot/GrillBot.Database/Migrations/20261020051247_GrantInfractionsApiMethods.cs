using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    /// <summary>
    /// The public API V2 got InfractionsController next to the legacy UserMeasuresController.
    /// Every API client allowed to call a legacy method is also allowed to call its replacement,
    /// so clients can move to the new endpoints without a manual permission change.
    /// The legacy permissions stay untouched.
    /// </summary>
    public partial class GrantInfractionsApiMethods : Migration
    {
        private static readonly (string Legacy, string Replacement)[] Methods =
        [
            ("UserMeasuresController.CreateUserMeasuresTimeoutAsync", "InfractionsController.CreateTimeoutAsync"),
            ("UserMeasuresController.DeleteUserMeasureTimeoutAsync", "InfractionsController.DeleteTimeoutAsync")
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (legacy, replacement) in Methods)
            {
                migrationBuilder.Sql($"""
                    UPDATE "ApiClients"
                    SET "AllowedMethods" = "AllowedMethods" || '["{replacement}"]'::jsonb
                    WHERE "AllowedMethods" @> '["{legacy}"]'::jsonb
                      AND NOT "AllowedMethods" @> '["{replacement}"]'::jsonb;
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (legacy, replacement) in Methods)
            {
                migrationBuilder.Sql($"""
                    UPDATE "ApiClients"
                    SET "AllowedMethods" = "AllowedMethods" - '{replacement}'
                    WHERE "AllowedMethods" @> '["{legacy}"]'::jsonb;
                    """);
            }
        }
    }
}
