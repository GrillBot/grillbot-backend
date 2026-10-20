using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementService.Core.Migrations
{
    /// <inheritdoc />
    public partial class MergeInfractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "infractions");

            migrationBuilder.CreateTable(
                name: "Timeouts",
                schema: "infractions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExternalId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModeratorId = table.Column<decimal>(type: "numeric", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timeouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Unverifies",
                schema: "infractions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LogSetId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModeratorId = table.Column<decimal>(type: "numeric", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unverifies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warnings",
                schema: "infractions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModeratorId = table.Column<decimal>(type: "numeric", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warnings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Timeouts_ExternalId",
                schema: "infractions",
                table: "Timeouts",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Timeouts_GuildId_UserId",
                schema: "infractions",
                table: "Timeouts",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Unverifies_GuildId_UserId",
                schema: "infractions",
                table: "Unverifies",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Unverifies_LogSetId",
                schema: "infractions",
                table: "Unverifies",
                column: "LogSetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warnings_GuildId_UserId",
                schema: "infractions",
                table: "Warnings",
                columns: new[] { "GuildId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Timeouts",
                schema: "infractions");

            migrationBuilder.DropTable(
                name: "Unverifies",
                schema: "infractions");

            migrationBuilder.DropTable(
                name: "Warnings",
                schema: "infractions");
        }
    }
}
