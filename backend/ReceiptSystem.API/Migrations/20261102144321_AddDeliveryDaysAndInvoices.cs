using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryDaysAndInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryDays",
                columns: table => new
                {
                    DeliveryDayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "date", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$LWjPzXrnfNqw1uOPw/Vd..Nf2Ky7PJkNzeHJV/DSdOtsPKI5f5S/2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$lzCKs/SSeBEmMM5/fVcXrOdNodU2Dp4XA0wtA0SiRWg3sg5YOOPeW");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$9FkzRJ.OBgMOn1F2j7pp.evUc825iiAA7fX79TS05vKb3iF0UbgAa");

            migrationBuilder.CreateIndex(
                name: "IX_DayInvoices_DeliveryDayId_RouteMerchantId",
                table: "DayInvoices",
                columns: new[] { "DeliveryDayId", "RouteMerchantId" },
                unique: true);

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
                columns: new[] { "RouteId", "DeliveryDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DayInvoices");

            migrationBuilder.DropTable(
                name: "DeliveryDays");

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
        }
    }
}
