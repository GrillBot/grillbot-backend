using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditLogService.Core.Migrations
{
    /// <inheritdoc />
    public partial class ArchivationLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchivationLogs",
                columns: table => new
                {
                    LogItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TotalRecordsCount = table.Column<int>(type: "integer", nullable: false),
                    JsonSize = table.Column<long>(type: "bigint", nullable: false),
                    ArchiveSize = table.Column<long>(type: "bigint", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    DeletedFilesCount = table.Column<int>(type: "integer", nullable: true),
                    DeletedFilesSize = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivationLogs", x => x.LogItemId);
                    table.ForeignKey(
                        name: "FK_ArchivationLogs_LogItems_LogItemId",
                        column: x => x.LogItemId,
                        principalTable: "LogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogItems_Type_CreatedAt",
                table: "LogItems",
                columns: new[] { "Type", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivationLogs");

            migrationBuilder.DropIndex(
                name: "IX_LogItems_Type_CreatedAt",
                table: "LogItems");
        }
    }
}
