using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddModule2RoutesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    RouteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.RouteId);
                    table.ForeignKey(
                        name: "FK_Routes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryDays",
                columns: table => new
                {
                    DeliveryDayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedDriver = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PrintedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryDays", x => x.DeliveryDayId);
                    table.ForeignKey(
                        name: "FK_DeliveryDays_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "RouteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryDays_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RouteMerchants",
                columns: table => new
                {
                    RouteMerchantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    PositionOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AddedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteMerchants", x => x.RouteMerchantId);
                    table.ForeignKey(
                        name: "FK_RouteMerchants_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteMerchants_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "RouteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteMerchants_Users_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DayInvoices",
                columns: table => new
                {
                    DayInvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryDayId = table.Column<int>(type: "int", nullable: false),
                    RouteMerchantId = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ManualPositionOverride = table.Column<int>(type: "int", nullable: true),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayInvoices", x => x.DayInvoiceId);
                    table.ForeignKey(
                        name: "FK_DayInvoices_DeliveryDays_DeliveryDayId",
                        column: x => x.DeliveryDayId,
                        principalTable: "DeliveryDays",
                        principalColumn: "DeliveryDayId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DayInvoices_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DayInvoices_RouteMerchants_RouteMerchantId",
                        column: x => x.RouteMerchantId,
                        principalTable: "RouteMerchants",
                        principalColumn: "RouteMerchantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DayInvoices_Users_EnteredByUserId",
                        column: x => x.EnteredByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Routes",
                columns: new[] { "RouteId", "CreatedAt", "CreatedByUserId", "IsActive", "Notes", "RouteName" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, null, "المنشية" },
                    { 2, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, null, "خريط" },
                    { 3, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, null, "أسوان" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$9kHh1AGGxn.2xxqeA6dw1OIurnp57L08RWTcG1QYyJyR2Y/RjeHIe");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$wc.IdPgdLwGZlubDimpKpeY9jOV09hs6YwUpQu/wPEn0xZq1DgeOO");

            migrationBuilder.InsertData(
                table: "DeliveryDays",
                columns: new[] { "DeliveryDayId", "AssignedDriver", "ConfirmedAt", "CreatedAt", "CreatedByUserId", "DeliveryDate", "Notes", "PrintedAt", "RouteId", "Status" },
                values: new object[] { 1, "أحمد محمود حسن", new DateTime(2026, 6, 21, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 1, "Confirmed" });

            migrationBuilder.InsertData(
                table: "RouteMerchants",
                columns: new[] { "RouteMerchantId", "AddedAt", "AddedByUserId", "IsActive", "MerchantId", "Notes", "PositionOrder", "RouteId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 1, null, 1, 1 },
                    { 2, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 2, null, 2, 1 },
                    { 3, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 3, null, 3, 1 },
                    { 4, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 4, null, 4, 1 },
                    { 5, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 5, null, 5, 1 },
                    { 6, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 6, null, 6, 1 },
                    { 7, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 7, null, 7, 1 },
                    { 8, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 8, null, 8, 1 },
                    { 9, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 9, null, 9, 1 },
                    { 10, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 10, null, 10, 1 },
                    { 11, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 11, null, 11, 1 },
                    { 12, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 12, null, 12, 1 },
                    { 13, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 13, null, 13, 1 },
                    { 14, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 14, null, 14, 1 },
                    { 15, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 15, null, 15, 1 },
                    { 16, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 1, null, 1, 2 },
                    { 17, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 2, null, 2, 2 },
                    { 18, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 3, null, 3, 2 },
                    { 19, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 4, null, 4, 2 },
                    { 20, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 5, null, 5, 2 },
                    { 21, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 6, null, 6, 2 },
                    { 22, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 7, null, 7, 2 },
                    { 23, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 8, null, 8, 2 },
                    { 24, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 9, null, 9, 2 },
                    { 25, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 10, null, 10, 2 },
                    { 26, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 1, null, 1, 3 },
                    { 27, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 2, null, 2, 3 },
                    { 28, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 3, null, 3, 3 },
                    { 29, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 4, null, 4, 3 },
                    { 30, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 5, null, 5, 3 },
                    { 31, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 6, null, 6, 3 },
                    { 32, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 7, null, 7, 3 },
                    { 33, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 8, null, 8, 3 },
                    { 34, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 9, null, 9, 3 },
                    { 35, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 10, null, 10, 3 },
                    { 36, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 11, null, 11, 3 },
                    { 37, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 12, null, 12, 3 }
                });

            migrationBuilder.InsertData(
                table: "DayInvoices",
                columns: new[] { "DayInvoiceId", "Amount", "DeliveryDayId", "EnteredAt", "EnteredByUserId", "InvoiceNumber", "ManualPositionOverride", "MerchantId", "Notes", "Quantity", "RouteMerchantId" },
                values: new object[,]
                {
                    { 1, 1750m, 1, new DateTime(2026, 6, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "INV-12371", null, 1, null, "2 كراتين", 1 },
                    { 2, 2000m, 1, new DateTime(2026, 6, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "INV-12372", null, 2, null, "3 كراتين", 2 },
                    { 3, 2250m, 1, new DateTime(2026, 6, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "INV-12373", null, 3, null, "4 كراتين", 3 },
                    { 4, 2500m, 1, new DateTime(2026, 6, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "INV-12374", null, 4, null, "5 كراتين", 4 },
                    { 5, 2750m, 1, new DateTime(2026, 6, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "INV-12375", null, 5, null, "6 كراتين", 5 },
                    { 6, 3000m, 1, new DateTime(2026, 6, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "INV-12376", null, 6, null, "7 كراتين", 6 },
                    { 7, 3250m, 1, new DateTime(2026, 6, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "INV-12377", null, 7, null, "8 كراتين", 7 },
                    { 8, 3500m, 1, new DateTime(2026, 6, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "INV-12378", null, 8, null, "9 كراتين", 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DayInvoices_DeliveryDayId",
                table: "DayInvoices",
                column: "DeliveryDayId");

            migrationBuilder.CreateIndex(
                name: "IX_DayInvoices_EnteredByUserId",
                table: "DayInvoices",
                column: "EnteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DayInvoices_MerchantId",
                table: "DayInvoices",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_DayInvoices_RouteMerchantId",
                table: "DayInvoices",
                column: "RouteMerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryDays_CreatedByUserId",
                table: "DeliveryDays",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryDays_RouteId_DeliveryDate",
                table: "DeliveryDays",
                columns: new[] { "RouteId", "DeliveryDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteMerchants_AddedByUserId",
                table: "RouteMerchants",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteMerchants_MerchantId",
                table: "RouteMerchants",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteMerchants_RouteId_MerchantId",
                table: "RouteMerchants",
                columns: new[] { "RouteId", "MerchantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteMerchants_RouteId_PositionOrder",
                table: "RouteMerchants",
                columns: new[] { "RouteId", "PositionOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_CreatedByUserId",
                table: "Routes",
                column: "CreatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DayInvoices");

            migrationBuilder.DropTable(
                name: "DeliveryDays");

            migrationBuilder.DropTable(
                name: "RouteMerchants");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$cYl.KeYR6zG3k8DGGDl/eOcz/8fhKEPjYE.e3K/fVrCNYs/YA/7Ty");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$wYFNiXKbJSwADB/EArQLZOvQH4z0p/cCRmQVOZYW9xoxq4wkmL/Bq");
        }
    }
}
