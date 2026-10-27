using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCameraAuditModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoadingAudits",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    TripReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingAudits", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_LoadingAudits_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadingAudits_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoadingDiscrepancies",
                columns: table => new
                {
                    DiscrepancyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductUnitId = table.Column<int>(type: "int", nullable: false),
                    ExpectedQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ActualQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DifferenceQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscrepancyType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Shortage"),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Open"),
                    Resolution = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedByUserId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingDiscrepancies", x => x.DiscrepancyId);
                    table.ForeignKey(
                        name: "FK_LoadingDiscrepancies_LoadingAudits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "LoadingAudits",
                        principalColumn: "AuditId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoadingDiscrepancies_ProductUnits_ProductUnitId",
                        column: x => x.ProductUnitId,
                        principalTable: "ProductUnits",
                        principalColumn: "ProductUnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadingDiscrepancies_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadingDiscrepancies_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseNotes",
                columns: table => new
                {
                    NoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscrepancyId = table.Column<int>(type: "int", nullable: false),
                    NoteText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    StatusChange = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseNotes", x => x.NoteId);
                    table.ForeignKey(
                        name: "FK_CaseNotes_LoadingDiscrepancies_DiscrepancyId",
                        column: x => x.DiscrepancyId,
                        principalTable: "LoadingDiscrepancies",
                        principalColumn: "DiscrepancyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseNotes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$6M1X9d/votkomS6pEm2qEe/GaTxtl4PUWeBw1QzdmNn66OUMXUDsa");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$7sXUsZqcMr3Ir6uM3HfBEOip3CwBVS0ad5XF7Zzfcn9ihldmq8sw6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$oQGFlRMrivAD1MIvrzeYEOl1pKoSgJNy0Pf8x4uemJtthRNiZnQQq");

            migrationBuilder.CreateIndex(
                name: "IX_CaseNotes_CreatedByUserId",
                table: "CaseNotes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseNotes_DiscrepancyId",
                table: "CaseNotes",
                column: "DiscrepancyId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingAudits_DriverId",
                table: "LoadingAudits",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingAudits_ReviewedByUserId",
                table: "LoadingAudits",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingDiscrepancies_AuditId",
                table: "LoadingDiscrepancies",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingDiscrepancies_ProductId",
                table: "LoadingDiscrepancies",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingDiscrepancies_ProductUnitId",
                table: "LoadingDiscrepancies",
                column: "ProductUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadingDiscrepancies_ResolvedByUserId",
                table: "LoadingDiscrepancies",
                column: "ResolvedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseNotes");

            migrationBuilder.DropTable(
                name: "LoadingDiscrepancies");

            migrationBuilder.DropTable(
                name: "LoadingAudits");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$xPgZxFOj3mEFXWNQYYlwd.Je9YEP5rmLFUNW1q6FDiwfc15s57faW");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$tJehpeEwuTMrLlMUNeu7S.mUSUXuunp2MhHqMtfvpt2uwXRDhkBv.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$4iZ.DDBmJeINqLFhHcbmOOGs7EI2ELd6B6.cJQWGY82Z87845yErS");
        }
    }
}
