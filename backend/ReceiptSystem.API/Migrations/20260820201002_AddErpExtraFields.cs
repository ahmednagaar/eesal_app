using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddErpExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "ExcelImportRows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpEntryTime",
                table: "ExcelImportRows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpId",
                table: "ExcelImportRows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpInvoiceNumber",
                table: "ExcelImportRows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpUser",
                table: "ExcelImportRows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Treasury",
                table: "ExcelImportRows",
                type: "nvarchar(max)",
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branch",
                table: "ExcelImportRows");

            migrationBuilder.DropColumn(
                name: "ErpEntryTime",
                table: "ExcelImportRows");

            migrationBuilder.DropColumn(
                name: "ErpId",
                table: "ExcelImportRows");

            migrationBuilder.DropColumn(
                name: "ErpInvoiceNumber",
                table: "ExcelImportRows");

            migrationBuilder.DropColumn(
                name: "ErpUser",
                table: "ExcelImportRows");

            migrationBuilder.DropColumn(
                name: "Treasury",
                table: "ExcelImportRows");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$uoAo7f6sWexNEowtyT5o.OzJIh8sotcZFqaPrsy2tuw1tLbQiPu4m");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$aQB55bF/6SAuqWyO5ksABuVxQ6Q1JKIOZoFF.gTICwg3PBoluhbEC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$JLZz8PEz.VUYBML7i/scjesC6H9PLoN.WSsV84H36vKTjhrAxm6FG");
        }
    }
}
