using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddGapBookIdAndReasonCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
