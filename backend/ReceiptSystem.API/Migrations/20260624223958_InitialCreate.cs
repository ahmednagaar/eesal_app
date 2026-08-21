using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReceiptSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Merchants",
                columns: table => new
                {
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchants", x => x.MerchantId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Treasury"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    DriverId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.DriverId);
                    table.ForeignKey(
                        name: "FK_Drivers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionSessions",
                columns: table => new
                {
                    SessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    SessionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RouteArea = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FirstReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    LastReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    TotalReceiptsCount = table.Column<int>(type: "int", nullable: false),
                    TotalAmountCollected = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HasGaps = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_CollectionSessions_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionSessions_Users_EnteredByUserId",
                        column: x => x.EnteredByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptBooks",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookNumber = table.Column<int>(type: "int", nullable: false),
                    StartReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    EndReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    AssignedToDriverId = table.Column<int>(type: "int", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptBooks", x => x.BookId);
                    table.ForeignKey(
                        name: "FK_ReceiptBooks_Drivers_AssignedToDriverId",
                        column: x => x.AssignedToDriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptBooks_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptGaps",
                columns: table => new
                {
                    GapId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissingReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    DetectedInSessionId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Open"),
                    Resolution = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptGaps", x => x.GapId);
                    table.ForeignKey(
                        name: "FK_ReceiptGaps_CollectionSessions_DetectedInSessionId",
                        column: x => x.DetectedInSessionId,
                        principalTable: "CollectionSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptGaps_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptGaps_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    ReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPartialPayment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.ReceiptId);
                    table.ForeignKey(
                        name: "FK_Receipts_CollectionSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "CollectionSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_ReceiptBooks_BookId",
                        column: x => x.BookId,
                        principalTable: "ReceiptBooks",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_Users_EnteredByUserId",
                        column: x => x.EnteredByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Merchants",
                columns: new[] { "MerchantId", "City", "CreatedAt", "IsActive", "MerchantName", "Notes", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, "كومومبو", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "محلات النور للتجارة", null, "01011111111" },
                    { 2, "أسوان", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "شركة الأمل للتوزيع", null, "01022222222" },
                    { 3, "إدفو", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "مؤسسة البركة", null, "01033333333" },
                    { 4, "كومومبو", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "محلات الحرمين", null, "01044444444" },
                    { 5, "دراو", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "تجارة السلام", null, "01055555555" },
                    { 6, "أسوان", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "محلات الفتح", null, "01066666666" },
                    { 7, "إدفو", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "شركة الوفاء للمواد الغذائية", null, "01077777777" },
                    { 8, "كومومبو", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "مؤسسة الخير", null, "01088888888" },
                    { 9, "نصر النوبة", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "محلات الإخوة", null, "01099999999" },
                    { 10, "أسوان", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "تجارة الصفا", null, "01111111111" },
                    { 11, "دراو", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "محلات الياسمين", null, "01122222222" },
                    { 12, "إدفو", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "شركة النجاح", null, "01133333333" },
                    { 13, "كومومبو", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "مؤسسة الحمد", null, "01144444444" },
                    { 14, "أسوان", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "محلات المدينة", null, "01155555555" },
                    { 15, "دراو", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "تجارة الرحمة", null, "01166666666" },
                    { 16, "نصر النوبة", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "محلات الأندلس", null, "01177777777" },
                    { 17, "أسوان", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "شركة التوحيد للتجارة", null, "01188888888" },
                    { 18, "إدفو", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "محلات الزهراء", null, "01199999999" },
                    { 19, "كومومبو", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "مؤسسة العروبة", null, "01200000000" },
                    { 20, "أسوان", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), true, "محلات السندباد", null, "01211111111" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "FullName", "IsActive", "LastLoginAt", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), "مدير النظام", true, null, "$2a$11$cYl.KeYR6zG3k8DGGDl/eOcz/8fhKEPjYE.e3K/fVrCNYs/YA/7Ty", "Admin", "admin" },
                    { 2, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), "أمين الخزينة", true, null, "$2a$11$wYFNiXKbJSwADB/EArQLZOvQH4z0p/cCRmQVOZYW9xoxq4wkmL/Bq", "Treasury", "خزينة" }
                });

            migrationBuilder.InsertData(
                table: "Drivers",
                columns: new[] { "DriverId", "CreatedAt", "CreatedByUserId", "FullName", "IsActive", "Notes", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "أحمد محمود حسن", true, null, "01012345678" },
                    { 2, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "محمد عبدالله سالم", true, null, "01123456789" },
                    { 3, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "خالد إبراهيم أحمد", true, null, "01234567890" },
                    { 4, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "عمر حسين علي", true, null, "01098765432" },
                    { 5, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "يوسف سعيد محمد", true, null, "01187654321" }
                });

            migrationBuilder.InsertData(
                table: "CollectionSessions",
                columns: new[] { "SessionId", "DriverId", "EnteredAt", "EnteredByUserId", "FirstReceiptNumber", "LastReceiptNumber", "Notes", "RouteArea", "SessionDate", "TotalAmountCollected", "TotalReceiptsCount" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 1, 15, null, "مدينة كومومبو", new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 45000m, 15 },
                    { 2, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 16, 35, null, "مدينة إدفو", new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 62000m, 20 }
                });

            migrationBuilder.InsertData(
                table: "CollectionSessions",
                columns: new[] { "SessionId", "DriverId", "EnteredAt", "EnteredByUserId", "FirstReceiptNumber", "HasGaps", "LastReceiptNumber", "Notes", "RouteArea", "SessionDate", "TotalAmountCollected", "TotalReceiptsCount" },
                values: new object[] { 3, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 36, true, 50, null, "مدينة دراو", new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 38500m, 14 });

            migrationBuilder.InsertData(
                table: "CollectionSessions",
                columns: new[] { "SessionId", "DriverId", "EnteredAt", "EnteredByUserId", "FirstReceiptNumber", "LastReceiptNumber", "Notes", "RouteArea", "SessionDate", "TotalAmountCollected", "TotalReceiptsCount" },
                values: new object[] { 4, 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, 51, 60, null, "مدينة أسوان", new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 28000m, 10 });

            migrationBuilder.InsertData(
                table: "ReceiptBooks",
                columns: new[] { "BookId", "AssignedByUserId", "AssignedDate", "AssignedToDriverId", "BookNumber", "CompletedDate", "CreatedAt", "EndReceiptNumber", "Notes", "StartReceiptNumber", "Status" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, null, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 50, null, 1, "InProgress" },
                    { 2, 1, new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 2, null, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 100, null, 51, "Assigned" },
                    { 3, 1, new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 3, null, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 150, null, 101, "Assigned" }
                });

            migrationBuilder.InsertData(
                table: "ReceiptGaps",
                columns: new[] { "GapId", "DetectedAt", "DetectedInSessionId", "DriverId", "MissingReceiptNumber", "Resolution", "ResolvedAt", "ResolvedByUserId", "Status" },
                values: new object[] { 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 3, 1, 47, null, null, null, "Open" });

            migrationBuilder.InsertData(
                table: "Receipts",
                columns: new[] { "ReceiptId", "Amount", "BookId", "CollectionDate", "DriverId", "EnteredAt", "EnteredByUserId", "MerchantId", "Notes", "ReceiptNumber", "SessionId" },
                values: new object[,]
                {
                    { 1, 3100m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 1, null, 1, 1 },
                    { 2, 3200m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 2, null, 2, 1 },
                    { 3, 3300m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 3, null, 3, 1 },
                    { 4, 3400m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 4, null, 4, 1 },
                    { 5, 3500m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 5, null, 5, 1 },
                    { 6, 3600m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 6, null, 6, 1 },
                    { 7, 3700m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 7, null, 7, 1 },
                    { 8, 3800m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 8, null, 8, 1 },
                    { 9, 3900m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 9, null, 9, 1 },
                    { 10, 4000m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 10, null, 10, 1 },
                    { 11, 4100m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 11, null, 11, 1 },
                    { 12, 4200m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 12, null, 12, 1 },
                    { 13, 4300m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 13, null, 13, 1 },
                    { 14, 4400m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 14, null, 14, 1 },
                    { 15, 4500m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 15, null, 15, 1 },
                    { 16, 3300m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 16, null, 16, 2 },
                    { 17, 3350m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 17, null, 17, 2 },
                    { 18, 3400m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 18, null, 18, 2 },
                    { 19, 3450m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 19, null, 19, 2 },
                    { 20, 3500m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 20, null, 20, 2 },
                    { 21, 3550m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 1, null, 21, 2 },
                    { 22, 3600m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 2, null, 22, 2 },
                    { 23, 3650m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 3, null, 23, 2 },
                    { 24, 3700m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 4, null, 24, 2 },
                    { 25, 3750m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 5, null, 25, 2 },
                    { 26, 3800m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 6, null, 26, 2 },
                    { 27, 3850m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 7, null, 27, 2 },
                    { 28, 3900m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 8, null, 28, 2 },
                    { 29, 3950m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 9, null, 29, 2 },
                    { 30, 4000m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 10, null, 30, 2 },
                    { 31, 4050m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 11, null, 31, 2 },
                    { 32, 4100m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 12, null, 32, 2 },
                    { 33, 4150m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 13, null, 33, 2 },
                    { 34, 4200m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 14, null, 34, 2 },
                    { 35, 4250m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 15, null, 35, 2 },
                    { 36, 4700m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 16, null, 36, 3 },
                    { 37, 4775m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 17, null, 37, 3 },
                    { 38, 4850m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 18, null, 38, 3 },
                    { 39, 4925m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 19, null, 39, 3 },
                    { 40, 5000m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 20, null, 40, 3 },
                    { 41, 5075m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 1, null, 41, 3 },
                    { 42, 5150m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 2, null, 42, 3 },
                    { 43, 5225m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 3, null, 43, 3 },
                    { 44, 5300m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 4, null, 44, 3 },
                    { 45, 5375m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 5, null, 45, 3 },
                    { 46, 5450m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 6, null, 46, 3 },
                    { 47, 5600m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 8, null, 48, 3 },
                    { 48, 5675m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 9, null, 49, 3 },
                    { 49, 5750m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 10, null, 50, 3 },
                    { 50, 5350m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, 11, null, 51, 4 },
                    { 51, 5400m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, 12, null, 52, 4 },
                    { 52, 5450m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, 13, null, 53, 4 },
                    { 53, 5500m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, 14, null, 54, 4 },
                    { 54, 5550m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, 15, null, 55, 4 },
                    { 55, 5600m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, 16, null, 56, 4 },
                    { 56, 5650m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, 17, null, 57, 4 },
                    { 57, 5700m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, 18, null, 58, 4 },
                    { 58, 5750m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, 19, null, 59, 4 },
                    { 59, 5800m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, 20, null, 60, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSessions_DriverId",
                table: "CollectionSessions",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSessions_EnteredByUserId",
                table: "CollectionSessions",
                column: "EnteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CreatedByUserId",
                table: "Drivers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_MerchantName",
                table: "Merchants",
                column: "MerchantName");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptBooks_AssignedByUserId",
                table: "ReceiptBooks",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptBooks_AssignedToDriverId",
                table: "ReceiptBooks",
                column: "AssignedToDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptBooks_BookNumber",
                table: "ReceiptBooks",
                column: "BookNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptGaps_DetectedInSessionId",
                table: "ReceiptGaps",
                column: "DetectedInSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptGaps_DriverId",
                table: "ReceiptGaps",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptGaps_ResolvedByUserId",
                table: "ReceiptGaps",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_BookId",
                table: "Receipts",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_DriverId",
                table: "Receipts",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_EnteredByUserId",
                table: "Receipts",
                column: "EnteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_MerchantId",
                table: "Receipts",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ReceiptNumber",
                table: "Receipts",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_SessionId",
                table: "Receipts",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ReceiptGaps");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropTable(
                name: "CollectionSessions");

            migrationBuilder.DropTable(
                name: "Merchants");

            migrationBuilder.DropTable(
                name: "ReceiptBooks");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
