-- =============================================================
-- Receipt System - Real Client Data Import (v2 - FIXED)
-- Generated: 2026-11-11 13:54:27
-- Source: Excel file with 945 active clients
-- Unique merchants: 936
-- Routes: 10
-- =============================================================
-- FIXES from v1:
--   1. Proper FK cascade: DayInvoices -> DeliveryDays -> RouteMerchants
--   2. Cleans up partial data from failed v1 run
--   3. Uses dynamic IDs (no hardcoded starting ID)
--   4. SET XACT_ABORT ON for atomic transaction
-- =============================================================

SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRANSACTION;

PRINT '=== STEP 1: Clean up partial v1 data ===';

-- Remove any RouteMerchant links created by v1 (IDs >= 100)
DELETE FROM RouteMerchants WHERE RouteMerchantId >= 100;

-- Remove merchants that v1 inserted (those after the original 20 seed merchants)
-- But first check: are there any FK references to these merchants?
-- Delete NoReceiptCollections, Receipts, etc. referencing merchants > 20
-- (These would only exist if the app was used after v1 partial import)
DELETE FROM AjalEntries WHERE MerchantId > 20;
DELETE FROM NoReceiptCollections WHERE MerchantId > 20;
-- GoodsReturns has no MerchantId column (uses CustomerName string) - skip
DELETE FROM ExcelImportRows WHERE MatchedMerchantId > 20;
DELETE FROM Receipts WHERE MerchantId > 20;
DELETE FROM DayInvoices WHERE MerchantId > 20;
DELETE FROM Merchants WHERE MerchantId > 20;

PRINT '=== STEP 2: Clean up ALL seed route data (proper FK order) ===';

-- Delete DayInvoices first (references RouteMerchants)
DELETE FROM DayInvoices;
-- Delete DeliveryDays (references Routes)
DELETE FROM DeliveryDays;
-- Now safe to delete RouteMerchants
DELETE FROM RouteMerchants;
-- Delete Ajal data referencing routes
DELETE FROM AjalEntries;
DELETE FROM AjalSessions;
-- Delete routes inserted by v1 (10-19) and seed routes (1-3)
DELETE FROM Routes;

-- Mark seed merchants as inactive
UPDATE Merchants SET IsActive = 0 WHERE MerchantId <= 20;

PRINT '=== STEP 3: Insert 10 real routes ===';

SET IDENTITY_INSERT Routes ON;
INSERT INTO Routes (RouteId, RouteName, IsActive, CreatedAt, CreatedByUserId) VALUES (10, N'الكاجوج', 1, GETDATE(), 1);
INSERT INTO Routes (RouteId, RouteName, IsActive, CreatedAt, CreatedByUserId) VALUES (11, N'المنشية', 1, GETDATE(), 1);
INSERT INTO Routes (RouteId, RouteName, IsActive, CreatedAt, CreatedByUserId) VALUES (12, N'اسوان', 1, GETDATE(), 1);
INSERT INTO Routes (RouteId, RouteName, IsActive, CreatedAt, CreatedByUserId) VALUES (13, N'الخطارة والجعافرة', 1, GETDATE(), 1);
INSERT INTO Routes (RouteId, RouteName, IsActive, CreatedAt, CreatedByUserId) VALUES (14, N'ادفو الشرق', 1, GETDATE(), 1);
INSERT INTO Routes (RouteId, RouteName, IsActive, CreatedAt, CreatedByUserId) VALUES (15, N'ادفو الغرب', 1, GETDATE(), 1);
INSERT INTO Routes (RouteId, RouteName, IsActive, CreatedAt, CreatedByUserId) VALUES (16, N'منيحه', 1, GETDATE(), 1);
INSERT INTO Routes (RouteId, RouteName, IsActive, CreatedAt, CreatedByUserId) VALUES (17, N'الغرب', 1, GETDATE(), 1);
INSERT INTO Routes (RouteId, RouteName, IsActive, CreatedAt, CreatedByUserId) VALUES (18, N'خريط', 1, GETDATE(), 1);
INSERT INTO Routes (RouteId, RouteName, IsActive, CreatedAt, CreatedByUserId) VALUES (19, N'كوم امبو', 1, GETDATE(), 1);
SET IDENTITY_INSERT Routes OFF;

PRINT '=== STEP 4: Insert 936 real merchants ===';

-- Create temp table to hold merchant data with a sequence key
CREATE TABLE #MerchantImport (
    SeqId INT IDENTITY(1,1) PRIMARY KEY,
    MerchantName NVARCHAR(200) NOT NULL,
    City NVARCHAR(100) NULL
);

INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم خليل الدكة', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم شحات الدكة', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو معاذ الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو هزاع كشتمنة الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد ايوب الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد منصور الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اسعد حربى الدكة', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج يسري الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام عبد الله شعريه الدكة', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسام عبد االله الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين الدكة', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حماده رزق الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عاصم شعبان الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الصبور محمد الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالرحمن الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالرحمن ماركت دكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'على احمد الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علي ابراهيم الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عماد الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عوض الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فرن الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فوزي عبدالوارث الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد فراج الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود عبدالقوى الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى محمود سيد الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'معاوية محمد الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ناصر صيام الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هبه احمد بسطاوي الدكه', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هناء رزق - الدكة', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'وائل فاروق الدكة', N'الدكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن يوسف الرغامه غرب', N'الرغامة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حماده فراج الرغامه غرب', N'الرغامة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سعدى عبدالحميد الرغامه غرب', N'الرغامة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الرازق عباس الرغامه غرب', N'الرغامة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود حسين الرغامة غرب', N'الرغامة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود مصطفى احمد الرغامه', N'الرغامة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هاشم الرغامه البلد', N'الرغامة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابوالحسن العريبي السلسه', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد بخيت السلسله', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الشيخ عبدالحميد السلسله', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سعد عبدالعال السلسله', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سليمان السلسلة', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلى عسكر السلسله', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عادل محمد االسلسله', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'على ادريس السلسه', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'على السلسله', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عيسى صلاح السلسله', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'كرم فخري السلسله', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ماركت محروس السلسله', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبدالراضى السلسلة', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود سيد السلسه', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ياسر محمود السلسله', N'السلسلة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن عبدالحفيظ الشبيكة', N'الشبيكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رمضان الشبيكه', N'الشبيكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عادل الحمزاوى الشبيكه', N'الشبيكة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم صالح الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عيسى الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام بدري منصور الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدرى الكيال الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدري التاجر الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'تقادم الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حربى ماضى الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن حيدر الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين ماركت الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حمادة ابو سناري الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شعبان ابو عوض الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صبري ابو الجيد الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'طارق الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'طه الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عادل الامين الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالمنعم المسيخ الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالناصر الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ماركت ال البيت الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد سيد الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد طه الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبد الفتاح الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود ابوقرين الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود الدقن الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود مصطفى الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى الظابط الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'يوسف شعبان الكاجوج', N'الكاجوج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'100ج زياده - ربيع محمد صالح سلوا بحري', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم بكر سلو', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو قصي سلو بحري', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو مالك سلو بحري', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد المهدي سلو', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد جمال سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عابدين سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عبد الحافظ المضيق سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج احمد العوري سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج محمد برتك سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج ناصر برتق سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بهاء سيف الله علي سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جمال ابو نصر سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حامد حسين ابرهيم سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حامد سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن محمد سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حماده ابوقرعه سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ربيع محمد صالح سلوا بحري', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سعد الحضري سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سوق الجملة سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سيد برتك سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الاله سعيد سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد العظيم سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالحفيظ اليمنى سلواا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عطارة محمد ابو طافش سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علاء العمده سلو', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علاء سلوا بحري', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علي ادم سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'كرار مصطفى الجمل سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عوض الله سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مطعم الجلاني سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مغربى سلوا', N'سلوا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد البدوي فطيرة', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد الفرحي فطيرة', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عبدالله فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عوض فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد موسى فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جعفر فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حماده فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عطارة المصطفي فطيرة', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد جمال فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد دهب فطيرة', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد محمود فطيرة', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود عبد الدايم فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود عبيدالله فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مسعود فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى الزهرء ماركت فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى طه فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفي الحمادي فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مهدي الخولي فطيره', N'فطيرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم كلابشه', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو رزان كلابشة', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد حسين كلابشه', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحسينى صالح كلابشة', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام مختار كلابشة', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جمال عبدالفتاح كلابشه', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شريف عبدالله كلابشة', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عادل شعبان كلابشه', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالحميد على كلابشة', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'على هانى كلابشة', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ماركت الامل والامانه كلابشة', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفي مرواو كلابشه', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'موسي كلابشه', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'يوسف الزين كلابشه', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'يوسف كلابشه', N'كلابشة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد ابو عثمان السلسله تبع اشرف ابوعثمان', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد المصراوي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد بشير', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عيسى داوود', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج هارون ارغامه غرب', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بلال الزين', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن ابوكباس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حمدي الشبكه', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سيد فتح الله', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صباح عبد العظيم ادم', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الرحيم الكاجوج (مزارع دواجن )', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالحميد عايد الحروبة', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالوهاب الكاجوجى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'على منصور محمد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عنتر ابو البس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ماركت التوحيد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد الكاجوجى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد حسن الزين', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود دابود', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'انس صلاح', N'at');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'على ابو خميسى', N'ha');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو الحسن سعيد العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابويوسف العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد اشرف العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الغب ابو عادل العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام مصطفى العتمور المستجد', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'امين بشير العتمور المستجد', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ايمن مصطفى العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدون ايصال - عبدالنبى فتح الله العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن بغدادى العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين حطب العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'خليفه محمود العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شريف العتمور قبلي', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالنبى فتح الله العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عرفه شحاته العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علي السوهاجي العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فارس شحات العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فتحي العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ابو عيد العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد احمد عيسى العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد حنفى العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى النوبى العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'منصور العتمور', N'العتمور');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد الطيرى المنشية', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عابدين المنشية', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد هباش المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اسراء عبدالصبور المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اشرف ابوعثمان المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حازم المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسام عنتر المنشية قبلي', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن الضاوي المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين الصياد المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حمدي امين المنشية', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'راضي بضون المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ركابى المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صالح المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'طارق محمود المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالرازق المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالله حساني المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالله كشك المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبده المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عز جمعه المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علاء صلاح المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'على علي المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عمر المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عمر عبدالله المنشه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عنتر ابو عليان المنشيه بحري', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ابراهيم المنشية', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد توفيق المنشية', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد جمال المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد حمدي المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد سعد المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد سيد عطيتوا المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد محمود المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى حسن المنشية', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفي ابو جاد المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'موسى المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'موسى عبدالموجود/المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'نور المنشيه', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هايبر الصفوة المنشية', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هايبر الوطنية المنشية', N'المنشية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو ايه حجازة', N'حجازة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابوالحسن حجازه', N'حجازة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد سعد حجازه', N'حجازة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدون ايصال - حسن حجازه', N'حجازة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن حجازه', N'حجازة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'متولى حجازة', N'حجازة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد حسين حجازه', N'حجازة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابوسيف نصر النوبة', N'نصر النوبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد شحتو نصر النوبة', N'نصر النوبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ايمن صالح نصر النوبة', N'نصر النوبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدون ايصال - معتز جملة ماركت نصر النوبة', N'نصر النوبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حامد نصر النوبه', N'نصر النوبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عامر حمدون نصر النوبة', N'نصر النوبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالرحيم نصر النوبه', N'نصر النوبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علاء الدين عبدالعزيز نصر النوبة', N'نصر النوبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد طلعت نصر النوبة', N'نصر النوبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'معتز جملة ماركت نصر النوبة', N'نصر النوبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'نصر الدين حسب الله نصر النوبة', N'نصر النوبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'يحى بكرى نصر النوبه', N'نصر النوبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم الشيباني', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابوندى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد ابوعابد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عبدالحميد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ادهم الشيبانى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'المهندس سيد عبدالرحمن', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'انور باشا', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسام عبدالصانع', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين احمد ابوشحاته', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلى ابو خرابه', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلى العجوز', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شعبان ابوهيلو', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صابر سرحان', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبده صابر سرحان', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عرفه الديب', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عزت عثمان', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علي القوصي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فخرانى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مجدي ابوداعه', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبدالقادر كلوس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود الريان', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود سرحان', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود عطارة السيف', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود كشك', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود هاشم', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مرعي المنشيه تموين', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مسلوب ابو وجيه', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مطعم الريان', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدون ايصال - قاسم يوسف ابو سمبل', N'ابو سمبل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج عشرى ابوالريش', N'ابوالريش');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام فرحة ابو الريش', N'ابوالريش');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسان ابو الريش', N'ابوالريش');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شفيق ابوالريش', N'ابوالريش');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مجدي ابوالريش', N'ابوالريش');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ربيع ابوالريش', N'ابوالريش');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد هلال ابو الريش', N'ابوالريش');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود حسن ابوالريش', N'ابوالريش');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى ابوالريش', N'ابوالريش');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابومريم اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد على اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اشرف الحلوانى اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'البرت اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام مخائيل اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'امير غرب اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدون ايصال - ابومريم اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدون ايصال - اشرف الحلوانى اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حامد اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رؤيه ماركت اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سعد اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سوبر ماركت سيدى بزار اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالله اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالناصر اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبده العطار اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'قاسم عطا اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبدالفتاح اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مركت الابيض اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'منتصر ابوالشيخ اسوان', N'اسوان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم عمر السيل', N'السيل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'خالد مصطفى السيل', N'السيل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رجب حسنين السيل', N'السيل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عمرو ابو اياد السيل', N'السيل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبيد السيل', N'السيل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حمدى ابو عبدالله كيما', N'كيما');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'د/محمد كيما', N'كيما');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود سعد كيما', N'كيما');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو انس جمال', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو ايه تيتو', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو علي صلاح', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابومعاذ عطارة الجهيني', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد ابوالنجا', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد البدرى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد الدالى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد الدشناوي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد بصل', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد توفيق', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد جمعه', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد على حنفي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد غانم الرضوان', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد محمود السمطى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج /احمد عبدالراضى الامير', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج سيد مكي الحريجي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج/ صبرى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج/محمد الامير', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ايمن ابوالحسن', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ايمن عطا', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جمال مجيد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جمعيه المقاولون العرب', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حربى الصداقة القديمة', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن حسب الله', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن فراج', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين السيد محمد احمد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حماده البس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حمدى عشور', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رجب عبدالشكور', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سعيد عبد الغنى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سيد حامد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صابر عبدالغفور', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صلاح ابوعداله', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عادل فخرى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالرازق ابوعداله', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالرحمن عبده ياسين', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبده صديق المطحن', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علاء ابوعداله', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علاء العدلي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عمر ابوعداله', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فتحى حسين', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فرج القفاص', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فرن احمد عبدالحافظ', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'قناوى الدمرداش', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد احمد شوقي الصداقه', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ادريس ابراهيم', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد المحموديه', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد المهندس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد سعد ابوالحسن', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبدلعزيز الشاذلى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد فاروق بهاريف', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد فراج الامير', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمدحواكه', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود ابومهاب', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود السلطان', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود الضبع', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود امين', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود ربيع', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود سعد ابوالحسن', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى العبادي الشادر', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى سالم', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى شرعى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مطعم الخديوى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مطعم السعد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مطعم تيتو', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مطعم قصر الشام', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'معتز ابوعداله', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هايبر زاد الخلا', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'وليد عبد الرازق', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ياسر احمد ابراهيم', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ياسين الدابودى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'خالد محمود العكرميه', N'rma');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد ابو معاذ الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد فتحى الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام جمال الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام طه الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ايهاب الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين احمد الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالعاطي جمال الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالله محمود الجعافرة', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'على الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عوض\الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد خالد الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد صالح الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد طه الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبد الحميد الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود جمال الجعافرة', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ناصر الجعافره', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هاشم عثمان', N'الجعافرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابانوب الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو بكر احمد الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد ابو ادريس الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد بريس الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد جمال الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد حامد الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد حسن ابو زرار الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد هاشم الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اكرم سعيد الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الشيخ حسين الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام سلمي الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام شمس الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام مريم الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام مصطفى الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'انور الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ايمن نجار الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بسطاوى الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'تعديل التاريخ . - مهدى حسين الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جرجس رماني الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حازم على الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حازم مرغنى بدوى الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن يوسف الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سعد طحونه الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سليم الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'طارق ابو ياسين الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عامر حسن البشاري الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالباسط الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عمر هارون الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فارس الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فاطمة عبدالباسط الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مجاهد خضر الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد الترعاوى الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد سعد الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عيد الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مدحت جمعيتى الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفي احمد حسين', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مهدى حسين الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'موريس الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'موسى الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مينا غالي الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ناصر الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'نصحي فؤاد الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'نوح الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ياسر سليمان الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'يوسف الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'يوسف صلاح احمد الخطارة', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'يوسف محمد الخطاره', N'الخطارة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'خالد ابوحريب العقبه', N'العقبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سمير احمد العقبه', N'العقبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'طه جامع العقبه', N'العقبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علاء ابومكه العقبه', N'العقبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فتحي حيدر العقبه', N'العقبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد العقبة', N'العقبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود نبيل العقبه', N'العقبة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم القفطيه', N'القفطية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابوالحسن محمد القفطية', N'القفطية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام مصطفى محمد القفطيه', N'القفطية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حامد سعيد القفطيه', N'القفطية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سعودي معبد القفطيه', N'القفطية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هشام القفطيه', N'القفطية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو حسن بلانه', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو خالد بلانه', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابويوسف بلانه', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عوض \بلانه', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جابر بحره بلانه', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سامي عبد الكريم بلانه', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شركة هوب للتوريدات بلانة', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'طه عادل بلانة', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالله بلانه', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عصام بلانة', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد بدوى بلانه', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد بلانه', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عوض ابو مؤمن بلانة', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى بلانه', N'بلانة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو مريم دراو', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابوالحسن على', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابوبكر القرشى', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد ابو كرار دراو', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد صغير دراو', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج ابراهيم عربي', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج احمد ابو جاد دراو', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج سعد الطويسى', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الشيخ احمد ابو النجا', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'امام محمد صالح', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ايمن منصور', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'برسي ابو شنوده دراو', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حجاج الصغير حسن', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حماده عبداللاه', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حميد سعيد', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'خالد السوداني', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سراج حسن', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سيد رضوان', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلى العربى', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلى الكرمي دراو', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلى سيف', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالرحمن الكرار دراو', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالكريم حسين دراو', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالله ابو عرقان', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عيد عبد الرسول', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مجدى عواض', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد الدسوقى', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد جمعه دراو', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد مصطفي شحات دراو', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد واحه البركه دراو', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود عبدالوهاب القوصى', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'وجدى الجزار', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'يوسف ناصر دراو', N'دراو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو اصالة الفيومى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد ادم', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد اسماعيل', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد حنفي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد سعد احمد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اسلام البس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اسماء الباين', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اشرف تايجر', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اشرف حسن ابوزرار', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الشافعي مصطفي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام احمد محمد صالح', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام خالد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ايه عشور احمد عبد الباسط', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'باب الريان /عبدالله محمود', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن البكري', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حماده هارون نجع ونس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رحمه العقبة الصغيرة', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رمضان برين', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سعد علي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شعبان جامع', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صدام التيربو', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صلاح عبدالحافظ', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عادل محمد ابراهيم', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عاشور عبدالباسط المقلا', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عامر مطر', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالحكيم الجعيدى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالله الرضاوى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علاء سعد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'على الكوبانيه', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عمرو رضاوي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مؤمن عبد الحكيم ابو زرار', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محسن سعد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ابراهيم الحجازي الشطب', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد العربى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد حازم', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد حسن ابوزرار', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد سلامه محمد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد شعبان', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد صلاح', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبدالغني', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد علي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد محمود ياسين', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود اسماعيل', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى عبدالستار', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى عبدالعظيم', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'وليد معروف', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم عابد ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اسلام الارديسه ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جبريل البدرى ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سالم ابو زيد ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلي ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علاء عبد الوهاب ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبد الطيف ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبد الوهاب ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عقده ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد نبيل الرتاج ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود حسين ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى العدوه ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى عويس ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'منصور زحل ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'نبيل موسى ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هاشم زهري ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حماده منصور اسنا', N'اسنا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جابر عبدالوهاب الرتاج', N'الرتاج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الراضي محمد الريتاج', N'الرتاج');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حمدي المفالسه', N'المفالسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سيد نقش المفالسه', N'المفالسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مينا زكري', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ياسر كلح الدماريه', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو عيس الطوناب', N'الطوناب');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد الطوناب', N'الطوناب');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رجب الطوناب', N'الطوناب');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فؤاد الطوناب', N'الطوناب');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فتحي الطوناب', N'الطوناب');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد الطوناب', N'الطوناب');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود ابوالحسن الطوناب', N'الطوناب');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هايبر لولو الطوناب', N'الطوناب');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هدرا الطوناب', N'الطوناب');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'وليد فتحى شابة الطوناب', N'الطوناب');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ياسر محمد الطوناب', N'الطوناب');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابانوب فكتور الارديسه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اشرف ودي االارديسه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج ابراهيم الارديسه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام روعه الادريسه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدري الارديسه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بشوي الاردسيه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جرجس الاردسيه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صلاح ابو رفاعي الارديسه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صلاح فخور الادريسه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالمطلب الاردسيه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'غريب الادريسه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مجدي الحيمري الارديسه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مجدي شحات الاردسيه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود علي الاردسيه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود منصور الارديسه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مريم صابر الردسية', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ممدوح الرديسيه', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مينا الرداسية', N'الارديسة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد محمود الفوزه', N'الفوزة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج جمعه رمضان الفوزه', N'الفوزة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ذ هبه نعيم عطا الله الفوزه', N'الفوزة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبد الله الفوزه', N'الفوزة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'نور رشيد الفوزه', N'الفوزة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم سليمان النزل', N'النزل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم يوسف النزل', N'النزل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد العسكري النزل', N'النزل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام كريم النزل', N'النزل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رمضان عبدالعليم النزل', N'النزل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ريمون اسحاق النزل', N'النزل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سيد النزل', N'النزل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عزب النزل', N'النزل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'على النزل', N'النزل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جرجس ذكى\ خور الزق', N'خور الزق');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'خالد خور الزق', N'خور الزق');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبدالله خور الزق', N'خور الزق');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ناصر خور الزق', N'خور الزق');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم حماده وادي عبادي 1', N'وادي عبادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام احمد وادي عبادي 3', N'وادي عبادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الله ودي عبادي 1', N'وادي عبادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ماركت مصطفي عبادي 3', N'وادي عبادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد حامد الودي عبادي 1', N'وادي عبادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابانوب حنا', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابانوب صدقى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم السنوسي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم الماذون', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابوجلال ماركت', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد اب علي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عبدالرازق', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اسماعيل عثمان ابوالليل', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الشيخ ابراهيم عوض', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'المجري ماركت', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام خالد هلال', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام يوسف المحاميد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'امير العطوانى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ايمن ودي عبادي 3 فرن', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بكر فراج', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بلال عوض', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جمال عدلي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حارس عوض الررتاج', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حريش', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رجب العطوني', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رزيق ذكي موسي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'زكريا عبدالسيد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سعد العلوي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلى ابو فريج', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عباس محمد عباس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد المجيد العطوانى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عطارة ابناء الامام الحسن', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عطاره ال البيت', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ماركت فيصل العطواني', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد الارديسه محج', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد العطواني', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد الوشاحى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد انور البحيره', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد نجار', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود عكاشه', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مدحت الشيمي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى ابومسلم', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ملاك البصلي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ميدو البحيره', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ميلاد يوسف سليمان خور ازق', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'وائل حامد العطواني', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ياسر ابو ادم', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'يحيى ابوحماده', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عبيد العدوه', N'العدوة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حماده العدوه', N'العدوة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الله العدوه', N'العدوة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عمر اسد العدوه', N'العدوة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ماركت عمر العدوه', N'العدوة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ادفو العدوه', N'العدوة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد مصطفي العدوه', N'العدوة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفي العربي العدوه', N'العدوة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شعبان ابوالغاب ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو جبر الله ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد البدري ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد صادق ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عادل ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج امجد صالح ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج محمد ابراهيم ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بسام سعيد ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسام ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن ابو رومي ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين حسن ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين عمر ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حماده زيدان ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رامز ابو الدهب ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صلاح الوكيل ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الله عيد ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالحفيظ ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالله نور ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عطاره محمود ابو بكر ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علاء احمد ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ماهر جرجس ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد توفيق ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مطعم الامبرطور ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'منصور ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هدرا جرجس ادفو', N'ادفو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محرم الحصايا', N'الحصايا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى الحصايا', N'الحصايا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'خميس مهلل كلح الجبل', N'كلح الجبل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رامي كلح الجبل', N'كلح الجبل');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم عثمان السباعه', N'السباعية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شعيب حاجر السباعيه', N'السباعية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عطاره ابو درقه السباعيه', N'السباعية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود عطاره السباعيه', N'السباعية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد حمدي البصليه', N'البصلية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدري البصليه', N'البصلية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'خالد علي البصليه', N'البصلية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سعيد البصليه', N'البصلية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علاء البصليه', N'البصلية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مجاهد الجدبي البصليه', N'البصلية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مومن العرموسي البصليه', N'البصلية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو بكر احمد الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد ابو عزون الرمادى', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد الرمادى', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد جمال الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عبادي الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عبده الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عمر الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الامين محمد الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحاج حسين ادم الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الشيخ مصطفي الجمل الرمادى', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بغدادي عابدين محمد الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حامد ابو عزون الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رجب صلاح الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سيد ابو زيد الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلي حميد الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلي معوض الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الروف الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الفتاح الزنيقه الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالله ابو كرملة الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عثمان القاضي الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عصام الرمادى', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبد السلام الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد يونس الرمادى', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود عسكري الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود محمد عبدالله الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفي عبدالمتعال الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'منصور ليثي الرمادي', N'الرمادي');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم العدوى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم القريطي الموسات', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد الكاشف', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد رمضان الكلحى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد سعد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اشرف سمير جبل', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الحسينى الباجا', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الهام علي - نجع ابوالنصر', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ايمن سمير', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدري ابو شباب', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حامد الشحتابي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسام ابراهيم الحباري', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن الشحتابى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن سليم', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن محمد احمد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين بدوي حسب الرب', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين عبدو الرغامه بلد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'دسوقي امين صادق الكلح', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد العزيز زغلول', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الله ابو كرم الله', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فارس ابوعقده', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود ابراهيم كلنتون', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ناجح عدلى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ياسر المصري', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد مصطفى الكيال', N'ar');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اشرف متولى', N'ar');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'توماس محروس', N'ar');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم محسن العطواني', N'ass');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد اشرف ابريم', N'ابريم');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد مصطفى الكيال', N'ارضيه');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد اسنا طفنيس', N'اسنا');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد يوسف اسوان الجديده', N'اسوان الجديدة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالرحمن الطيري النجاجره', N'النجاجرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'وليد الطيب النجاجرة', N'النجاجرة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عباس النصريه', N'النصرية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'وهبى النصريه', N'النصرية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن كلح الدقاديق', N'كلح الدقاديق');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام هشام نجع الحجر', N'نجع الحجر');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مزمل نجع الحجر', N'نجع الحجر');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'- عطاره الخير', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالراضي منصور', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'- محمد العياط', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابراهيم النجار', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو احمد عنتر', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام مهدى دهميت شارع 11', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ايهاب صابر', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حمام الشارع الجديد', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سالم الملاحظ الزمالك', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ابو عقده', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سيد السنبك', N'ارضيه');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ضياء الدكر اقليت', N'اقليت');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ابو الشيخ اقليت', N'اقليت');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابواسلام منيحه', N'منيحة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد ابراهيم احمد منيحة', N'منيحة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد الريح منيحة', N'منيحة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الشيخ محمد منيحه', N'منيحة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'رفعت محمود كرار منيحة', N'منيحة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الرحمن صلاح منيحة', N'منيحة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد حسين منيحه', N'منيحة');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'راغب عابدين', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سيد ابوالجود', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد حسين ماركة المرتاح', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد ربيع المنصوريه', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد عبدالتواب المنصوريه', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد موسى المنصوريه', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الشيخ حسين المنصورية', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين زيدان المنصوريه', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عثمان العقبى المنصورية', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عصام مرعي المنصورية', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علي حسب الله المنصورية', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مبارك جبر المنصوريه', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ابو الحندي المنصورية', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد خير سعيد المنصورية', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد زيدان المنصورية', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عوض المنصورية', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود سلامه المنصوريه', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود شعبان المنصورية', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ياسر الغرباوي المنصورية', N'المنصورية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'5000ج فكه تحصيل قديم - هانى عبد المعين بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابانوب كرم بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو وليد بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد العسيرى بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد النجار بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد النكد بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد قرشى بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اسامه منظفات بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'الشيخ زكريا / بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام احمد بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام عبد الله بهلول بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام عمر بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام ياسين بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ايليا جميل بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بخيت ساويرس بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدوي نبيل بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جمال السيد كحول بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حافظ بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسن عبدالفتاح بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حسين السمان بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حماده القاضي بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سليم عبدالمنعم بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سيد ابو الشيخ بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلى بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلى هاشم بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صابر بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صالح السلواوى بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'صبحي رحمة بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عادل بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عامر البيلي بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عامر صديق بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالله السعودي بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عثمان بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'علاء عبد الجواد بنبان بحرى', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'على ابو خضر بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عمر الجهني بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عمر بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عمر ترك بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فرن الحجازيه بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فوزى محمود حامد بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'كريم بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'كيرلس شوقى بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ماركت اولاد عز بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مجاهد عبد الرحيم بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد الامين الصراف بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد برسي بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد حسن بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عبدالقادر بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد فراج بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد فوزى بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود حامد بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى العسكر بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى جمعه بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'نبيل عبدالمنعم بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هاشم جبريل بنبان بحرى', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هانى عبد المعين بنبان', N'بنبان');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد ابو رفاعي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ام حسام فارس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'جرجس عيسي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شادلي ايوب', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'طه حبيب الله', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الرحمن حبيب الله', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الرحمن فارس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبدالفتاح الصحار', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'على ابوغرابي', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'فتح الله عبد الحفيظ', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'كريم حمزة فارس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ماركت الغزال فارس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ابوطالب فارس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد الصراف', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمود ابوسيف كاش', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفي عبادي ابوير', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'يحيي فارس', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'حمزه ابوعوده', N'العباسية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سعيد غنيم', N'العباسية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عرفه عبدالموجود', N'العباسية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عماد العباسيه', N'العباسية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ابو العمده', N'العباسية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى عبادى', N'العباسية');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد مهليل خريط', N'خريط');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شاذلي ابو دهيس خريط', N'خريط');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'طه البشاري خريط', N'خريط');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبد الفتاح حماد خريط', N'خريط');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ابو عجلان خريط', N'خريط');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد ابو فاضل خريط', N'خريط');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد البشارى خريط', N'خريط');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد الدريني خريط', N'خريط');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'هشام عبد الرسول خريط', N'خريط');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'اشرف عبد الغنى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'بدوى حسين', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'مصطفى عبد الشكور', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ياسر الشاذلى', NULL);
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابو يوسف هايبر سكلنس', N'كوم امبو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'ابوالحسن نوح', N'كوم امبو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'احمد محمود فرن الاصلاح', N'كوم امبو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'سمير كياس', N'كوم امبو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'شنوده', N'كوم امبو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عبده عاطف', N'كوم امبو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'عربي دكان بورسعيد', N'كوم امبو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'متى', N'كوم امبو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد الموالدى', N'كوم امبو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد جابر العدوه كوم امبو', N'كوم امبو');
INSERT INTO #MerchantImport (MerchantName, City) VALUES (N'محمد عطيتو', N'كوم امبو');

