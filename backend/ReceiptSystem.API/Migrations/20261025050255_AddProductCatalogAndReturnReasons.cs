using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCatalogAndReturnReasons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_ProductCategories_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnReasonCategories",
                columns: table => new
                {
                    ReasonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReasonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReasonNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsSystemDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnReasonCategories", x => x.ReasonId);
                    table.ForeignKey(
                        name: "FK_ReturnReasonCategories_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitsOfMeasure",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnitAbbreviation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitsOfMeasure", x => x.UnitId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductUnits",
                columns: table => new
                {
                    ProductUnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ConversionToBaseQty = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    BaseUnitId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUnits", x => x.ProductUnitId);
                    table.ForeignKey(
                        name: "FK_ProductUnits_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductUnits_UnitsOfMeasure_BaseUnitId",
                        column: x => x.BaseUnitId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductUnits_UnitsOfMeasure_UnitId",
                        column: x => x.UnitId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ReturnReasonCategories",
                columns: new[] { "ReasonId", "CreatedAt", "CreatedByUserId", "IsActive", "IsSystemDefault", "ReasonName", "ReasonNameEn", "SortOrder" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, true, "مرتجع عميل", "Customer Return", 1 },
                    { 2, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, true, "لم يحمل", "Not Loaded", 2 },
                    { 3, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, true, "تالف أثناء النقل", "Damaged During Transport", 3 },
                    { 4, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, true, "منتج خاطئ", "Wrong Product", 4 },
                    { 5, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, true, "رفض العميل", "Customer Refusal", 5 },
                    { 6, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, true, "إضافة", "Addition / Surplus Sold", 6 },
                    { 7, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, true, "أخرى", "Other", 7 }
                });

            migrationBuilder.InsertData(
                table: "UnitsOfMeasure",
                columns: new[] { "UnitId", "IsActive", "SortOrder", "UnitAbbreviation", "UnitName" },
                values: new object[,]
                {
                    { 1, true, 1, "شيك", "شيكارة" },
                    { 2, true, 2, "كرت", "كرتونة" },
                    { 3, true, 3, "لفة", "لفة" },
                    { 4, true, 4, "كجم", "كيلو" },
                    { 5, true, 5, "علبة", "علبة" },
                    { 6, true, 6, "باكو", "باكو" },
                    { 7, true, 7, "قطعة", "قطعة" }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CategoryName",
                table: "ProductCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CreatedByUserId",
                table: "ProductCategories",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_ProductName",
                table: "Products",
                columns: new[] { "CategoryId", "ProductName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedByUserId",
                table: "Products",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode",
                table: "Products",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_BaseUnitId",
                table: "ProductUnits",
                column: "BaseUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_ProductId_UnitId",
                table: "ProductUnits",
                columns: new[] { "ProductId", "UnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_UnitId",
                table: "ProductUnits",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnReasonCategories_CreatedByUserId",
                table: "ReturnReasonCategories",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnReasonCategories_ReasonName",
                table: "ReturnReasonCategories",
                column: "ReasonName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitsOfMeasure_UnitName",
                table: "UnitsOfMeasure",
                column: "UnitName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductUnits");

            migrationBuilder.DropTable(
                name: "ReturnReasonCategories");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "UnitsOfMeasure");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$HMJi6oCnkdy9ENg9d8P2T.VIxDWl3p3P4FfARjwa.FS.8pilfbxSq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$z.XixqtlWbVYr7oatl7VHuJKrGWvTe2EfoCL1AIyTOjP.qoPJA6Wi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$IlmNxenhjGL3Sfu4VsOO..Tsr6Y2MTwyNSojRgXAJ5bTrL9sxoOeK");
        }
    }
}
