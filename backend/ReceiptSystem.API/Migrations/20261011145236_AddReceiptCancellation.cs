using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptCancellation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DetectedInSessionId",
                table: "ReceiptGaps",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "ReceiptGaps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledByReason",
                table: "ReceiptGaps",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancelledByUserId",
                table: "ReceiptGaps",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ReceiptGaps",
                keyColumn: "GapId",
                keyValue: 1,
                columns: new[] { "CancelledAt", "CancelledByReason", "CancelledByUserId" },
                values: new object[] { null, null, null });

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

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ReceiptNumber",
                table: "Receipts",
                column: "ReceiptNumber",
                unique: true,
                filter: "[SeriesId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptGaps_CancelledByUserId",
                table: "ReceiptGaps",
                column: "CancelledByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptGaps_Users_CancelledByUserId",
                table: "ReceiptGaps",
                column: "CancelledByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptGaps_Users_CancelledByUserId",
                table: "ReceiptGaps");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_ReceiptNumber",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptGaps_CancelledByUserId",
                table: "ReceiptGaps");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "ReceiptGaps");

            migrationBuilder.DropColumn(
                name: "CancelledByReason",
                table: "ReceiptGaps");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "ReceiptGaps");

            migrationBuilder.AlterColumn<int>(
                name: "DetectedInSessionId",
                table: "ReceiptGaps",
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
                value: "$2a$11$quJUEgm2gUdSNIglxmKyR.V0FE2g9TUB/n4TluXADcPSNxwMldWfC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$AoT./fgMLgjCd5JKduZ3U.t.JVGo8qdbWaET4XGvz0Qk1/HuxsdHG");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$VeIZfPu0BOvz3.RH9RmHN.utXTDssswToNsybAT10hnPzvnQoZ2Zu");
        }
    }
}