-- Bulk insert from temp table into Merchants (identity auto-assigns MerchantId)
INSERT INTO Merchants (MerchantName, City, IsActive, CreatedAt)
SELECT MerchantName, City, 1, GETDATE()
FROM #MerchantImport
ORDER BY SeqId;

-- Build mapping: SeqId -> actual MerchantId
-- Since we inserted in SeqId order and MerchantId is identity, they line up
CREATE TABLE #MerchantIdMap (
    SeqId INT,
    MerchantId INT
);

-- Use ROW_NUMBER to match the insertion order
;WITH Numbered AS (
    SELECT MerchantId, MerchantName, City,
           ROW_NUMBER() OVER (ORDER BY MerchantId) AS RowNum
    FROM Merchants
    WHERE MerchantId > 20
)
INSERT INTO #MerchantIdMap (SeqId, MerchantId)
SELECT n.RowNum, n.MerchantId
FROM Numbered n;

PRINT '=== STEP 5: Insert RouteMerchant associations ===';

-- Route: الكاجوج (RouteId=10) - 170 merchants
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 1, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 1;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 2, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 2;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 3, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 3;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 4, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 4;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 5, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 5;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 6, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 6;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 7, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 7;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 8, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 8;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 9, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 9;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 10, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 10;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 11, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 11;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 12, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 12;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 13, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 13;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 14, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 14;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 15, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 15;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 16, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 16;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 17, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 17;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 18, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 18;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 19, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 19;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 20, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 20;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 21, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 21;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 22, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 22;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 23, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 23;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 24, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 24;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 25, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 25;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 26, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 26;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 27, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 27;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 28, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 28;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 29, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 29;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 30, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 30;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 31, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 31;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 32, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 32;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 33, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 33;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 34, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 34;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 35, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 35;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 36, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 36;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 37, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 37;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 38, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 38;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 39, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 39;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 40, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 40;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 41, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 41;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 42, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 42;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 43, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 43;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 44, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 44;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 45, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 45;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 46, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 46;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 47, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 47;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 48, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 48;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 49, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 49;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 50, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 50;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 51, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 51;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 52, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 52;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 53, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 53;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 54, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 54;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 55, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 55;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 56, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 56;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 57, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 57;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 58, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 58;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 59, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 59;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 60, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 60;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 61, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 61;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 62, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 62;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 63, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 63;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 64, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 64;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 65, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 65;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 66, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 66;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 67, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 67;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 68, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 68;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 69, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 69;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 70, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 70;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 71, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 71;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 72, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 72;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 73, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 73;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 74, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 74;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 75, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 75;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 76, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 76;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 77, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 77;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 78, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 78;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 79, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 79;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 80, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 80;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 81, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 81;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 82, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 82;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 83, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 83;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 84, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 84;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 85, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 85;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 86, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 86;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 87, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 87;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 88, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 88;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 89, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 89;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 90, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 90;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 91, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 91;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 92, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 92;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 93, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 93;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 94, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 94;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 95, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 95;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 96, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 96;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 97, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 97;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 98, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 98;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 99, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 99;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 100, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 100;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 101, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 101;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 102, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 102;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 103, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 103;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 104, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 104;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 105, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 105;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 106, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 106;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 107, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 107;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 108, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 108;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 109, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 109;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 110, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 110;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 111, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 111;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 112, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 112;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 113, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 113;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 114, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 114;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 115, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 115;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 116, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 116;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 117, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 117;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 118, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 118;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 119, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 119;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 120, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 120;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 121, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 121;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 122, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 122;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 123, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 123;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 124, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 124;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 125, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 125;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 126, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 126;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 127, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 127;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 128, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 128;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 129, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 129;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 130, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 130;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 131, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 131;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 132, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 132;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 133, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 133;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 134, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 134;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 135, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 135;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 136, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 136;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 137, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 137;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 138, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 138;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 139, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 139;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 140, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 140;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 141, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 141;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 142, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 142;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 143, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 143;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 144, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 144;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 145, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 145;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 146, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 146;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 147, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 147;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 148, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 148;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 149, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 149;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 150, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 150;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 151, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 151;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 152, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 152;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 153, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 153;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 154, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 154;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 155, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 155;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 156, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 156;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 157, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 157;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 158, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 158;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 159, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 159;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 160, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 160;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 161, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 161;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 162, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 162;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 163, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 163;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 164, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 164;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 165, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 165;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 166, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 166;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 167, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 167;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 168, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 168;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 169, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 169;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 10, m.MerchantId, 170, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 170;

