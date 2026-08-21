using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAjalInvoicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AjalInvoices",
                columns: table => new
                {
                    AjalInvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: true),
                    CallCenterEmployeeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SessionDate = table.Column<DateTime>(type: "date", nullable: false),
                    InvoiceStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ModificationNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImportSource = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Manual"),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AjalInvoices", x => x.AjalInvoiceId);
                    table.ForeignKey(
                        name: "FK_AjalInvoices_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjalInvoices_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "RouteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjalInvoices_Users_EnteredByUserId",
                        column: x => x.EnteredByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    SettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.SettingKey);
                    table.ForeignKey(
                        name: "FK_SystemSettings_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "SettingKey", "Description", "SettingValue", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { "InvoicePrefix", "البادئة الثابتة لأرقام الفواتير", "441", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { "InvoicePrefixWarningThreshold", "أظهر تحذيراً عند وصول رقم الفاتورة الأخير لهذا الحد", "950", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { "InvoiceTotalDigits", "إجمالي عدد أرقام الفاتورة الكاملة", "6", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), null }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$xKkwSJRQ07QsJRA0az4mN./i5R.mhr9ruh.DAQ79M6i.pf5JgZNX.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$U89Hc9Bvzo2QTq9jNAN8DexNlw4aBpV00/x2oxICBG1c0FtD8zeBu");

            migrationBuilder.CreateIndex(
                name: "IX_AjalInvoices_EnteredByUserId",
                table: "AjalInvoices",
                column: "EnteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AjalInvoices_InvoiceNumber",
                table: "AjalInvoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AjalInvoices_MerchantId",
                table: "AjalInvoices",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_AjalInvoices_RouteId",
                table: "AjalInvoices",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_UpdatedByUserId",
                table: "SystemSettings",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AjalInvoices");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$7b/FceE8rrtoJ5PHXt8Xze4J7.R4VIYPi6oN1LcqNMRUZcvcw6wQi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$so6l1FJaVxytjZoxDS6Bruzma2qIs0UqWxGqXjJgqxo.M1.hvnhjC");
        }
    }
}
