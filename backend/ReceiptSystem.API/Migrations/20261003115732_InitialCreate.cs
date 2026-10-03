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
                name: "ExcelImportBatches",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UploadedFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: false),
                    TotalRowsCount = table.Column<int>(type: "int", nullable: false),
                    AssignedRowsCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "InProgress"),
                    LedgerTotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActualCashTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelImportBatches", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_ExcelImportBatches_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    RouteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.RouteId);
                    table.ForeignKey(
                        name: "FK_Routes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    SettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.SettingKey);
                    table.ForeignKey(
                        name: "FK_SystemSettings_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
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
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ImportSource = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Manual"),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ActualCashCounted = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CashDifference = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CashReconciled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReconciledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReconciledByUserId = table.Column<int>(type: "int", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedByUserId = table.Column<int>(type: "int", nullable: true)
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
                        name: "FK_CollectionSessions_Users_ConfirmedByUserId",
                        column: x => x.ConfirmedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionSessions_Users_EnteredByUserId",
                        column: x => x.EnteredByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionSessions_Users_ReconciledByUserId",
                        column: x => x.ReconciledByUserId,
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
                    SeriesId = table.Column<int>(type: "int", nullable: true),
                    BookNumber = table.Column<int>(type: "int", nullable: false),
                    StartReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    EndReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    AssignedToDriverId = table.Column<int>(type: "int", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ReturnedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnedToUserId = table.Column<int>(type: "int", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptBooks", x => x.BookId);
                    table.ForeignKey(
                        name: "FK_ReceiptBooks_BookSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "BookSeries",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_ReceiptBooks_Users_ReturnedToUserId",
                        column: x => x.ReturnedToUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptBooks_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AjalSessions",
                columns: table => new
                {
                    SessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionDate = table.Column<DateTime>(type: "date", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AjalSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_AjalSessions_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjalSessions_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "RouteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjalSessions_Users_EnteredByUserId",
                        column: x => x.EnteredByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RouteMerchants",
                columns: table => new
                {
                    RouteMerchantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    PositionOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AddedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteMerchants", x => x.RouteMerchantId);
                    table.ForeignKey(
                        name: "FK_RouteMerchants_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteMerchants_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "RouteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteMerchants_Users_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "ReceiptGaps",
                columns: table => new
                {
                    GapId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissingReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    DetectedInSessionId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Open"),
                    Resolution = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReasonCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                        name: "FK_ReceiptGaps_ReceiptBooks_BookId",
                        column: x => x.BookId,
                        principalTable: "ReceiptBooks",
                        principalColumn: "BookId",
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
                    SeriesId = table.Column<int>(type: "int", nullable: true),
                    BookId = table.Column<int>(type: "int", nullable: true),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPartialPayment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ImportSource = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Manual"),
                    IsWithoutReceipt = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DesktopSystemUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.ReceiptId);
                    table.ForeignKey(
                        name: "FK_Receipts_BookSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "BookSeries",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "AjalEntries",
                columns: table => new
                {
                    EntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsReviewed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AjalEntries", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_AjalEntries_AjalSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AjalSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AjalEntries_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjalEntries_Users_EnteredByUserId",
                        column: x => x.EnteredByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjalEntries_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExcelImportRows",
                columns: table => new
                {
                    RowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    MerchantNameRaw = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MatchedMerchantId = table.Column<int>(type: "int", nullable: true),
                    IsNewMerchant = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAssigned = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AssignedReceiptId = table.Column<int>(type: "int", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErpInvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErpUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErpEntryTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Treasury = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErpId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelImportRows", x => x.RowId);
                    table.ForeignKey(
                        name: "FK_ExcelImportRows_ExcelImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ExcelImportBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExcelImportRows_Merchants_MatchedMerchantId",
                        column: x => x.MatchedMerchantId,
                        principalTable: "Merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExcelImportRows_Receipts_AssignedReceiptId",
                        column: x => x.AssignedReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "ReceiptId",
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
                table: "SystemSettings",
                columns: new[] { "SettingKey", "Description", "SettingValue", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { "InvoicePrefix", "البادئة الثابتة لأرقام الفواتير", "441", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { "InvoicePrefixWarningThreshold", "أظهر تحذيراً عند وصول رقم الفاتورة الأخير لهذا الحد", "950", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { "InvoiceTotalDigits", "إجمالي عدد أرقام الفاتورة الكاملة", "6", new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "FullName", "IsActive", "LastLoginAt", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), "مدير النظام", true, null, "$2a$11$quJUEgm2gUdSNIglxmKyR.V0FE2g9TUB/n4TluXADcPSNxwMldWfC", "Admin", "admin" },
                    { 2, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), "أمين الخزينة", true, null, "$2a$11$AoT./fgMLgjCd5JKduZ3U.t.JVGo8qdbWaET4XGvz0Qk1/HuxsdHG", "Treasury", "خزينة" },
                    { 3, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), "موظف الكول سنتر", true, null, "$2a$11$VeIZfPu0BOvz3.RH9RmHN.utXTDssswToNsybAT10hnPzvnQoZ2Zu", "CallCenter", "callcenter" }
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
                table: "Routes",
                columns: new[] { "RouteId", "CreatedAt", "CreatedByUserId", "IsActive", "Notes", "RouteName" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, null, "المنشية" },
                    { 2, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, null, "خريط" },
                    { 3, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, null, "أسوان" }
                });

            migrationBuilder.InsertData(
                table: "AjalSessions",
                columns: new[] { "SessionId", "DriverId", "EnteredAt", "EnteredByUserId", "Notes", "RouteId", "SessionDate" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 6, 21, 22, 0, 0, 0, DateTimeKind.Unspecified), 1, null, 1, new DateTime(2026, 6, 21, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 2, new DateTime(2026, 6, 21, 22, 30, 0, 0, DateTimeKind.Unspecified), 1, null, 2, new DateTime(2026, 6, 21, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 3, new DateTime(2026, 6, 21, 23, 0, 0, 0, DateTimeKind.Unspecified), 1, null, 3, new DateTime(2026, 6, 21, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "CollectionSessions",
                columns: new[] { "SessionId", "ActualCashCounted", "CashDifference", "ConfirmedAt", "ConfirmedByUserId", "DriverId", "EnteredAt", "EnteredByUserId", "FirstReceiptNumber", "ImportSource", "LastReceiptNumber", "Notes", "OriginalFileName", "ReconciledAt", "ReconciledByUserId", "RouteArea", "SessionDate", "TotalAmountCollected", "TotalReceiptsCount" },
                values: new object[,]
                {
                    { 1, null, null, null, null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 1, "Manual", 15, null, null, null, null, "مدينة كومومبو", new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 45000m, 15 },
                    { 2, null, null, null, null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 16, "Manual", 35, null, null, null, null, "مدينة إدفو", new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 62000m, 20 }
                });

            migrationBuilder.InsertData(
                table: "CollectionSessions",
                columns: new[] { "SessionId", "ActualCashCounted", "CashDifference", "ConfirmedAt", "ConfirmedByUserId", "DriverId", "EnteredAt", "EnteredByUserId", "FirstReceiptNumber", "HasGaps", "ImportSource", "LastReceiptNumber", "Notes", "OriginalFileName", "ReconciledAt", "ReconciledByUserId", "RouteArea", "SessionDate", "TotalAmountCollected", "TotalReceiptsCount" },
                values: new object[] { 3, null, null, null, null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 36, true, "Manual", 50, null, null, null, null, "مدينة دراو", new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 38500m, 14 });

            migrationBuilder.InsertData(
                table: "CollectionSessions",
                columns: new[] { "SessionId", "ActualCashCounted", "CashDifference", "ConfirmedAt", "ConfirmedByUserId", "DriverId", "EnteredAt", "EnteredByUserId", "FirstReceiptNumber", "ImportSource", "LastReceiptNumber", "Notes", "OriginalFileName", "ReconciledAt", "ReconciledByUserId", "RouteArea", "SessionDate", "TotalAmountCollected", "TotalReceiptsCount" },
                values: new object[] { 4, null, null, null, null, 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, 51, "Manual", 60, null, null, null, null, "مدينة أسوان", new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 28000m, 10 });

            migrationBuilder.InsertData(
                table: "ReceiptBooks",
                columns: new[] { "BookId", "AssignedByUserId", "AssignedDate", "AssignedToDriverId", "BookNumber", "CompletedDate", "CreatedAt", "EndReceiptNumber", "Notes", "ReturnedDate", "ReturnedToUserId", "SeriesId", "StartReceiptNumber", "Status", "VerifiedAt", "VerifiedByUserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, null, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 50, null, null, null, null, 1, "InProgress", null, null },
                    { 2, 1, new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 2, null, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 100, null, null, null, null, 51, "Assigned", null, null },
                    { 3, 1, new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 3, null, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 150, null, null, null, null, 101, "Assigned", null, null }
                });

            migrationBuilder.InsertData(
                table: "RouteMerchants",
                columns: new[] { "RouteMerchantId", "AddedAt", "AddedByUserId", "IsActive", "MerchantId", "Notes", "PositionOrder", "RouteId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 1, null, 1, 1 },
                    { 2, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 2, null, 2, 1 },
                    { 3, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 3, null, 3, 1 },
                    { 4, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 4, null, 4, 1 },
                    { 5, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 5, null, 5, 1 },
                    { 6, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 6, null, 6, 1 },
                    { 7, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 7, null, 7, 1 },
                    { 8, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 8, null, 8, 1 },
                    { 9, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 9, null, 9, 1 },
                    { 10, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 10, null, 10, 1 },
                    { 11, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 11, null, 11, 1 },
                    { 12, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 12, null, 12, 1 },
                    { 13, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 13, null, 13, 1 },
                    { 14, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 14, null, 14, 1 },
                    { 15, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 15, null, 15, 1 },
                    { 16, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 1, null, 1, 2 },
                    { 17, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 2, null, 2, 2 },
                    { 18, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 3, null, 3, 2 },
                    { 19, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 4, null, 4, 2 },
                    { 20, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 5, null, 5, 2 },
                    { 21, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 6, null, 6, 2 },
                    { 22, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 7, null, 7, 2 },
                    { 23, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 8, null, 8, 2 },
                    { 24, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 9, null, 9, 2 },
                    { 25, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 10, null, 10, 2 },
                    { 26, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 1, null, 1, 3 },
                    { 27, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 2, null, 2, 3 },
                    { 28, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 3, null, 3, 3 },
                    { 29, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 4, null, 4, 3 },
                    { 30, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 5, null, 5, 3 },
                    { 31, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 6, null, 6, 3 },
                    { 32, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 7, null, 7, 3 },
                    { 33, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 8, null, 8, 3 },
                    { 34, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 9, null, 9, 3 },
                    { 35, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 10, null, 10, 3 },
                    { 36, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 11, null, 11, 3 },
                    { 37, new DateTime(2026, 6, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 12, null, 12, 3 }
                });

            migrationBuilder.InsertData(
                table: "AjalEntries",
                columns: new[] { "EntryId", "Amount", "EnteredAt", "EnteredByUserId", "InvoiceNumber", "IsReviewed", "MerchantId", "Notes", "ReviewedAt", "ReviewedByUserId", "SessionId", "SortOrder" },
                values: new object[,]
                {
                    { 1, 1750m, new DateTime(2026, 6, 21, 22, 0, 0, 0, DateTimeKind.Unspecified), 1, "441103", true, 1, null, new DateTime(2026, 6, 22, 10, 0, 0, 0, DateTimeKind.Unspecified), 3, 1, 1 },
                    { 2, 2000m, new DateTime(2026, 6, 21, 22, 0, 0, 0, DateTimeKind.Unspecified), 1, "441106", true, 2, null, new DateTime(2026, 6, 22, 10, 0, 0, 0, DateTimeKind.Unspecified), 3, 1, 2 },
                    { 3, 2250m, new DateTime(2026, 6, 21, 22, 0, 0, 0, DateTimeKind.Unspecified), 1, "441109", true, 3, null, new DateTime(2026, 6, 22, 10, 0, 0, 0, DateTimeKind.Unspecified), 3, 1, 3 },
                    { 4, 2500m, new DateTime(2026, 6, 21, 22, 0, 0, 0, DateTimeKind.Unspecified), 1, "441112", true, 4, null, new DateTime(2026, 6, 22, 10, 0, 0, 0, DateTimeKind.Unspecified), 3, 1, 4 },
                    { 5, 2750m, new DateTime(2026, 6, 21, 22, 0, 0, 0, DateTimeKind.Unspecified), 1, "441115", true, 5, null, new DateTime(2026, 6, 22, 10, 0, 0, 0, DateTimeKind.Unspecified), 3, 1, 5 }
                });

            migrationBuilder.InsertData(
                table: "AjalEntries",
                columns: new[] { "EntryId", "Amount", "EnteredAt", "EnteredByUserId", "InvoiceNumber", "MerchantId", "Notes", "ReviewedAt", "ReviewedByUserId", "SessionId", "SortOrder" },
                values: new object[,]
                {
                    { 6, 3000m, new DateTime(2026, 6, 21, 22, 0, 0, 0, DateTimeKind.Unspecified), 1, "441118", 6, null, null, null, 1, 6 },
                    { 7, 3250m, new DateTime(2026, 6, 21, 22, 0, 0, 0, DateTimeKind.Unspecified), 1, "441121", 7, null, null, null, 1, 7 },
                    { 8, 3500m, new DateTime(2026, 6, 21, 22, 0, 0, 0, DateTimeKind.Unspecified), 1, "441124", 8, null, null, null, 1, 8 },
                    { 9, 2300m, new DateTime(2026, 6, 21, 22, 30, 0, 0, DateTimeKind.Unspecified), 1, "441132", 1, null, null, null, 2, 1 },
                    { 10, 2600m, new DateTime(2026, 6, 21, 22, 30, 0, 0, DateTimeKind.Unspecified), 1, "441134", 2, null, null, null, 2, 2 },
                    { 11, 2900m, new DateTime(2026, 6, 21, 22, 30, 0, 0, DateTimeKind.Unspecified), 1, "441136", 3, null, null, null, 2, 3 },
                    { 12, 3200m, new DateTime(2026, 6, 21, 22, 30, 0, 0, DateTimeKind.Unspecified), 1, "441138", 4, null, null, null, 2, 4 },
                    { 13, 3500m, new DateTime(2026, 6, 21, 22, 30, 0, 0, DateTimeKind.Unspecified), 1, "441140", 5, null, null, null, 2, 5 },
                    { 14, 3800m, new DateTime(2026, 6, 21, 22, 30, 0, 0, DateTimeKind.Unspecified), 1, "441142", 6, null, null, null, 2, 6 },
                    { 15, 3500m, new DateTime(2026, 6, 21, 23, 0, 0, 0, DateTimeKind.Unspecified), 1, "441164", 6, null, null, null, 3, 1 },
                    { 16, 4000m, new DateTime(2026, 6, 21, 23, 0, 0, 0, DateTimeKind.Unspecified), 1, "441168", 7, null, null, null, 3, 2 },
                    { 17, 4500m, new DateTime(2026, 6, 21, 23, 0, 0, 0, DateTimeKind.Unspecified), 1, "441172", 8, null, null, null, 3, 3 },
                    { 18, 5000m, new DateTime(2026, 6, 21, 23, 0, 0, 0, DateTimeKind.Unspecified), 1, "441176", 9, null, null, null, 3, 4 },
                    { 19, 5500m, new DateTime(2026, 6, 21, 23, 0, 0, 0, DateTimeKind.Unspecified), 1, "441180", 10, null, null, null, 3, 5 }
                });

            migrationBuilder.InsertData(
                table: "ReceiptGaps",
                columns: new[] { "GapId", "BookId", "DetectedAt", "DetectedInSessionId", "DriverId", "MissingReceiptNumber", "ReasonCategory", "Resolution", "ResolvedAt", "ResolvedByUserId", "Status" },
                values: new object[] { 1, null, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 3, 1, 47, null, null, null, null, "Open" });

            migrationBuilder.InsertData(
                table: "Receipts",
                columns: new[] { "ReceiptId", "Amount", "BookId", "CollectionDate", "DesktopSystemUser", "DriverId", "EnteredAt", "EnteredByUserId", "ImportSource", "MerchantId", "Notes", "ReceiptNumber", "SeriesId", "SessionId" },
                values: new object[,]
                {
                    { 1, 3100m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 1, null, 1, null, 1 },
                    { 2, 3200m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 2, null, 2, null, 1 },
                    { 3, 3300m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 3, null, 3, null, 1 },
                    { 4, 3400m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 4, null, 4, null, 1 },
                    { 5, 3500m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 5, null, 5, null, 1 },
                    { 6, 3600m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 6, null, 6, null, 1 },
                    { 7, 3700m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 7, null, 7, null, 1 },
                    { 8, 3800m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 8, null, 8, null, 1 },
                    { 9, 3900m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 9, null, 9, null, 1 },
                    { 10, 4000m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 10, null, 10, null, 1 },
                    { 11, 4100m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 11, null, 11, null, 1 },
                    { 12, 4200m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 12, null, 12, null, 1 },
                    { 13, 4300m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 13, null, 13, null, 1 },
                    { 14, 4400m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 14, null, 14, null, 1 },
                    { 15, 4500m, 1, new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 15, null, 15, null, 1 },
                    { 16, 3300m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 16, null, 16, null, 2 },
                    { 17, 3350m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 17, null, 17, null, 2 },
                    { 18, 3400m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 18, null, 18, null, 2 },
                    { 19, 3450m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 19, null, 19, null, 2 },
                    { 20, 3500m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 20, null, 20, null, 2 },
                    { 21, 3550m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 1, null, 21, null, 2 },
                    { 22, 3600m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 2, null, 22, null, 2 },
                    { 23, 3650m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 3, null, 23, null, 2 },
                    { 24, 3700m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 4, null, 24, null, 2 },
                    { 25, 3750m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 5, null, 25, null, 2 },
                    { 26, 3800m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 6, null, 26, null, 2 },
                    { 27, 3850m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 7, null, 27, null, 2 },
                    { 28, 3900m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 8, null, 28, null, 2 },
                    { 29, 3950m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 9, null, 29, null, 2 },
                    { 30, 4000m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 10, null, 30, null, 2 },
                    { 31, 4050m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 11, null, 31, null, 2 },
                    { 32, 4100m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 12, null, 32, null, 2 },
                    { 33, 4150m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 13, null, 33, null, 2 },
                    { 34, 4200m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 14, null, 34, null, 2 },
                    { 35, 4250m, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 23, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 15, null, 35, null, 2 },
                    { 36, 4700m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 16, null, 36, null, 3 },
                    { 37, 4775m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 17, null, 37, null, 3 },
                    { 38, 4850m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 18, null, 38, null, 3 },
                    { 39, 4925m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 19, null, 39, null, 3 },
                    { 40, 5000m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 20, null, 40, null, 3 },
                    { 41, 5075m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 1, null, 41, null, 3 },
                    { 42, 5150m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 2, null, 42, null, 3 },
                    { 43, 5225m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 3, null, 43, null, 3 },
                    { 44, 5300m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 4, null, 44, null, 3 },
                    { 45, 5375m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 5, null, 45, null, 3 },
                    { 46, 5450m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 6, null, 46, null, 3 },
                    { 47, 5600m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 8, null, 48, null, 3 },
                    { 48, 5675m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 9, null, 49, null, 3 },
                    { 49, 5750m, 1, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 6, 24, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 10, null, 50, null, 3 },
                    { 50, 5350m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 11, null, 51, null, 4 },
                    { 51, 5400m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 12, null, 52, null, 4 },
                    { 52, 5450m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 13, null, 53, null, 4 },
                    { 53, 5500m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 14, null, 54, null, 4 },
                    { 54, 5550m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 15, null, 55, null, 4 },
                    { 55, 5600m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 16, null, 56, null, 4 },
                    { 56, 5650m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 17, null, 57, null, 4 },
                    { 57, 5700m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 18, null, 58, null, 4 },
                    { 58, 5750m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 19, null, 59, null, 4 },
                    { 59, 5800m, 2, new DateTime(2026, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new DateTime(2026, 6, 24, 17, 0, 0, 0, DateTimeKind.Unspecified), 2, "Manual", 20, null, 60, null, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AjalEntries_EnteredByUserId",
                table: "AjalEntries",
                column: "EnteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AjalEntries_InvoiceNumber",
                table: "AjalEntries",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AjalEntries_MerchantId",
                table: "AjalEntries",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_AjalEntries_ReviewedByUserId",
                table: "AjalEntries",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AjalEntries_SessionId",
                table: "AjalEntries",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AjalSessions_DriverId",
                table: "AjalSessions",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_AjalSessions_EnteredByUserId",
                table: "AjalSessions",
                column: "EnteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AjalSessions_RouteId",
                table: "AjalSessions",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

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

            migrationBuilder.CreateIndex(
                name: "IX_BookSeries_CreatedByUserId",
                table: "BookSeries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookSeries_SeriesCode",
                table: "BookSeries",
                column: "SeriesCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSessions_ConfirmedByUserId",
                table: "CollectionSessions",
                column: "ConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSessions_DriverId",
                table: "CollectionSessions",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSessions_EnteredByUserId",
                table: "CollectionSessions",
                column: "EnteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSessions_ReconciledByUserId",
                table: "CollectionSessions",
                column: "ReconciledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CreatedByUserId",
                table: "Drivers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelImportBatches_UploadedByUserId",
                table: "ExcelImportBatches",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelImportRows_AssignedReceiptId",
                table: "ExcelImportRows",
                column: "AssignedReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelImportRows_BatchId",
                table: "ExcelImportRows",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelImportRows_MatchedMerchantId",
                table: "ExcelImportRows",
                column: "MatchedMerchantId");

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
                name: "IX_ReceiptGaps_BookId",
                table: "ReceiptGaps",
                column: "BookId");

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
                name: "IX_Receipts_SeriesId_ReceiptNumber",
                table: "Receipts",
                columns: new[] { "SeriesId", "ReceiptNumber" },
                unique: true,
                filter: "[SeriesId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_SessionId",
                table: "Receipts",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteMerchants_AddedByUserId",
                table: "RouteMerchants",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteMerchants_MerchantId",
                table: "RouteMerchants",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteMerchants_RouteId_MerchantId",
                table: "RouteMerchants",
                columns: new[] { "RouteId", "MerchantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteMerchants_RouteId_PositionOrder",
                table: "RouteMerchants",
                columns: new[] { "RouteId", "PositionOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_CreatedByUserId",
                table: "Routes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_UpdatedByUserId",
                table: "SystemSettings",
                column: "UpdatedByUserId");

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
                name: "AjalEntries");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BookMovements");

            migrationBuilder.DropTable(
                name: "ExcelImportRows");

            migrationBuilder.DropTable(
                name: "ReceiptGaps");

            migrationBuilder.DropTable(
                name: "RouteMerchants");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "AjalSessions");

            migrationBuilder.DropTable(
                name: "ExcelImportBatches");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "CollectionSessions");

            migrationBuilder.DropTable(
                name: "Merchants");

            migrationBuilder.DropTable(
                name: "ReceiptBooks");

            migrationBuilder.DropTable(
                name: "BookSeries");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