-- Route: المنشية (RouteId=11) - 110 merchants
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 1, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 171;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 2, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 172;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 3, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 173;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 4, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 174;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 5, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 175;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 6, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 176;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 7, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 177;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 8, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 178;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 9, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 179;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 10, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 180;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 11, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 181;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 12, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 182;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 13, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 183;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 14, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 184;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 15, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 185;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 16, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 186;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 17, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 187;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 18, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 188;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 19, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 189;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 20, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 190;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 21, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 191;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 22, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 192;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 23, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 193;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 24, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 194;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 25, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 195;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 26, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 196;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 27, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 197;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 28, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 198;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 29, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 199;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 30, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 200;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 31, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 201;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 32, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 202;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 33, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 203;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 34, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 204;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 35, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 205;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 36, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 206;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 37, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 207;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 38, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 208;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 39, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 209;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 40, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 210;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 41, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 211;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 42, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 212;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 43, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 213;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 44, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 214;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 45, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 215;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 46, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 216;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 47, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 217;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 48, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 218;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 49, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 219;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 50, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 220;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 51, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 221;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 52, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 222;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 53, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 223;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 54, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 224;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 55, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 225;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 56, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 226;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 57, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 227;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 58, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 228;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 59, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 229;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 60, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 230;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 61, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 231;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 62, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 232;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 63, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 233;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 64, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 234;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 65, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 235;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 66, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 236;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 67, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 237;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 68, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 238;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 69, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 239;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 70, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 240;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 71, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 241;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 72, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 242;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 73, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 243;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 74, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 244;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 75, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 245;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 76, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 246;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 77, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 247;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 78, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 248;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 79, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 249;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 80, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 250;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 81, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 251;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 82, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 252;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 83, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 253;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 84, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 254;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 85, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 255;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 86, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 256;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 87, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 257;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 88, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 258;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 89, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 259;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 90, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 260;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 91, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 261;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 92, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 262;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 93, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 263;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 94, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 264;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 95, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 265;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 96, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 266;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 97, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 267;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 98, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 268;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 99, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 269;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 100, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 270;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 101, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 271;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 102, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 272;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 103, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 273;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 104, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 274;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 105, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 275;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 106, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 276;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 107, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 277;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 108, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 278;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 109, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 279;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 11, m.MerchantId, 110, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 280;

