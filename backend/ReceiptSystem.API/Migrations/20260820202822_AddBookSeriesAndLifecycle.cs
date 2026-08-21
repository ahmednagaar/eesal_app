using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBookSeriesAndLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Receipts_ReceiptNumber",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptBooks_BookNumber",
                table: "ReceiptBooks");

            migrationBuilder.AddColumn<int>(
                name: "SeriesId",
                table: "Receipts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "ReceiptBooks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnedDate",
                table: "ReceiptBooks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReturnedToUserId",
                table: "ReceiptBooks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeriesId",
                table: "ReceiptBooks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "ReceiptBooks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerifiedByUserId",
                table: "ReceiptBooks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookSeries",
                columns: table => new
                {
                    SeriesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeriesCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_BookSeries", x => x.SeriesId);
                    table.ForeignKey(
                        name: "FK_BookSeries_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ReceiptBooks",
                keyColumn: "BookId",
                keyValue: 1,
                columns: new[] { "ReturnedDate", "ReturnedToUserId", "SeriesId", "VerifiedAt", "VerifiedByUserId" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ReceiptBooks",
                keyColumn: "BookId",
                keyValue: 2,
                columns: new[] { "ReturnedDate", "ReturnedToUserId", "SeriesId", "VerifiedAt", "VerifiedByUserId" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ReceiptBooks",
                keyColumn: "BookId",
                keyValue: 3,
                columns: new[] { "ReturnedDate", "ReturnedToUserId", "SeriesId", "VerifiedAt", "VerifiedByUserId" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 1,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 2,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 3,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 4,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 5,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 6,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 7,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 8,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 9,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 10,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 11,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 12,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 13,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 14,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 15,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 16,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 17,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 18,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 19,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 20,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 21,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 22,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 23,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 24,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 25,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 26,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 27,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 28,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 29,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 30,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 31,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 32,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 33,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 34,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 35,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 36,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 37,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 38,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 39,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 40,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 41,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 42,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 43,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 44,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 45,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 46,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 47,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 48,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 49,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 50,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 51,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 52,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 53,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 54,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 55,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 56,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 57,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 58,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 59,
                column: "SeriesId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$K84Ca.v0qflwXahvVKrv5OJEpDwdljTeiXxpP7YkSfW59LMRqKPES");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$X4S1KT9CnnMrYBDWBhtLHOOp4NUhn/j/qeizv5FgJ.T4lQ0JNRRmi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$fgEHJMm2wOop12Wq9QIH4.DHmmRWjL2kMmzOfWq5g1VHSZW9Wivje");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_SeriesId_ReceiptNumber",
                table: "Receipts",
                columns: new[] { "SeriesId", "ReceiptNumber" },
                unique: true,
                filter: "[SeriesId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptBooks_ReturnedToUserId",
                table: "ReceiptBooks",
                column: "ReturnedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptBooks_SeriesId_BookNumber",
                table: "ReceiptBooks",
                columns: new[] { "SeriesId", "BookNumber" },
                unique: true,
                filter: "[SeriesId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptBooks_VerifiedByUserId",
                table: "ReceiptBooks",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookSeries_CreatedByUserId",
                table: "BookSeries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookSeries_SeriesCode",
                table: "BookSeries",
                column: "SeriesCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptBooks_BookSeries_SeriesId",
                table: "ReceiptBooks",
                column: "SeriesId",
                principalTable: "BookSeries",
                principalColumn: "SeriesId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptBooks_Users_ReturnedToUserId",
                table: "ReceiptBooks",
                column: "ReturnedToUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptBooks_Users_VerifiedByUserId",
                table: "ReceiptBooks",
                column: "VerifiedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_BookSeries_SeriesId",
                table: "Receipts",
                column: "SeriesId",
                principalTable: "BookSeries",
                principalColumn: "SeriesId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptBooks_BookSeries_SeriesId",
                table: "ReceiptBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptBooks_Users_ReturnedToUserId",
                table: "ReceiptBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptBooks_Users_VerifiedByUserId",
                table: "ReceiptBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_BookSeries_SeriesId",
                table: "Receipts");

            migrationBuilder.DropTable(
                name: "BookSeries");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_SeriesId_ReceiptNumber",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptBooks_ReturnedToUserId",
                table: "ReceiptBooks");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptBooks_SeriesId_BookNumber",
                table: "ReceiptBooks");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptBooks_VerifiedByUserId",
                table: "ReceiptBooks");

            migrationBuilder.DropColumn(
                name: "SeriesId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "ReceiptBooks");

            migrationBuilder.DropColumn(
                name: "ReturnedDate",
                table: "ReceiptBooks");

            migrationBuilder.DropColumn(
                name: "ReturnedToUserId",
                table: "ReceiptBooks");

            migrationBuilder.DropColumn(
                name: "SeriesId",
                table: "ReceiptBooks");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "ReceiptBooks");

            migrationBuilder.DropColumn(
                name: "VerifiedByUserId",
                table: "ReceiptBooks");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$jaJFMcZZIBhhP5B4ZLxO9erQgu9Vgkp7keLk9gxHvDobXyUblR8Xi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$TcLWRfpZ13laizpaQ5AhcOGtXhLI2rL9El2PZsp82.1R2V9WfGDnW");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$9Oux/SMOCYuCicmF9Mr4k.UND3e5g8rlkLsnBJutbQHVtftA43256");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ReceiptNumber",
                table: "Receipts",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptBooks_BookNumber",
                table: "ReceiptBooks",
                column: "BookNumber",
                unique: true);
        }
    }
}
