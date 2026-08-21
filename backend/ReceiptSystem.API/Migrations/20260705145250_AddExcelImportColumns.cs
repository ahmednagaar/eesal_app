using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddExcelImportColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "Receipts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "DesktopSystemUser",
                table: "Receipts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportSource",
                table: "Receipts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Manual");

            migrationBuilder.AddColumn<bool>(
                name: "IsWithoutReceipt",
                table: "Receipts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ImportSource",
                table: "CollectionSessions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Manual");

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "CollectionSessions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CollectionSessions",
                keyColumn: "SessionId",
                keyValue: 1,
                columns: new[] { "ImportSource", "OriginalFileName" },
                values: new object[] { "Manual", null });

            migrationBuilder.UpdateData(
                table: "CollectionSessions",
                keyColumn: "SessionId",
                keyValue: 2,
                columns: new[] { "ImportSource", "OriginalFileName" },
                values: new object[] { "Manual", null });

            migrationBuilder.UpdateData(
                table: "CollectionSessions",
                keyColumn: "SessionId",
                keyValue: 3,
                columns: new[] { "ImportSource", "OriginalFileName" },
                values: new object[] { "Manual", null });

            migrationBuilder.UpdateData(
                table: "CollectionSessions",
                keyColumn: "SessionId",
                keyValue: 4,
                columns: new[] { "ImportSource", "OriginalFileName" },
                values: new object[] { "Manual", null });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 1,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 2,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 3,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 4,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 5,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 6,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 7,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 8,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 9,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 10,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 11,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 12,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 13,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 14,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 15,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 16,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 17,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 18,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 19,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 20,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 21,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 22,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 23,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 24,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 25,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 26,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 27,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 28,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 29,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 30,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 31,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 32,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 33,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 34,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 35,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 36,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 37,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 38,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 39,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 40,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 41,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 42,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 43,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 44,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 45,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 46,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 47,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 48,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 49,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 50,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 51,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 52,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 53,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 54,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 55,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 56,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 57,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 58,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

            migrationBuilder.UpdateData(
                table: "Receipts",
                keyColumn: "ReceiptId",
                keyValue: 59,
                columns: new[] { "DesktopSystemUser", "ImportSource" },
                values: new object[] { null, "Manual" });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesktopSystemUser",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "ImportSource",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "IsWithoutReceipt",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "ImportSource",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "CollectionSessions");

            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "Receipts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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
        }
    }
}