-- Route: اسوان (RouteId=12) - 110 merchants
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 1, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 257;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 2, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 281;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 3, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 282;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 4, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 283;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 5, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 284;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 6, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 285;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 7, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 286;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 8, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 287;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 9, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 288;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 10, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 289;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 11, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 290;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 12, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 291;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 13, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 292;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 14, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 293;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 15, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 294;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 16, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 295;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 17, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 296;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 18, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 297;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 19, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 298;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 20, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 299;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 21, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 300;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 22, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 301;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 23, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 302;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 24, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 303;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 25, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 304;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 26, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 305;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 27, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 306;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 28, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 307;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 29, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 308;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 30, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 309;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 31, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 310;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 32, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 311;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 33, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 312;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 34, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 313;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 35, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 314;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 36, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 315;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 37, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 316;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 38, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 317;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 39, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 318;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 40, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 319;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 41, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 320;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 42, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 321;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 43, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 322;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 44, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 323;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 45, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 324;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 46, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 325;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 47, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 326;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 48, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 327;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 49, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 328;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 50, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 329;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 51, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 330;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 52, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 331;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 53, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 332;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 54, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 333;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 55, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 334;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 56, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 335;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 57, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 336;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 58, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 337;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 59, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 338;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 60, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 339;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 61, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 340;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 62, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 341;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 63, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 342;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 64, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 343;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 65, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 344;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 66, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 345;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 67, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 346;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 68, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 347;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 69, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 348;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 70, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 349;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 71, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 350;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 72, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 351;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 73, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 352;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 74, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 353;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 75, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 354;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 76, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 355;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 77, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 356;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 78, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 357;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 79, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 358;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 80, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 359;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 81, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 360;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 82, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 361;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 83, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 362;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 84, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 363;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 85, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 364;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 86, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 365;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 87, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 366;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 88, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 367;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 89, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 368;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 90, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 369;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 91, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 370;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 92, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 371;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 93, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 372;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 94, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 373;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 95, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 374;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 96, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 375;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 97, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 376;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 98, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 377;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 99, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 378;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 100, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 379;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 101, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 380;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 102, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 381;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 103, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 382;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 104, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 383;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 105, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 384;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 106, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 385;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 107, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 386;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 108, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 387;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 109, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 388;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 12, m.MerchantId, 110, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 389;

