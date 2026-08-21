using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionConfirmAndReconcile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ActualCashCounted",
                table: "CollectionSessions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CashDifference",
                table: "CollectionSessions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CashReconciled",
                table: "CollectionSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "CollectionSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfirmedByUserId",
                table: "CollectionSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "CollectionSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReconciledAt",
                table: "CollectionSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReconciledByUserId",
                table: "CollectionSessions",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CollectionSessions",
                keyColumn: "SessionId",
                keyValue: 1,
                columns: new[] { "ActualCashCounted", "CashDifference", "ConfirmedAt", "ConfirmedByUserId", "ReconciledAt", "ReconciledByUserId" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "CollectionSessions",
                keyColumn: "SessionId",
                keyValue: 2,
                columns: new[] { "ActualCashCounted", "CashDifference", "ConfirmedAt", "ConfirmedByUserId", "ReconciledAt", "ReconciledByUserId" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "CollectionSessions",
                keyColumn: "SessionId",
                keyValue: 3,
                columns: new[] { "ActualCashCounted", "CashDifference", "ConfirmedAt", "ConfirmedByUserId", "ReconciledAt", "ReconciledByUserId" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "CollectionSessions",
                keyColumn: "SessionId",
                keyValue: 4,
                columns: new[] { "ActualCashCounted", "CashDifference", "ConfirmedAt", "ConfirmedByUserId", "ReconciledAt", "ReconciledByUserId" },
                values: new object[] { null, null, null, null, null, null });

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

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "FullName", "IsActive", "LastLoginAt", "PasswordHash", "Role", "Username" },
                values: new object[] { 3, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), "موظف الكول سنتر", true, null, "$2a$11$cP49O0RO1RinHGMPn10rwu8/DHOLS9t0QhcCn0vOGu8nrnzXb4/42", "CallCenter", "callcenter" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSessions_ConfirmedByUserId",
                table: "CollectionSessions",
                column: "ConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSessions_ReconciledByUserId",
                table: "CollectionSessions",
                column: "ReconciledByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionSessions_Users_ConfirmedByUserId",
                table: "CollectionSessions",
                column: "ConfirmedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionSessions_Users_ReconciledByUserId",
                table: "CollectionSessions",
                column: "ReconciledByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionSessions_Users_ConfirmedByUserId",
                table: "CollectionSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionSessions_Users_ReconciledByUserId",
                table: "CollectionSessions");

            migrationBuilder.DropIndex(
                name: "IX_CollectionSessions_ConfirmedByUserId",
                table: "CollectionSessions");

            migrationBuilder.DropIndex(
                name: "IX_CollectionSessions_ReconciledByUserId",
                table: "CollectionSessions");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "ActualCashCounted",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "CashDifference",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "CashReconciled",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "ConfirmedByUserId",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "ReconciledAt",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "ReconciledByUserId",
                table: "CollectionSessions");

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
        }
    }
}
