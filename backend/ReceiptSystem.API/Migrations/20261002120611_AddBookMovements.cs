using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBookMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookId",
                table: "ReceiptGaps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonCategory",
                table: "ReceiptGaps",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookMovements",
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
                    table.PrimaryKey("PK_BookMovements", x => x.MovementId);
                    table.ForeignKey(
                        name: "FK_BookMovements_Drivers_FromDriverId",
                        column: x => x.FromDriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookMovements_Drivers_ToDriverId",
                        column: x => x.ToDriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookMovements_ReceiptBooks_BookId",
                        column: x => x.BookId,
                        principalTable: "ReceiptBooks",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookMovements_Users_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ReceiptGaps",
                keyColumn: "GapId",
                keyValue: 1,
                columns: new[] { "BookId", "ReasonCategory" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$QZoGf3mqBy98p.kKCrfcde3lQHMCLDgccn2Mtp6kb3.qmaGvrTeqq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$su8MSc4LKOpzKs/q18vee.GVZrT3YrVeF0IUwFuzghjlKcTQhbKeu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$5Aacu6lZ.MLeXcPgD8aGx.HusHnBmA1OUlYDYXbbhiuBV5aZjI9QO");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptGaps_BookId",
                table: "ReceiptGaps",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookMovements_BookId",
                table: "BookMovements",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookMovements_FromDriverId",
                table: "BookMovements",
                column: "FromDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_BookMovements_PerformedByUserId",
                table: "BookMovements",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookMovements_ToDriverId",
                table: "BookMovements",
                column: "ToDriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptGaps_ReceiptBooks_BookId",
                table: "ReceiptGaps",
                column: "BookId",
                principalTable: "ReceiptBooks",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptGaps_ReceiptBooks_BookId",
                table: "ReceiptGaps");

            migrationBuilder.DropTable(
                name: "BookMovements");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptGaps_BookId",
                table: "ReceiptGaps");

            migrationBuilder.DropColumn(
                name: "BookId",
                table: "ReceiptGaps");

            migrationBuilder.DropColumn(
                name: "ReasonCategory",
                table: "ReceiptGaps");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$B3bxOdPDuNNKN9doGjh.q.ybUT8RDvMfAk55CY9DQOWL2IKgx3862");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$UH9BSr0/IK1I8tVvsNAkBe9.xc.g0fLtq6hQ34Qcm69MuRfaQTZqu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$LxXONltvkIGVAwgEujS2uObTSo/LQaMVVxboTjoywxIRKkt5mghuS");
        }
    }
}