-- Route: الخطارة والجعافرة (RouteId=13) - 173 merchants
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 1, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 390;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 2, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 391;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 3, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 392;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 4, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 393;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 5, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 394;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 6, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 395;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 7, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 396;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 8, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 397;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 9, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 398;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 10, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 399;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 11, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 400;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 12, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 401;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 13, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 402;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 14, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 403;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 15, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 404;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 16, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 405;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 17, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 406;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 18, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 407;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 19, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 408;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 20, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 409;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 21, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 410;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 22, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 411;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 23, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 412;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 24, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 413;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 25, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 414;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 26, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 415;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 27, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 416;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 28, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 417;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 29, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 418;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 30, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 419;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 31, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 420;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 32, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 421;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 33, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 422;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 34, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 423;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 35, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 424;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 36, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 425;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 37, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 426;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 38, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 427;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 39, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 428;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 40, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 429;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 41, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 430;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 42, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 431;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 43, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 432;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 44, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 433;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 45, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 434;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 46, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 435;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 47, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 436;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 48, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 437;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 49, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 438;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 50, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 439;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 51, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 440;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 52, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 441;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 53, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 442;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 54, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 443;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 55, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 444;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 56, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 445;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 57, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 446;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 58, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 447;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 59, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 448;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 60, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 449;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 61, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 450;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 62, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 451;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 63, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 452;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 64, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 453;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 65, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 454;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 66, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 455;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 67, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 456;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 68, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 457;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 69, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 458;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 70, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 459;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 71, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 460;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 72, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 461;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 73, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 462;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 74, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 463;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 75, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 464;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 76, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 465;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 77, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 466;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 78, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 467;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 79, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 468;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 80, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 469;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 81, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 470;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 82, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 471;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 83, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 472;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 84, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 473;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 85, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 474;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 86, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 475;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 87, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 476;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 88, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 477;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 89, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 478;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 90, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 479;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 91, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 480;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 92, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 481;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 93, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 482;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 94, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 483;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 95, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 484;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 96, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 485;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 97, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 486;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 98, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 487;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 99, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 488;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 100, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 489;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 101, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 490;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 102, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 491;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 103, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 492;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 104, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 493;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 105, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 494;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 106, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 495;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 107, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 496;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 108, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 497;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 109, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 498;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 110, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 499;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 111, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 500;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 112, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 501;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 113, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 502;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 114, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 503;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 115, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 504;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 116, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 505;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 117, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 506;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 118, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 507;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 119, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 508;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 120, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 509;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 121, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 510;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 122, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 511;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 123, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 512;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 124, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 513;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 125, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 514;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 126, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 515;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 127, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 516;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 128, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 517;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 129, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 518;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 130, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 519;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 131, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 520;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 132, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 521;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 133, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 522;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 134, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 523;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 135, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 524;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 136, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 525;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 137, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 526;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 138, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 527;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 139, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 528;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 140, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 529;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 141, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 530;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 142, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 531;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 143, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 532;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 144, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 533;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 145, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 534;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 146, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 535;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 147, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 536;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 148, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 537;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 149, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 538;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 150, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 539;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 151, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 540;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 152, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 541;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 153, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 542;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 154, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 543;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 155, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 544;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 156, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 545;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 157, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 546;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 158, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 547;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 159, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 548;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 160, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 549;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 161, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 550;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 162, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 551;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 163, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 552;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 164, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 553;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 165, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 554;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 166, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 555;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 167, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 556;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 168, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 557;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 169, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 558;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 170, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 559;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 171, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 560;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 172, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 561;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 13, m.MerchantId, 173, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 562;

