using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMustChangePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "MustChangePassword", "PasswordHash" },
                values: new object[] { true, "$2a$11$HMJi6oCnkdy9ENg9d8P2T.VIxDWl3p3P4FfARjwa.FS.8pilfbxSq" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "MustChangePassword", "PasswordHash" },
                values: new object[] { true, "$2a$11$z.XixqtlWbVYr7oatl7VHuJKrGWvTe2EfoCL1AIyTOjP.qoPJA6Wi" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "MustChangePassword", "PasswordHash" },
                values: new object[] { true, "$2a$11$IlmNxenhjGL3Sfu4VsOO..Tsr6Y2MTwyNSojRgXAJ5bTrL9sxoOeK" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "Users");

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
        }
    }
}
