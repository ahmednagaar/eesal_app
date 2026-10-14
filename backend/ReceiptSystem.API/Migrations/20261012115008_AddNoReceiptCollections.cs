using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNoReceiptCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NoReceiptCollections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    ImportRowId = table.Column<int>(type: "int", nullable: true),
                    DriverId = table.Column<int>(type: "int", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: true),
                    MerchantNameRaw = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoReceiptCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoReceiptCollections_CollectionSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "CollectionSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoReceiptCollections_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoReceiptCollections_ExcelImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ExcelImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoReceiptCollections_ExcelImportRows_ImportRowId",
                        column: x => x.ImportRowId,
                        principalTable: "ExcelImportRows",
                        principalColumn: "RowId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoReceiptCollections_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoReceiptCollections_Users_EnteredByUserId",
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
                value: "$2a$11$oRVU6A5z2QehQyraiOUqg.zAQMB8XCzRr05wL59QEIrKdIHjiW6d2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$1s104CSLjnh.Qn64rGj3sOcPZwA0TU0bddVA.eDR9SoTchaaNXbtW");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$jFPre09LSRpag/RcPCBA8OlXT8F/cc58qv1Z19260apH63UaMpOvm");

            migrationBuilder.CreateIndex(
                name: "IX_NoReceiptCollections_BatchId",
                table: "NoReceiptCollections",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_NoReceiptCollections_DriverId",
                table: "NoReceiptCollections",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_NoReceiptCollections_EnteredByUserId",
                table: "NoReceiptCollections",
                column: "EnteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NoReceiptCollections_ImportRowId",
                table: "NoReceiptCollections",
                column: "ImportRowId");

            migrationBuilder.CreateIndex(
                name: "IX_NoReceiptCollections_MerchantId",
                table: "NoReceiptCollections",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_NoReceiptCollections_SessionId",
                table: "NoReceiptCollections",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoReceiptCollections");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$NSpxcgAhqLWNosA3rsd7EeSclGb8oaG2G9lsyV3m.HfIFuBS1y4bm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$QDjPLv3shyEVqbDjneJkGeYGtp36fU2h09660phNddwp47N/x8riq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$T5GADPlnkZi19/xzfAoYw.Q8wKQehP7PmUd50UU9ra8XchbsMJGm6");
        }
    }
}