-- Route: ادفو الشرق (RouteId=14) - 126 merchants
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 1, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 563;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 2, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 564;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 3, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 565;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 4, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 566;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 5, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 567;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 6, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 568;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 7, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 569;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 8, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 570;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 9, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 571;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 10, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 572;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 11, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 573;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 12, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 574;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 13, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 575;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 14, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 576;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 15, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 577;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 16, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 578;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 17, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 579;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 18, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 580;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 19, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 581;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 20, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 582;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 21, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 583;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 22, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 584;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 23, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 585;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 24, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 586;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 25, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 587;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 26, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 588;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 27, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 589;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 28, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 590;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 29, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 591;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 30, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 592;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 31, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 593;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 32, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 594;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 33, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 595;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 34, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 596;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 35, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 597;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 36, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 598;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 37, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 599;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 38, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 600;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 39, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 601;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 40, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 602;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 41, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 603;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 42, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 604;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 43, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 605;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 44, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 606;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 45, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 607;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 46, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 608;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 47, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 609;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 48, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 610;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 49, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 611;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 50, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 612;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 51, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 613;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 52, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 614;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 53, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 615;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 54, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 616;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 55, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 617;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 56, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 618;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 57, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 619;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 58, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 620;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 59, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 621;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 60, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 622;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 61, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 623;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 62, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 624;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 63, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 625;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 64, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 626;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 65, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 627;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 66, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 628;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 67, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 629;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 68, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 630;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 69, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 631;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 70, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 632;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 71, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 633;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 72, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 634;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 73, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 635;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 74, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 636;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 75, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 637;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 76, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 638;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 77, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 639;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 78, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 640;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 79, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 641;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 80, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 642;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 81, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 643;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 82, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 644;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 83, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 645;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 84, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 646;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 85, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 647;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 86, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 648;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 87, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 649;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 88, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 650;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 89, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 651;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 90, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 652;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 91, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 653;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 92, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 654;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 93, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 655;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 94, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 656;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 95, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 657;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 96, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 658;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 97, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 659;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 98, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 660;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 99, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 661;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 100, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 662;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 101, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 663;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 102, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 664;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 103, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 665;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 104, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 666;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 105, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 667;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 106, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 668;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 107, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 669;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 108, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 670;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 109, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 671;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 110, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 672;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 111, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 673;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 112, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 674;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 113, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 675;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 114, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 676;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 115, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 677;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 116, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 678;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 117, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 679;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 118, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 680;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 119, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 681;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 120, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 682;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 121, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 683;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 122, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 684;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 123, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 685;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 124, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 686;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 125, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 687;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 14, m.MerchantId, 126, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 688;

