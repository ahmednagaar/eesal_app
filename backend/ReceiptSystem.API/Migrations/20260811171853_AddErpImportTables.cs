using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddErpImportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExcelImportBatches",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UploadedFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: false),
                    TotalRowsCount = table.Column<int>(type: "int", nullable: false),
                    AssignedRowsCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "InProgress"),
                    LedgerTotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActualCashTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelImportBatches", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_ExcelImportBatches_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExcelImportRows",
                columns: table => new
                {
                    RowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    MerchantNameRaw = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MatchedMerchantId = table.Column<int>(type: "int", nullable: true),
                    IsNewMerchant = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAssigned = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AssignedReceiptId = table.Column<int>(type: "int", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelImportRows", x => x.RowId);
                    table.ForeignKey(
                        name: "FK_ExcelImportRows_ExcelImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ExcelImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExcelImportRows_Merchants_MatchedMerchantId",
                        column: x => x.MatchedMerchantId,
                        principalTable: "Merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExcelImportRows_Receipts_AssignedReceiptId",
                        column: x => x.AssignedReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "ReceiptId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$uoAo7f6sWexNEowtyT5o.OzJIh8sotcZFqaPrsy2tuw1tLbQiPu4m");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$aQB55bF/6SAuqWyO5ksABuVxQ6Q1JKIOZoFF.gTICwg3PBoluhbEC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$JLZz8PEz.VUYBML7i/scjesC6H9PLoN.WSsV84H36vKTjhrAxm6FG");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelImportBatches_UploadedByUserId",
                table: "ExcelImportBatches",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelImportRows_AssignedReceiptId",
                table: "ExcelImportRows",
                column: "AssignedReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelImportRows_BatchId",
                table: "ExcelImportRows",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelImportRows_MatchedMerchantId",
                table: "ExcelImportRows",
                column: "MatchedMerchantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExcelImportRows");

            migrationBuilder.DropTable(
                name: "ExcelImportBatches");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$y82cMINXlb/qrSzw0RUDh.Y4jD5hgTjG0rxegp43xT7k9z0pFydu.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$LHt2CEELh3UlIdicrJpvJuVQ7vhZkNMNlri/HvudMalVLj4u4180C");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$cP49O0RO1RinHGMPn10rwu8/DHOLS9t0QhcCn0vOGu8nrnzXb4/42");
        }
    }
}
