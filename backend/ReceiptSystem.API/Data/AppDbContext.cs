using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Models;

namespace ReceiptSystem.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<BookSeries> BookSeries => Set<BookSeries>();
    public DbSet<ReceiptBook> ReceiptBooks => Set<ReceiptBook>();
    public DbSet<CollectionSession> CollectionSessions => Set<CollectionSession>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ReceiptGap> ReceiptGaps => Set<ReceiptGap>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Module 2: Route Order & Loading Sheets
    public DbSet<Models.Route> Routes => Set<Models.Route>();
    public DbSet<RouteMerchant> RouteMerchants => Set<RouteMerchant>();
    public DbSet<DeliveryDay> DeliveryDays => Set<DeliveryDay>();
    public DbSet<DayInvoice> DayInvoices => Set<DayInvoice>();

    // Module 3: Ajal Register
    public DbSet<AjalInvoice> AjalInvoices => Set<AjalInvoice>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    // ERP Import
    public DbSet<ExcelImportBatch> ExcelImportBatches => Set<ExcelImportBatch>();
    public DbSet<ExcelImportRow> ExcelImportRows => Set<ExcelImportRow>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── User ──
        mb.Entity<User>(e =>
        {
            e.HasKey(u => u.UserId);
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Username).HasMaxLength(50).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
            e.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("Treasury");
            e.Property(u => u.IsActive).HasDefaultValue(true);
            e.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        // ── Driver ──
        mb.Entity<Driver>(e =>
        {
            e.HasKey(d => d.DriverId);
            e.Property(d => d.FullName).HasMaxLength(100).IsRequired();
            e.Property(d => d.PhoneNumber).HasMaxLength(20).IsRequired();
            e.Property(d => d.IsActive).HasDefaultValue(true);
            e.Property(d => d.Notes).HasMaxLength(500);
            e.Property(d => d.CreatedAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(d => d.CreatedByUser).WithMany().HasForeignKey(d => d.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Merchant ──
        mb.Entity<Merchant>(e =>
        {
            e.HasKey(m => m.MerchantId);
            e.Property(m => m.MerchantName).HasMaxLength(200).IsRequired();
            e.HasIndex(m => m.MerchantName);
            e.Property(m => m.City).HasMaxLength(100);
            e.Property(m => m.PhoneNumber).HasMaxLength(20);
            e.Property(m => m.IsActive).HasDefaultValue(true);
            e.Property(m => m.Notes).HasMaxLength(500);
            e.Property(m => m.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        // ── BookSeries ──
        mb.Entity<BookSeries>(e =>
        {
            e.HasKey(s => s.SeriesId);
            e.HasIndex(s => s.SeriesCode).IsUnique();
            e.Property(s => s.SeriesCode).HasMaxLength(10).IsRequired();
            e.Property(s => s.Status).HasMaxLength(20).HasDefaultValue("Active");
            e.Property(s => s.Notes).HasMaxLength(500);
            e.Property(s => s.CreatedAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(s => s.CreatedByUser).WithMany().HasForeignKey(s => s.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── ReceiptBook ──
        mb.Entity<ReceiptBook>(e =>
        {
            e.HasKey(b => b.BookId);
            e.HasIndex(b => new { b.SeriesId, b.BookNumber }).IsUnique();
            e.Property(b => b.Status).HasMaxLength(20).HasDefaultValue("Available");
            e.Property(b => b.Notes).HasMaxLength(500);
            e.Property(b => b.CreatedAt).HasDefaultValueSql("GETDATE()");
            e.Property(b => b.IsVerified).HasDefaultValue(false);
            e.HasOne(b => b.Series).WithMany(s => s.Books).HasForeignKey(b => b.SeriesId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.AssignedToDriver).WithMany(d => d.AssignedBooks).HasForeignKey(b => b.AssignedToDriverId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.AssignedByUser).WithMany().HasForeignKey(b => b.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.ReturnedToUser).WithMany().HasForeignKey(b => b.ReturnedToUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.VerifiedByUser).WithMany().HasForeignKey(b => b.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── CollectionSession ──
        mb.Entity<CollectionSession>(e =>
        {
            e.HasKey(s => s.SessionId);
            e.Property(s => s.RouteArea).HasMaxLength(200).IsRequired();
            e.Property(s => s.TotalAmountCollected).HasColumnType("decimal(18,2)");
            e.Property(s => s.HasGaps).HasDefaultValue(false);
            e.Property(s => s.Notes).HasMaxLength(1000);
            e.Property(s => s.EnteredAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(s => s.Driver).WithMany(d => d.Sessions).HasForeignKey(s => s.DriverId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.EnteredByUser).WithMany().HasForeignKey(s => s.EnteredByUserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(s => s.ImportSource).HasMaxLength(20).HasDefaultValue("Manual");
            e.Property(s => s.OriginalFileName).HasMaxLength(255);

            // Cash Reconciliation
            e.Property(s => s.ActualCashCounted).HasColumnType("decimal(18,2)");
            e.Property(s => s.CashDifference).HasColumnType("decimal(18,2)");
            e.Property(s => s.CashReconciled).HasDefaultValue(false);
            e.HasOne(s => s.ReconciledByUser).WithMany().HasForeignKey(s => s.ReconciledByUserId).OnDelete(DeleteBehavior.Restrict);

            // Session Confirmation
            e.Property(s => s.IsConfirmed).HasDefaultValue(false);
            e.HasOne(s => s.ConfirmedByUser).WithMany().HasForeignKey(s => s.ConfirmedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Receipt ──
        mb.Entity<Receipt>(e =>
        {
            e.HasKey(r => r.ReceiptId);
            e.HasIndex(r => new { r.SeriesId, r.ReceiptNumber }).IsUnique();
            e.HasOne(r => r.Series).WithMany().HasForeignKey(r => r.SeriesId).OnDelete(DeleteBehavior.Restrict);
            e.Property(r => r.Amount).HasColumnType("decimal(18,2)");
            e.Property(r => r.IsPartialPayment).HasDefaultValue(false);
            e.Property(r => r.Notes).HasMaxLength(500);
            e.Property(r => r.EnteredAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(r => r.Book).WithMany(b => b.Receipts).HasForeignKey(r => r.BookId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(r => r.Session).WithMany(s => s.Receipts).HasForeignKey(r => r.SessionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Driver).WithMany(d => d.Receipts).HasForeignKey(r => r.DriverId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Merchant).WithMany(m => m.Receipts).HasForeignKey(r => r.MerchantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.EnteredByUser).WithMany().HasForeignKey(r => r.EnteredByUserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(r => r.ImportSource).HasMaxLength(20).HasDefaultValue("Manual");
            e.Property(r => r.IsWithoutReceipt).HasDefaultValue(false);
            e.Property(r => r.DesktopSystemUser).HasMaxLength(100);
        });

        // ── ReceiptGap ──
        mb.Entity<ReceiptGap>(e =>
        {
            e.HasKey(g => g.GapId);
            e.Property(g => g.Status).HasMaxLength(30).HasDefaultValue("Open");
            e.Property(g => g.Resolution).HasMaxLength(1000);
            e.Property(g => g.DetectedAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(g => g.DetectedInSession).WithMany(s => s.DetectedGaps).HasForeignKey(g => g.DetectedInSessionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(g => g.Driver).WithMany(d => d.Gaps).HasForeignKey(g => g.DriverId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(g => g.ResolvedByUser).WithMany().HasForeignKey(g => g.ResolvedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── AuditLog ──
        mb.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.LogId);
            e.Property(a => a.Action).HasMaxLength(100).IsRequired();
            e.Property(a => a.EntityType).HasMaxLength(50).IsRequired();
            e.Property(a => a.IpAddress).HasMaxLength(50);
            e.Property(a => a.CreatedAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Module 2: Route ──
        mb.Entity<Models.Route>(e =>
        {
            e.HasKey(r => r.RouteId);
            e.Property(r => r.RouteName).HasMaxLength(200).IsRequired();
            e.Property(r => r.IsActive).HasDefaultValue(true);
            e.Property(r => r.Notes).HasMaxLength(500);
            e.Property(r => r.CreatedAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(r => r.CreatedByUser).WithMany().HasForeignKey(r => r.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Module 2: RouteMerchant ──
        mb.Entity<RouteMerchant>(e =>
        {
            e.HasKey(rm => rm.RouteMerchantId);
            e.HasIndex(rm => new { rm.RouteId, rm.PositionOrder }).IsUnique();
            e.HasIndex(rm => new { rm.RouteId, rm.MerchantId }).IsUnique();
            e.Property(rm => rm.IsActive).HasDefaultValue(true);
            e.Property(rm => rm.Notes).HasMaxLength(300);
            e.Property(rm => rm.AddedAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(rm => rm.Route).WithMany(r => r.RouteMerchants).HasForeignKey(rm => rm.RouteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(rm => rm.Merchant).WithMany(m => m.RouteMerchants).HasForeignKey(rm => rm.MerchantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(rm => rm.AddedByUser).WithMany().HasForeignKey(rm => rm.AddedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Module 2: DeliveryDay ──
        mb.Entity<DeliveryDay>(e =>
        {
            e.HasKey(dd => dd.DeliveryDayId);
            e.HasIndex(dd => new { dd.RouteId, dd.DeliveryDate }).IsUnique();
            e.Property(dd => dd.Status).HasMaxLength(20).HasDefaultValue("Draft");
            e.Property(dd => dd.AssignedDriver).HasMaxLength(100);
            e.Property(dd => dd.Notes).HasMaxLength(500);
            e.Property(dd => dd.CreatedAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(dd => dd.Route).WithMany(r => r.DeliveryDays).HasForeignKey(dd => dd.RouteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(dd => dd.CreatedByUser).WithMany().HasForeignKey(dd => dd.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Module 2: DayInvoice ──
        mb.Entity<DayInvoice>(e =>
        {
            e.HasKey(di => di.DayInvoiceId);
            e.Property(di => di.InvoiceNumber).HasMaxLength(50);
            e.Property(di => di.Quantity).HasMaxLength(200);
            e.Property(di => di.Amount).HasColumnType("decimal(18,2)");
            e.Property(di => di.Notes).HasMaxLength(300);
            e.Property(di => di.EnteredAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(di => di.DeliveryDay).WithMany(dd => dd.DayInvoices).HasForeignKey(di => di.DeliveryDayId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(di => di.RouteMerchant).WithMany(rm => rm.DayInvoices).HasForeignKey(di => di.RouteMerchantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(di => di.Merchant).WithMany(m => m.DayInvoices).HasForeignKey(di => di.MerchantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(di => di.EnteredByUser).WithMany().HasForeignKey(di => di.EnteredByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Module 3: AjalInvoice ──
        mb.Entity<AjalInvoice>(e =>
        {
            e.HasKey(a => a.AjalInvoiceId);
            e.HasIndex(a => a.InvoiceNumber).IsUnique();
            e.Property(a => a.InvoiceNumber).HasMaxLength(20).IsRequired();
            e.Property(a => a.CallCenterEmployeeName).HasMaxLength(100);
            e.Property(a => a.Amount).HasColumnType("decimal(18,2)");
            e.Property(a => a.SessionDate).HasColumnType("date");
            e.Property(a => a.InvoiceStatus).HasMaxLength(20).HasDefaultValue("Active");
            e.Property(a => a.OriginalAmount).HasColumnType("decimal(18,2)");
            e.Property(a => a.ModificationNote).HasMaxLength(500);
            e.Property(a => a.Notes).HasMaxLength(500);
            e.Property(a => a.ImportSource).HasMaxLength(20).HasDefaultValue("Manual");
            e.Property(a => a.EnteredAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(a => a.Merchant).WithMany().HasForeignKey(a => a.MerchantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Route).WithMany().HasForeignKey(a => a.RouteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.EnteredByUser).WithMany().HasForeignKey(a => a.EnteredByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Module 3: SystemSetting ──
        mb.Entity<SystemSetting>(e =>
        {
            e.HasKey(s => s.SettingKey);
            e.Property(s => s.SettingKey).HasMaxLength(100);
            e.Property(s => s.SettingValue).HasMaxLength(500).IsRequired();
            e.Property(s => s.Description).HasMaxLength(300);
            e.Property(s => s.UpdatedAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(s => s.UpdatedByUser).WithMany().HasForeignKey(s => s.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── ExcelImportBatch ──
        mb.Entity<ExcelImportBatch>(e =>
        {
            e.HasKey(b => b.BatchId);
            e.Property(b => b.UploadedFileName).HasMaxLength(255).IsRequired();
            e.Property(b => b.Status).HasMaxLength(20).HasDefaultValue("InProgress");
            e.Property(b => b.LedgerTotalAmount).HasColumnType("decimal(18,2)");
            e.Property(b => b.ActualCashTotal).HasColumnType("decimal(18,2)");
            e.Property(b => b.Notes).HasMaxLength(500);
            e.HasOne(b => b.UploadedByUser).WithMany().HasForeignKey(b => b.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── ExcelImportRow ──
        mb.Entity<ExcelImportRow>(e =>
        {
            e.HasKey(r => r.RowId);
            e.Property(r => r.MerchantNameRaw).HasMaxLength(200).IsRequired();
            e.Property(r => r.Amount).HasColumnType("decimal(18,2)");
            e.Property(r => r.IsAssigned).HasDefaultValue(false);
            e.Property(r => r.IsNewMerchant).HasDefaultValue(false);
            e.HasOne(r => r.Batch).WithMany(b => b.Rows).HasForeignKey(r => r.BatchId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.MatchedMerchant).WithMany().HasForeignKey(r => r.MatchedMerchantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.AssignedReceipt).WithMany().HasForeignKey(r => r.AssignedReceiptId).OnDelete(DeleteBehavior.Restrict);
        });

        SeedData(mb);
    }

    private void SeedData(ModelBuilder mb)
    {
        var now = new DateTime(2026, 6, 22, 8, 0, 0);

        // Users
        mb.Entity<User>().HasData(
            new User { UserId = 1, Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), FullName = "مدير النظام", Role = "Admin", IsActive = true, CreatedAt = now },
            new User { UserId = 2, Username = "خزينة", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Khazna@123"), FullName = "أمين الخزينة", Role = "Treasury", IsActive = true, CreatedAt = now },
            new User { UserId = 3, Username = "callcenter", PasswordHash = BCrypt.Net.BCrypt.HashPassword("CallCenter@123"), FullName = "موظف الكول سنتر", Role = "CallCenter", IsActive = true, CreatedAt = now }
        );

        // SystemSettings seed
        mb.Entity<SystemSetting>().HasData(
            new SystemSetting { SettingKey = "InvoicePrefix", SettingValue = "441", Description = "البادئة الثابتة لأرقام الفواتير", UpdatedAt = now },
            new SystemSetting { SettingKey = "InvoicePrefixWarningThreshold", SettingValue = "950", Description = "أظهر تحذيراً عند وصول رقم الفاتورة الأخير لهذا الحد", UpdatedAt = now },
            new SystemSetting { SettingKey = "InvoiceTotalDigits", SettingValue = "6", Description = "إجمالي عدد أرقام الفاتورة الكاملة", UpdatedAt = now }
        );

        // Drivers
        mb.Entity<Driver>().HasData(
            new Driver { DriverId = 1, FullName = "أحمد محمود حسن", PhoneNumber = "01012345678", IsActive = true, CreatedAt = now, CreatedByUserId = 1 },
            new Driver { DriverId = 2, FullName = "محمد عبدالله سالم", PhoneNumber = "01123456789", IsActive = true, CreatedAt = now, CreatedByUserId = 1 },
            new Driver { DriverId = 3, FullName = "خالد إبراهيم أحمد", PhoneNumber = "01234567890", IsActive = true, CreatedAt = now, CreatedByUserId = 1 },
            new Driver { DriverId = 4, FullName = "عمر حسين علي", PhoneNumber = "01098765432", IsActive = true, CreatedAt = now, CreatedByUserId = 1 },
            new Driver { DriverId = 5, FullName = "يوسف سعيد محمد", PhoneNumber = "01187654321", IsActive = true, CreatedAt = now, CreatedByUserId = 1 }
        );

        // Merchants (20)
        mb.Entity<Merchant>().HasData(
            new Merchant { MerchantId = 1, MerchantName = "محلات النور للتجارة", City = "كومومبو", PhoneNumber = "01011111111", CreatedAt = now },
            new Merchant { MerchantId = 2, MerchantName = "شركة الأمل للتوزيع", City = "أسوان", PhoneNumber = "01022222222", CreatedAt = now },
            new Merchant { MerchantId = 3, MerchantName = "مؤسسة البركة", City = "إدفو", PhoneNumber = "01033333333", CreatedAt = now },
            new Merchant { MerchantId = 4, MerchantName = "محلات الحرمين", City = "كومومبو", PhoneNumber = "01044444444", CreatedAt = now },
            new Merchant { MerchantId = 5, MerchantName = "تجارة السلام", City = "دراو", PhoneNumber = "01055555555", CreatedAt = now },
            new Merchant { MerchantId = 6, MerchantName = "محلات الفتح", City = "أسوان", PhoneNumber = "01066666666", CreatedAt = now },
            new Merchant { MerchantId = 7, MerchantName = "شركة الوفاء للمواد الغذائية", City = "إدفو", PhoneNumber = "01077777777", CreatedAt = now },
            new Merchant { MerchantId = 8, MerchantName = "مؤسسة الخير", City = "كومومبو", PhoneNumber = "01088888888", CreatedAt = now },
            new Merchant { MerchantId = 9, MerchantName = "محلات الإخوة", City = "نصر النوبة", PhoneNumber = "01099999999", CreatedAt = now },
            new Merchant { MerchantId = 10, MerchantName = "تجارة الصفا", City = "أسوان", PhoneNumber = "01111111111", CreatedAt = now },
            new Merchant { MerchantId = 11, MerchantName = "محلات الياسمين", City = "دراو", PhoneNumber = "01122222222", CreatedAt = now },
            new Merchant { MerchantId = 12, MerchantName = "شركة النجاح", City = "إدفو", PhoneNumber = "01133333333", CreatedAt = now },
            new Merchant { MerchantId = 13, MerchantName = "مؤسسة الحمد", City = "كومومبو", PhoneNumber = "01144444444", CreatedAt = now },
            new Merchant { MerchantId = 14, MerchantName = "محلات المدينة", City = "أسوان", PhoneNumber = "01155555555", CreatedAt = now },
            new Merchant { MerchantId = 15, MerchantName = "تجارة الرحمة", City = "دراو", PhoneNumber = "01166666666", CreatedAt = now },
            new Merchant { MerchantId = 16, MerchantName = "محلات الأندلس", City = "نصر النوبة", PhoneNumber = "01177777777", CreatedAt = now },
            new Merchant { MerchantId = 17, MerchantName = "شركة التوحيد للتجارة", City = "أسوان", PhoneNumber = "01188888888", CreatedAt = now },
            new Merchant { MerchantId = 18, MerchantName = "محلات الزهراء", City = "إدفو", PhoneNumber = "01199999999", CreatedAt = now },
            new Merchant { MerchantId = 19, MerchantName = "مؤسسة العروبة", City = "كومومبو", PhoneNumber = "01200000000", CreatedAt = now },
            new Merchant { MerchantId = 20, MerchantName = "محلات السندباد", City = "أسوان", PhoneNumber = "01211111111", CreatedAt = now }
        );

        // Receipt Books (3 books: 1-50, 51-100, 101-150)
        mb.Entity<ReceiptBook>().HasData(
            new ReceiptBook { BookId = 1, BookNumber = 1, StartReceiptNumber = 1, EndReceiptNumber = 50, AssignedToDriverId = 1, AssignedDate = new DateTime(2026, 6, 20), Status = "InProgress", AssignedByUserId = 1, CreatedAt = now },
            new ReceiptBook { BookId = 2, BookNumber = 2, StartReceiptNumber = 51, EndReceiptNumber = 100, AssignedToDriverId = 2, AssignedDate = new DateTime(2026, 6, 20), Status = "Assigned", AssignedByUserId = 1, CreatedAt = now },
            new ReceiptBook { BookId = 3, BookNumber = 3, StartReceiptNumber = 101, EndReceiptNumber = 150, AssignedToDriverId = 3, AssignedDate = new DateTime(2026, 6, 20), Status = "Assigned", AssignedByUserId = 1, CreatedAt = now }
        );

        // Sessions: 3 days of data for driver 1 with intentional gap at receipt 47
        var day1 = new DateTime(2026, 6, 22);
        var day2 = new DateTime(2026, 6, 23);
        var day3 = new DateTime(2026, 6, 24);

        mb.Entity<CollectionSession>().HasData(
            new CollectionSession { SessionId = 1, DriverId = 1, SessionDate = day1, RouteArea = "مدينة كومومبو", FirstReceiptNumber = 1, LastReceiptNumber = 15, TotalReceiptsCount = 15, TotalAmountCollected = 45000m, HasGaps = false, EnteredByUserId = 2, EnteredAt = day1.AddHours(16) },
            new CollectionSession { SessionId = 2, DriverId = 1, SessionDate = day2, RouteArea = "مدينة إدفو", FirstReceiptNumber = 16, LastReceiptNumber = 35, TotalReceiptsCount = 20, TotalAmountCollected = 62000m, HasGaps = false, EnteredByUserId = 2, EnteredAt = day2.AddHours(16) },
            // Day 3: receipts 36-46, 48-50 (missing 47!)
            new CollectionSession { SessionId = 3, DriverId = 1, SessionDate = day3, RouteArea = "مدينة دراو", FirstReceiptNumber = 36, LastReceiptNumber = 50, TotalReceiptsCount = 14, TotalAmountCollected = 38500m, HasGaps = true, EnteredByUserId = 2, EnteredAt = day3.AddHours(16) },
            // Driver 2 session
            new CollectionSession { SessionId = 4, DriverId = 2, SessionDate = day3, RouteArea = "مدينة أسوان", FirstReceiptNumber = 51, LastReceiptNumber = 60, TotalReceiptsCount = 10, TotalAmountCollected = 28000m, HasGaps = false, EnteredByUserId = 2, EnteredAt = day3.AddHours(17) }
        );

        // Receipts for sessions
        var receipts = new List<Receipt>();
        int receiptId = 1;

        // Session 1: receipts 1–15
        for (int i = 1; i <= 15; i++)
        {
            receipts.Add(new Receipt { ReceiptId = receiptId++, ReceiptNumber = i, BookId = 1, SessionId = 1, DriverId = 1, MerchantId = ((i - 1) % 20) + 1, CollectionDate = day1, Amount = 3000m + (i * 100), EnteredByUserId = 2, EnteredAt = day1.AddHours(16) });
        }
        // Session 2: receipts 16–35
        for (int i = 16; i <= 35; i++)
        {
            receipts.Add(new Receipt { ReceiptId = receiptId++, ReceiptNumber = i, BookId = 1, SessionId = 2, DriverId = 1, MerchantId = ((i - 1) % 20) + 1, CollectionDate = day2, Amount = 2500m + (i * 50), EnteredByUserId = 2, EnteredAt = day2.AddHours(16) });
        }
        // Session 3: receipts 36–46, 48–50 (skip 47!)
        for (int i = 36; i <= 50; i++)
        {
            if (i == 47) continue; // Intentional gap
            receipts.Add(new Receipt { ReceiptId = receiptId++, ReceiptNumber = i, BookId = 1, SessionId = 3, DriverId = 1, MerchantId = ((i - 1) % 20) + 1, CollectionDate = day3, Amount = 2000m + (i * 75), EnteredByUserId = 2, EnteredAt = day3.AddHours(16) });
        }
        // Session 4: Driver 2, receipts 51–60
        for (int i = 51; i <= 60; i++)
        {
            receipts.Add(new Receipt { ReceiptId = receiptId++, ReceiptNumber = i, BookId = 2, SessionId = 4, DriverId = 2, MerchantId = ((i - 1) % 20) + 1, CollectionDate = day3, Amount = 2800m + (i * 50), EnteredByUserId = 2, EnteredAt = day3.AddHours(17) });
        }

        mb.Entity<Receipt>().HasData(receipts);

        // Gap for missing receipt 47
        mb.Entity<ReceiptGap>().HasData(
            new ReceiptGap { GapId = 1, MissingReceiptNumber = 47, DetectedInSessionId = 3, DriverId = 1, DetectedAt = day3.AddHours(16), Status = "Open" }
        );

        // ══════════════════════════════════
        // MODULE 2 SEED DATA
        // ══════════════════════════════════

        // Routes
        mb.Entity<Models.Route>().HasData(
            new Models.Route { RouteId = 1, RouteName = "المنشية", IsActive = true, CreatedAt = now, CreatedByUserId = 1 },
            new Models.Route { RouteId = 2, RouteName = "خريط", IsActive = true, CreatedAt = now, CreatedByUserId = 1 },
            new Models.Route { RouteId = 3, RouteName = "أسوان", IsActive = true, CreatedAt = now, CreatedByUserId = 1 }
        );

        // Route 1: المنشية — 15 merchants
        var rmId = 1;
        for (int i = 1; i <= 15; i++)
        {
            mb.Entity<RouteMerchant>().HasData(
                new RouteMerchant { RouteMerchantId = rmId, RouteId = 1, MerchantId = i, PositionOrder = i, IsActive = true, AddedAt = now, AddedByUserId = 1 }
            );
            rmId++;
        }

        // Route 2: خريط — 10 merchants (IDs 1-10)
        for (int i = 1; i <= 10; i++)
        {
            mb.Entity<RouteMerchant>().HasData(
                new RouteMerchant { RouteMerchantId = rmId, RouteId = 2, MerchantId = i, PositionOrder = i, IsActive = true, AddedAt = now, AddedByUserId = 1 }
            );
            rmId++;
        }

        // Route 3: أسوان — 12 merchants (IDs 1-12)
        for (int i = 1; i <= 12; i++)
        {
            mb.Entity<RouteMerchant>().HasData(
                new RouteMerchant { RouteMerchantId = rmId, RouteId = 3, MerchantId = i, PositionOrder = i, IsActive = true, AddedAt = now, AddedByUserId = 1 }
            );
            rmId++;
        }

        // One completed delivery day for yesterday (Route 1, 8 merchants)
        var yesterday = now.AddDays(-1).Date;
        mb.Entity<DeliveryDay>().HasData(
            new DeliveryDay { DeliveryDayId = 1, RouteId = 1, DeliveryDate = yesterday, AssignedDriver = "أحمد محمود حسن", Status = "Confirmed", CreatedAt = yesterday, CreatedByUserId = 1, ConfirmedAt = yesterday.AddHours(10) }
        );

        // 8 invoices for the delivery day (merchants at positions 1-8 on Route 1)
        for (int i = 1; i <= 8; i++)
        {
            mb.Entity<DayInvoice>().HasData(
                new DayInvoice
                {
                    DayInvoiceId = i,
                    DeliveryDayId = 1,
                    RouteMerchantId = i, // RouteMerchant IDs 1-8 are positions 1-8 on Route 1
                    MerchantId = i,
                    InvoiceNumber = $"INV-{12370 + i}",
                    Quantity = $"{i + 1} كراتين",
                    Amount = 1500m + (i * 250),
                    EnteredByUserId = 1,
                    EnteredAt = yesterday.AddHours(8)
                }
            );
        }
    }
}