-- Route: ادفو الغرب (RouteId=15) - 91 merchants
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 1, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 689;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 2, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 690;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 3, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 691;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 4, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 692;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 5, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 693;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 6, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 694;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 7, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 695;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 8, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 696;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 9, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 697;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 10, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 698;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 11, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 699;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 12, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 700;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 13, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 701;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 14, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 702;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 15, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 703;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 16, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 704;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 17, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 705;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 18, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 706;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 19, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 707;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 20, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 708;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 21, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 709;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 22, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 710;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 23, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 711;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 24, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 712;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 25, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 713;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 26, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 714;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 27, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 715;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 28, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 716;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 29, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 717;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 30, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 718;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 31, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 719;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 32, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 720;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 33, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 721;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 34, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 722;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 35, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 723;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 36, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 724;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 37, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 725;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 38, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 726;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 39, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 727;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 40, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 728;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 41, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 729;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 42, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 730;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 43, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 731;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 44, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 732;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 45, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 733;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 46, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 734;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 47, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 735;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 48, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 736;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 49, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 737;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 50, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 738;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 51, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 739;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 52, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 740;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 53, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 741;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 54, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 742;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 55, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 743;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 56, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 744;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 57, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 745;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 58, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 746;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 59, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 747;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 60, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 748;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 61, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 749;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 62, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 750;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 63, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 751;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 64, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 752;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 65, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 753;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 66, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 754;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 67, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 755;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 68, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 756;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 69, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 757;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 70, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 758;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 71, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 759;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 72, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 760;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 73, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 761;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 74, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 762;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 75, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 763;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 76, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 764;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 77, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 765;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 78, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 766;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 79, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 767;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 80, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 768;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 81, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 769;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 82, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 770;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 83, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 771;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 84, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 772;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 85, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 773;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 86, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 774;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 87, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 775;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 88, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 776;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 89, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 777;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 90, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 778;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 15, m.MerchantId, 91, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 779;

-- Route: منيحه (RouteId=16) - 13 merchants
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 1, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 805;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 2, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 806;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 3, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 807;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 4, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 808;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 5, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 809;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 6, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 810;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 7, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 811;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 8, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 812;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 9, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 813;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 10, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 814;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 11, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 815;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 12, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 816;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 16, m.MerchantId, 13, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 817;

-- Route: الغرب (RouteId=17) - 89 merchants
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 1, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 818;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 2, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 819;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 3, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 820;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 4, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 821;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 5, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 822;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 6, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 823;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 7, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 824;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 8, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 825;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 9, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 826;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 10, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 827;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 11, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 828;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 12, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 829;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 13, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 830;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 14, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 831;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 15, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 832;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 16, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 833;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 17, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 834;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 18, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 835;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 19, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 836;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 20, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 837;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 21, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 838;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 22, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 839;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 23, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 840;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 24, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 841;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 25, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 842;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 26, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 843;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 27, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 844;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 28, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 845;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 29, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 846;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 30, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 847;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 31, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 848;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 32, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 849;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 33, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 850;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 34, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 851;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 35, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 852;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 36, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 853;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 37, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 854;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 38, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 855;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 39, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 856;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 40, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 857;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 41, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 858;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 42, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 859;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 43, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 860;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 44, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 861;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 45, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 862;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 46, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 863;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 47, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 864;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 48, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 865;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 49, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 866;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 50, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 867;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 51, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 868;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 52, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 869;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 53, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 870;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 54, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 871;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 55, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 872;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 56, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 873;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 57, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 874;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 58, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 875;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 59, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 876;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 60, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 877;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 61, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 878;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 62, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 879;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 63, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 880;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 64, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 881;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 65, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 882;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 66, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 883;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 67, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 884;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 68, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 885;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 69, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 886;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 70, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 887;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 71, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 888;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 72, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 889;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 73, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 890;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 74, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 891;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 75, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 892;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 76, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 893;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 77, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 894;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 78, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 895;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 79, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 896;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 80, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 897;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 81, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 898;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 82, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 899;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 83, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 900;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 84, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 901;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 85, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 902;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 86, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 903;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 87, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 904;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 88, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 905;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 17, m.MerchantId, 89, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 906;

-- Route: خريط (RouteId=18) - 19 merchants
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 1, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 907;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 2, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 908;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 3, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 909;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 4, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 910;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 5, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 911;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 6, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 912;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 7, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 913;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 8, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 914;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 9, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 915;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 10, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 916;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 11, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 917;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 12, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 918;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 13, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 919;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 14, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 920;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 15, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 921;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 16, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 922;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 17, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 923;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 18, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 924;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 18, m.MerchantId, 19, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 925;

-- Route: كوم امبو (RouteId=19) - 11 merchants
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 19, m.MerchantId, 1, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 926;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 19, m.MerchantId, 2, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 927;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 19, m.MerchantId, 3, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 928;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 19, m.MerchantId, 4, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 929;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 19, m.MerchantId, 5, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 930;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 19, m.MerchantId, 6, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 931;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 19, m.MerchantId, 7, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 932;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 19, m.MerchantId, 8, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 933;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 19, m.MerchantId, 9, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 934;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 19, m.MerchantId, 10, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 935;
INSERT INTO RouteMerchants (RouteId, MerchantId, PositionOrder, IsActive, AddedAt, AddedByUserId)
SELECT 19, m.MerchantId, 11, 1, GETDATE(), 1
FROM #MerchantIdMap m WHERE m.SeqId = 936;

-- Clean up temp tables
DROP TABLE #MerchantIdMap;
DROP TABLE #MerchantImport;

PRINT '=== VERIFICATION ===';

SELECT 'Routes' AS [Table], COUNT(*) AS [Count] FROM Routes WHERE IsActive = 1;
SELECT 'Active Merchants' AS [Table], COUNT(*) AS [Count] FROM Merchants WHERE IsActive = 1;
SELECT 'Inactive Seed Merchants' AS [Table], COUNT(*) AS [Count] FROM Merchants WHERE IsActive = 0;
SELECT 'RouteMerchants' AS [Table], COUNT(*) AS [Count] FROM RouteMerchants WHERE IsActive = 1;

SELECT r.RouteName, COUNT(rm.MerchantId) AS MerchantCount
FROM Routes r
LEFT JOIN RouteMerchants rm ON r.RouteId = rm.RouteId AND rm.IsActive = 1
WHERE r.IsActive = 1
GROUP BY r.RouteName
ORDER BY MerchantCount DESC;

PRINT '=== ALL DONE! ===';

-- =============================================================
-- SUMMARY
-- Routes inserted: 10
-- Merchants inserted: 936
-- RouteMerchant links: 912
-- =============================================================

COMMIT TRANSACTION;

-- If something went wrong, XACT_ABORT will auto-rollback.
-- You can also manually run: ROLLBACK TRANSACTION;
