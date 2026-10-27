using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnBooksAndGoodsReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UnitName",
                table: "UnitsOfMeasure",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "UnitsOfMeasure",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ReturnReasonCategories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ReasonNameEn",
                table: "ReturnReasonCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReasonName",
                table: "ReturnReasonCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "Products",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ProductCategories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ReturnBookSeries",
                columns: table => new
                {
                    SeriesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeriesCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalBooks = table.Column<int>(type: "int", nullable: false),
                    ReceiptsPerBook = table.Column<int>(type: "int", nullable: false),
                    StartReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    EndReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnBookSeries", x => x.SeriesId);
                    table.ForeignKey(
                        name: "FK_ReturnBookSeries_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnReceiptBooks",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeriesId = table.Column<int>(type: "int", nullable: true),
                    BookNumber = table.Column<int>(type: "int", nullable: false),
                    StartReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    EndReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    AssignedToDriverId = table.Column<int>(type: "int", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ReturnedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnedToUserId = table.Column<int>(type: "int", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnReceiptBooks", x => x.BookId);
                    table.ForeignKey(
                        name: "FK_ReturnReceiptBooks_Drivers_AssignedToDriverId",
                        column: x => x.AssignedToDriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnReceiptBooks_ReturnBookSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "ReturnBookSeries",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnReceiptBooks_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnReceiptBooks_Users_ReturnedToUserId",
                        column: x => x.ReturnedToUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnReceiptBooks_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReturns",
                columns: table => new
                {
                    ReturnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: true),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    ReasonId = table.Column<int>(type: "int", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Open"),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReturns", x => x.ReturnId);
                    table.ForeignKey(
                        name: "FK_GoodsReturns_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReturns_ReturnReasonCategories_ReasonId",
                        column: x => x.ReasonId,
                        principalTable: "ReturnReasonCategories",
                        principalColumn: "ReasonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReturns_ReturnReceiptBooks_BookId",
                        column: x => x.BookId,
                        principalTable: "ReturnReceiptBooks",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReturns_Users_EnteredByUserId",
                        column: x => x.EnteredByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnBookMovements",
                columns: table => new
                {
                    MovementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FromDriverId = table.Column<int>(type: "int", nullable: true),
                    ToDriverId = table.Column<int>(type: "int", nullable: true),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnBookMovements", x => x.MovementId);
                    table.ForeignKey(
                        name: "FK_ReturnBookMovements_Drivers_FromDriverId",
                        column: x => x.FromDriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnBookMovements_Drivers_ToDriverId",
                        column: x => x.ToDriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnBookMovements_ReturnReceiptBooks_BookId",
                        column: x => x.BookId,
                        principalTable: "ReturnReceiptBooks",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnBookMovements_Users_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReturnItems",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductUnitId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReturnItems", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_GoodsReturnItems_GoodsReturns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "GoodsReturns",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodsReturnItems_ProductUnits_ProductUnitId",
                        column: x => x.ProductUnitId,
                        principalTable: "ProductUnits",
                        principalColumn: "ProductUnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReturnItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnItems_ProductId",
                table: "GoodsReturnItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnItems_ProductUnitId",
                table: "GoodsReturnItems",
                column: "ProductUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnItems_ReturnId",
                table: "GoodsReturnItems",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturns_BookId",
                table: "GoodsReturns",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturns_DriverId",
                table: "GoodsReturns",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturns_EnteredByUserId",
                table: "GoodsReturns",
                column: "EnteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturns_ReasonId",
                table: "GoodsReturns",
                column: "ReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnBookMovements_BookId",
                table: "ReturnBookMovements",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnBookMovements_FromDriverId",
                table: "ReturnBookMovements",
                column: "FromDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnBookMovements_PerformedByUserId",
                table: "ReturnBookMovements",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnBookMovements_ToDriverId",
                table: "ReturnBookMovements",
                column: "ToDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnBookSeries_CreatedByUserId",
                table: "ReturnBookSeries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnBookSeries_SeriesCode",
                table: "ReturnBookSeries",
                column: "SeriesCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReturnReceiptBooks_AssignedByUserId",
                table: "ReturnReceiptBooks",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnReceiptBooks_AssignedToDriverId",
                table: "ReturnReceiptBooks",
                column: "AssignedToDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnReceiptBooks_ReturnedToUserId",
                table: "ReturnReceiptBooks",
                column: "ReturnedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnReceiptBooks_SeriesId_BookNumber",
                table: "ReturnReceiptBooks",
                columns: new[] { "SeriesId", "BookNumber" },
                unique: true,
                filter: "[SeriesId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnReceiptBooks_VerifiedByUserId",
                table: "ReturnReceiptBooks",
                column: "VerifiedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoodsReturnItems");

            migrationBuilder.DropTable(
                name: "ReturnBookMovements");

            migrationBuilder.DropTable(
                name: "GoodsReturns");

            migrationBuilder.DropTable(
                name: "ReturnReceiptBooks");

            migrationBuilder.DropTable(
                name: "ReturnBookSeries");

            migrationBuilder.AlterColumn<string>(
                name: "UnitName",
                table: "UnitsOfMeasure",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "UnitsOfMeasure",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ReturnReasonCategories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ReasonNameEn",
                table: "ReturnReasonCategories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReasonName",
                table: "ReturnReasonCategories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ProductCategories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$NMcgJUMuyi7.yCqQaQAWQOVwlytpCQfK/iTUDwuRqQ8BVEsPXfnNS");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$LPmN.lG5Wd16kv1WksShaOuVfnET5eBNjxI8ziDTTk4QXjH2SZNK6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$ZxnsaGonZEoFARe/b4vEhu9O1zzx7xMsOh3OOhFb1TkzCDjlMgbKy");
        }
    }
}
