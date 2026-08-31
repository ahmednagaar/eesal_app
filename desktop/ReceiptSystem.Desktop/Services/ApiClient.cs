using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace ReceiptSystem.Desktop.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private string? _token;
    private DateTime _tokenExpiresAt = DateTime.MinValue;
    private bool _isRefreshing = false;

    // In-memory placeholder — always overwritten by LoginWindow from AppSettings
    // before any API call is made. Never used as a live fallback.
    public string BaseUrl { get; set; } = "http://localhost:5000/api";
    public string? CurrentUserName { get; private set; }
    public string? CurrentUserRole { get; private set; }
    public int CurrentUserId { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(_token);

    public ApiClient()
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private void SetAuth()
    {
        if (!string.IsNullOrEmpty(_token))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);
    }

    // ══════════════════════════════════════
    // Auth
    // ══════════════════════════════════════

    public async Task<bool> LoginAsync(string username, string password)
    {
        var body = new { Username = username, Password = password };
        var response = await PostAsync("auth/login", body);
        if (response == null) return false;

        try
        {
            _token = response["token"]?.ToString();
            CurrentUserName = response["fullName"]?.ToString() ?? username;
            CurrentUserRole = response["role"]?.ToString();
            if (int.TryParse(response["userId"]?.ToString(), out int uid))
                CurrentUserId = uid;
        }
        catch
        {
            // Fallback: try PascalCase property names
            try
            {
                _token = response["Token"]?.ToString();
                CurrentUserName = response["FullName"]?.ToString() ?? username;
                CurrentUserRole = response["Role"]?.ToString();
                if (int.TryParse(response["UserId"]?.ToString(), out int uid2))
                    CurrentUserId = uid2;
            }
            catch
            {
                return false;
            }
        }

        if (string.IsNullOrEmpty(_token)) return false;

        // Parse token expiry (JWT standard: 8h from login by default)
        try
        {
            var exp = response["expiresAt"]?.ToString() ?? response["ExpiresAt"]?.ToString();
            DateTime parsedExpiry = DateTime.MinValue;
            if (!string.IsNullOrEmpty(exp) && DateTime.TryParse(exp, out parsedExpiry))
                _tokenExpiresAt = parsedExpiry.ToUniversalTime();
            else
                _tokenExpiresAt = DateTime.UtcNow.AddHours(8); // fallback
        }
        catch
        {
            _tokenExpiresAt = DateTime.UtcNow.AddHours(8);
        }

        SetAuth();
        return true;
    }

    public async Task<bool> RefreshTokenAsync()
    {
        if (_isRefreshing) return true;
        _isRefreshing = true;
        try
        {
            var response = await PostAsync("auth/refresh", new { });
            if (response == null) return false;
            var newToken = response["token"]?.ToString() ?? response["Token"]?.ToString();
            if (string.IsNullOrEmpty(newToken)) return false;
            _token = newToken;

            var exp = response["expiresAt"]?.ToString() ?? response["ExpiresAt"]?.ToString();
            DateTime parsedExpiry = DateTime.MinValue;
            if (!string.IsNullOrEmpty(exp) && DateTime.TryParse(exp, out parsedExpiry))
                _tokenExpiresAt = parsedExpiry.ToUniversalTime();
            else
                _tokenExpiresAt = DateTime.UtcNow.AddHours(8);

            SetAuth();
            return true;
        }
        catch { return false; }
        finally { _isRefreshing = false; }
    }

    /// <summary>
    /// Called after every successful authenticated API call.
    /// If token expires within 30 minutes, trigger a background refresh.
    /// </summary>
    private void CheckTokenExpiryAndRefresh()
    {
        if (_tokenExpiresAt == DateTime.MinValue || _isRefreshing) return;
        var remaining = _tokenExpiresAt - DateTime.UtcNow;
        if (remaining.TotalMinutes < 30)
        {
            // Fire-and-forget refresh — next call will use the new token
            _ = RefreshTokenAsync();
        }
    }

    public async Task<bool> CheckConnectionAsync()
    {
        try
        {
            var result = await GetAsync("auth/me");
            return result != null;
        }
        catch { return false; }
    }

    // ══════════════════════════════════════
    // Drivers
    // ══════════════════════════════════════

    public async Task<string?> GetDriversJsonAsync()
    {
        return await GetAsync("drivers");
    }

    public async Task<dynamic?> CreateDriverAsync(string fullName, string phoneNumber, string? notes)
    {
        return await PostAsync("drivers", new { FullName = fullName, PhoneNumber = phoneNumber, Notes = notes ?? "" });
    }

    public async Task<bool> UpdateDriverAsync(int id, string fullName, string phoneNumber, string? notes)
    {
        return await PutAsync($"drivers/{id}", new { FullName = fullName, PhoneNumber = phoneNumber, Notes = notes ?? "" });
    }

    public async Task<bool> DeleteDriverAsync(int id)
    {
        return await DeleteAsync($"drivers/{id}");
    }

    // ══════════════════════════════════════
    // Merchants
    // ══════════════════════════════════════

    public async Task<string?> GetMerchantsJsonAsync(int page = 1, int pageSize = 50)
    {
        return await GetAsync($"merchants?page={page}&pageSize={pageSize}");
    }

    public async Task<string?> SearchMerchantsJsonAsync(string query)
    {
        return await GetAsync($"merchants/search?q={Uri.EscapeDataString(query)}");
    }

    public async Task<dynamic?> CreateMerchantAsync(string name, string? city, string? phone, string? notes)
    {
        return await PostAsync("merchants", new { MerchantName = name, City = city, PhoneNumber = phone, Notes = notes });
    }

    public async Task<bool> UpdateMerchantAsync(int id, string name, string? city, string? phone, string? notes)
    {
        return await PutAsync($"merchants/{id}", new { MerchantName = name, City = city, PhoneNumber = phone, Notes = notes });
    }

    // ══════════════════════════════════════
    // Routes
    // ══════════════════════════════════════

    public async Task<List<Models.Route>> GetRoutesAsync()
    {
        var json = await GetAsync("routes");
        if (json == null) return new List<Models.Route>();
        return JsonConvert.DeserializeObject<List<Models.Route>>(json) ?? new List<Models.Route>();
    }

    public async Task<dynamic?> CreateRouteAsync(string routeName, string? notes = null)
    {
        return await PostAsync("routes", new { RouteName = routeName, Notes = notes ?? "" });
    }

    public async Task<bool> UpdateRouteAsync(int id, string routeName, string? notes = null)
    {
        return await PutAsync($"routes/{id}", new { RouteName = routeName, Notes = notes ?? "" });
    }

    public async Task<bool> DeleteRouteAsync(int id)
    {
        return await DeleteAsync($"routes/{id}");
    }

    public async Task<List<Models.RouteMerchant>> GetRouteMerchantsAsync(int routeId)
    {
        var json = await GetAsync($"routes/{routeId}/merchants");
        if (json == null) return new List<Models.RouteMerchant>();
        return JsonConvert.DeserializeObject<List<Models.RouteMerchant>>(json) ?? new List<Models.RouteMerchant>();
    }

    public async Task<dynamic?> AddMerchantToRouteAsync(int routeId, int merchantId, int position)
    {
        return await PostAsync($"routes/{routeId}/merchants", new { MerchantId = merchantId, Position = position });
    }

    public async Task<bool> UpdateMerchantPositionAsync(int routeId, int routeMerchantId, int newPosition)
    {
        return await PutAsync($"routes/{routeId}/merchants/{routeMerchantId}/position", new { NewPosition = newPosition });
    }

    public async Task<bool> RemoveMerchantFromRouteAsync(int routeId, int routeMerchantId)
    {
        return await DeleteAsync($"routes/{routeId}/merchants/{routeMerchantId}");
    }

    public async Task<List<Models.MerchantSearchResult>> SearchMerchantsForRouteAsync(string query)
    {
        var json = await GetAsync($"merchants/search?q={Uri.EscapeDataString(query)}");
        if (json == null) return new List<Models.MerchantSearchResult>();
        return JsonConvert.DeserializeObject<List<Models.MerchantSearchResult>>(json) ?? new List<Models.MerchantSearchResult>();
    }

    // ══════════════════════════════════════
    // Delivery Days
    // ══════════════════════════════════════

    public async Task<List<Models.DeliveryDayItem>> GetDeliveryDaysAsync(int? routeId = null, string? date = null)
    {
        var query = new List<string>();
        if (routeId.HasValue) query.Add($"routeId={routeId}");
        if (!string.IsNullOrEmpty(date)) query.Add($"date={date}");
        
        string q = query.Count > 0 ? "?" + string.Join("&", query) : "";
        var json = await GetAsync($"delivery-days{q}");
        
        if (json == null) return new List<Models.DeliveryDayItem>();
        return JsonConvert.DeserializeObject<List<Models.DeliveryDayItem>>(json) ?? new List<Models.DeliveryDayItem>();
    }

    public async Task<dynamic?> CreateDeliveryDayAsync(int routeId, DateTime date, string? driver = null)
    {
        var body = new { RouteId = routeId, DeliveryDate = date.ToString("yyyy-MM-dd"), AssignedDriver = driver };
        return await PostAsync("delivery-days", body);
    }

    public async Task<dynamic?> GetDeliveryDayAsync(int id)
    {
        var json = await GetAsync($"delivery-days/{id}");
        if (json == null) return null;
        return JsonConvert.DeserializeObject<dynamic>(json);
    }

    public async Task<List<dynamic>> GetDeliveryDaysAsync(DateTime? date = null, int? routeId = null)
    {
        var query = new List<string>();
        if (date.HasValue) query.Add($"date={date.Value:yyyy-MM-dd}");
        if (routeId.HasValue) query.Add($"routeId={routeId.Value}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
        var json = await GetAsync($"delivery-days{qs}");
        if (json == null) return new List<dynamic>();
        return JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();
    }

    // ══════════════════════════════════════
    // Day Invoices
    // ══════════════════════════════════════

    public async Task<dynamic?> AddDayInvoiceAsync(int dayId, int routeMerchantId, int merchantId,
        string? invoiceNumber = null, string? quantity = null, decimal? amount = null, string? notes = null)
    {
        var body = new
        {
            RouteMerchantId = routeMerchantId,
            MerchantId = merchantId,
            InvoiceNumber = invoiceNumber ?? "",
            Quantity = quantity ?? "",
            Amount = amount,
            Notes = notes ?? ""
        };
        return await PostAsync($"delivery-days/{dayId}/invoices", body);
    }

    public async Task<bool> UpdateDayInvoiceAsync(int dayId, int invoiceId,
        string? invoiceNumber, string? quantity, decimal? amount, string? notes)
    {
        var body = new { InvoiceNumber = invoiceNumber ?? "", Quantity = quantity ?? "", Amount = amount, Notes = notes ?? "" };
        return await PutAsync($"delivery-days/{dayId}/invoices/{invoiceId}", body);
    }

    public async Task<bool> RemoveDayInvoiceAsync(int dayId, int invoiceId)
    {
        return await DeleteAsync($"delivery-days/{dayId}/invoices/{invoiceId}");
    }

    public async Task<bool> ReorderDayInvoicesAsync(int dayId, List<object> reorders)
    {
        return await PutAsync($"delivery-days/{dayId}/reorder", reorders);
    }

    // ══════════════════════════════════════
    // Print Sheets
    // ══════════════════════════════════════

    public async Task<dynamic?> GetLoadingSheetAsync(int dayId)
    {
        var json = await GetAsync($"delivery-days/{dayId}/loading-sheet");
        if (json == null) return null;
        return JsonConvert.DeserializeObject<dynamic>(json);
    }

    public async Task<dynamic?> GetDeliverySheetAsync(int dayId)
    {
        var json = await GetAsync($"delivery-days/{dayId}/delivery-sheet");
        if (json == null) return null;
        return JsonConvert.DeserializeObject<dynamic>(json);
    }

    // ══════════════════════════════════════
    // Sessions (Module 1)
    // ══════════════════════════════════════

    public async Task<string?> GetSessionsJsonAsync(int page = 1, int pageSize = 20, string? date = null, int? driverId = null)
    {
        var q = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrEmpty(date)) q.Add($"date={date}");
        if (driverId.HasValue) q.Add($"driverId={driverId}");
        return await GetAsync($"sessions?{string.Join("&", q)}");
    }

    public async Task<string?> GetTodaySessionsJsonAsync()
    {
        return await GetAsync("sessions/today");
    }

    public async Task<string?> GetSessionDetailJsonAsync(int sessionId)
    {
        return await GetAsync($"sessions/{sessionId}");
    }

    public async Task<string?> GetLastSessionForDriverJsonAsync(int driverId)
    {
        return await GetAsync($"sessions/driver/{driverId}/last");
    }

    public async Task<dynamic?> CreateSessionAsync(object dto)
    {
        return await PostAsync("sessions", dto);
    }

    public async Task<bool> ConfirmSessionAsync(int sessionId)
    {
        return await PutAsync($"sessions/{sessionId}/confirm", new { });
    }

    public async Task<dynamic?> UnlockSessionAsync(int sessionId, string reason)
    {
        return await PutWithResponseAsync($"sessions/{sessionId}/unlock", new { Reason = reason });
    }

    public async Task<dynamic?> ReconcileSessionAsync(int sessionId, decimal actualCashCounted)
    {
        return await PutWithResponseAsync($"sessions/{sessionId}/reconcile", new { ActualCashCounted = actualCashCounted });
    }

    public async Task<string?> GetMissingDriversTodayJsonAsync()
    {
        return await GetAsync("sessions/missing-today");
    }

    // ══════════════════════════════════════
    // Receipts (Module 1)
    // ══════════════════════════════════════

    public async Task<string?> SearchReceiptJsonAsync(int receiptNumber)
    {
        return await GetAsync($"receipts/search?number={receiptNumber}");
    }

    public async Task<string?> CheckDuplicateJsonAsync(int receiptNumber)
    {
        return await GetAsync($"receipts/check-duplicate/{receiptNumber}");
    }

    public async Task<bool> UpdateReceiptAsync(int receiptId, object dto)
    {
        return await PutAsync($"receipts/{receiptId}", dto);
    }

    public async Task<bool> DeleteReceiptAsync(int receiptId)
    {
        return await DeleteAsync($"receipts/{receiptId}");
    }

    // ══════════════════════════════════════
    // Gaps (Module 1)
    // ══════════════════════════════════════

    public async Task<string?> GetGapsJsonAsync(string? status = null)
    {
        var q = !string.IsNullOrEmpty(status) ? $"?status={status}" : "";
        return await GetAsync($"gaps{q}");
    }

    public async Task<string?> GetGapSummaryJsonAsync()
    {
        return await GetAsync("gaps/summary");
    }

    public async Task<bool> ResolveGapAsync(int gapId, string status, string resolution)
    {
        return await PutAsync($"gaps/{gapId}/resolve", new { Status = status, Resolution = resolution });
    }

    // ══════════════════════════════════════
    // Receipt Books (Module 1)
    // ══════════════════════════════════════



    public async Task<string?> GetAvailableBooksJsonAsync()
    {
        return await GetAsync("books/available");
    }



    public async Task<bool> AssignBookAsync(int bookId, int driverId, DateTime? assignedDate = null)
    {
        return await PutAsync($"books/{bookId}/assign", new { DriverId = driverId, AssignedDate = assignedDate?.ToString("yyyy-MM-dd") });
    }

    public async Task<dynamic?> AssignBooksBatchAsync(int[] bookIds, int driverId, DateTime? assignedDate = null)
    {
        return await PostAsync("books/assign-batch", new { BookIds = bookIds, DriverId = driverId, AssignedDate = assignedDate?.ToString("yyyy-MM-dd") });
    }

    public async Task<bool> UpdateBookAsync(int bookId, int? bookNumber = null, string? notes = null)
    {
        return await PutAsync($"books/{bookId}", new { BookNumber = bookNumber, Notes = notes });
    }

    public async Task<bool> DeactivateBookAsync(int bookId)
    {
        return await DeleteAsync($"books/{bookId}");
    }



    // ══════════════════════════════════════
    // Book Series
    // ══════════════════════════════════════

    public async Task<string?> GetSeriesListJsonAsync()
    {
        return await GetAsync("book-series");
    }

    public async Task<dynamic?> CreateSeriesAsync(string seriesCode, int totalBooks = 500, int receiptsPerBook = 50, int startReceiptNumber = 1, string? notes = null)
    {
        return await PostAsync("book-series", new
        {
            SeriesCode = seriesCode,
            TotalBooks = totalBooks,
            ReceiptsPerBook = receiptsPerBook,
            StartReceiptNumber = startReceiptNumber,
            Notes = notes
        });
    }

    public async Task<string?> GetSeriesDetailJsonAsync(int seriesId)
    {
        return await GetAsync($"book-series/{seriesId}");
    }

    public async Task<bool> CompleteSeriesAsync(int seriesId)
    {
        return await PutAsync($"book-series/{seriesId}/complete", new { });
    }

    public async Task<string?> GetSeriesAlertsJsonAsync()
    {
        return await GetAsync("book-series/alerts");
    }

    public async Task<bool> DeleteSeriesAsync(int seriesId)
    {
        return await DeleteAsync($"book-series/{seriesId}");
    }

    public async Task<dynamic?> AssignNextBookAsync(int seriesId, int driverId, DateTime? assignedDate = null)
    {
        return await PostAsync($"book-series/{seriesId}/assign-next", new { DriverId = driverId, AssignedDate = assignedDate?.ToString("yyyy-MM-dd") });
    }

    // ── Book Lifecycle (Return & Verify) ──

    public async Task<bool> ReturnBookAsync(int bookId, string? notes = null)
    {
        return await PutAsync($"books/{bookId}/return", new { Notes = notes });
    }

    public async Task<bool> VerifyBookAsync(int bookId)
    {
        return await PutAsync($"books/{bookId}/verify", new { });
    }

    public async Task<string?> GetBookHistoryJsonAsync(int bookId)
    {
        return await GetAsync($"books/{bookId}/history");
    }

    // ══════════════════════════════════════
    // ERP Import (Module 1)
    // ══════════════════════════════════════

    public async Task<dynamic?> UploadErpPreviewAsync(string filePath)
    {
        return await PostMultipartAsync("erp-import/preview", filePath);
    }

    public async Task<dynamic?> CreateErpBatchAsync(object dto)
    {
        return await PostAsync("erp-import/batches", dto);
    }

    public async Task<string?> GetErpBatchesJsonAsync(string? status = null, string? date = null)
    {
        var q = new List<string>();
        if (!string.IsNullOrEmpty(status)) q.Add($"status={status}");
        if (!string.IsNullOrEmpty(date)) q.Add($"date={date}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return await GetAsync($"erp-import/batches{qs}");
    }

    public async Task<string?> GetErpBatchDetailJsonAsync(int batchId)
    {
        return await GetAsync($"erp-import/batches/{batchId}");
    }

    public async Task<dynamic?> AssignErpBlockAsync(int batchId, object dto)
    {
        return await PostAsync($"erp-import/batches/{batchId}/assign-block", dto);
    }

    public async Task<dynamic?> AssignErpSingleAsync(int batchId, object dto)
    {
        return await PostAsync($"erp-import/batches/{batchId}/assign-single", dto);
    }

    public async Task<dynamic?> AddErpManualRowAsync(int batchId, object dto)
    {
        return await PostAsync($"erp-import/batches/{batchId}/add-manual-row", dto);
    }

    public async Task<dynamic?> ReconcileErpBatchAsync(int batchId, decimal ledgerTotalAmount, decimal actualCashTotal)
    {
        return await PutWithResponseAsync($"erp-import/batches/{batchId}/reconcile",
            new { LedgerTotalAmount = ledgerTotalAmount, ActualCashTotal = actualCashTotal });
    }

    public async Task<dynamic?> CompleteErpBatchAsync(int batchId)
    {
        return await PutWithResponseAsync($"erp-import/batches/{batchId}/complete", new { });
    }

    // ══════════════════════════════════════
    // Reports (Module 1)
    // ══════════════════════════════════════

    public async Task<string?> GetDailyReportJsonAsync(string? date = null)
    {
        var q = !string.IsNullOrEmpty(date) ? $"?date={date}" : "";
        return await GetAsync($"reports/daily{q}");
    }

    public async Task<string?> GetDashboardJsonAsync()
    {
        return await GetAsync("reports/dashboard");
    }

    public async Task<string?> GetDriverPerformanceJsonAsync(string from, string to)
    {
        return await GetAsync($"reports/driver-performance?from={from}&to={to}");
    }

    // ══════════════════════════════════════
    // Search & Export (Module 1)
    // ══════════════════════════════════════

    public async Task<string?> SearchReceiptsJsonAsync(string? merchantName = null, int? driverId = null,
        string? dateFrom = null, string? dateTo = null, int? receiptNumber = null,
        decimal? amountMin = null, decimal? amountMax = null, string? importSource = null,
        int page = 1, int pageSize = 50)
    {
        var q = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrEmpty(merchantName)) q.Add($"merchantName={Uri.EscapeDataString(merchantName)}");
        if (driverId.HasValue) q.Add($"driverId={driverId}");
        if (!string.IsNullOrEmpty(dateFrom)) q.Add($"dateFrom={dateFrom}");
        if (!string.IsNullOrEmpty(dateTo)) q.Add($"dateTo={dateTo}");
        if (receiptNumber.HasValue) q.Add($"receiptNumber={receiptNumber}");
        if (amountMin.HasValue) q.Add($"amountMin={amountMin}");
        if (amountMax.HasValue) q.Add($"amountMax={amountMax}");
        if (!string.IsNullOrEmpty(importSource)) q.Add($"importSource={importSource}");
        return await GetAsync($"search/receipts?{string.Join("&", q)}");
    }

    public async Task<byte[]?> ExportReceiptsAsync(string? merchantName = null, int? driverId = null,
        string? dateFrom = null, string? dateTo = null, int? receiptNumber = null,
        decimal? amountMin = null, decimal? amountMax = null, string? importSource = null)
    {
        var q = new List<string>();
        if (!string.IsNullOrEmpty(merchantName)) q.Add($"merchantName={Uri.EscapeDataString(merchantName)}");
        if (driverId.HasValue) q.Add($"driverId={driverId}");
        if (!string.IsNullOrEmpty(dateFrom)) q.Add($"dateFrom={dateFrom}");
        if (!string.IsNullOrEmpty(dateTo)) q.Add($"dateTo={dateTo}");
        if (receiptNumber.HasValue) q.Add($"receiptNumber={receiptNumber}");
        if (amountMin.HasValue) q.Add($"amountMin={amountMin}");
        if (amountMax.HasValue) q.Add($"amountMax={amountMax}");
        if (!string.IsNullOrEmpty(importSource)) q.Add($"importSource={importSource}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return await GetBytesAsync($"search/receipts/export{qs}");
    }

    public async Task<string?> GetMerchantPaymentHistoryJsonAsync(int merchantId, string? from = null, string? to = null)
    {
        var q = new List<string>();
        if (!string.IsNullOrEmpty(from)) q.Add($"from={from}");
        if (!string.IsNullOrEmpty(to)) q.Add($"to={to}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return await GetAsync($"search/merchants/{merchantId}/payment-history{qs}");
    }

    // ══════════════════════════════════════
    // HTTP Helpers
    // ══════════════════════════════════════

    private async Task<string?> GetAsync(string endpoint)
    {
        try
        {
            var response = await _http.GetAsync($"{BaseUrl}/{endpoint}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new ApiUnauthorizedException();
            if (!response.IsSuccessStatusCode) return null;
            CheckTokenExpiryAndRefresh();
            return await response.Content.ReadAsStringAsync();
        }
        catch (ApiUnauthorizedException) { throw; }
        catch { return null; }
    }

    private async Task<dynamic?> PostAsync(string endpoint, object body)
    {
        try
        {
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{BaseUrl}/{endpoint}", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new ApiUnauthorizedException();
            
            if (!response.IsSuccessStatusCode)
            {
                // Try to extract error message from response
                try
                {
                    var errorObj = JsonConvert.DeserializeObject<dynamic>(result);
                    string? errorMsg = errorObj?["message"]?.ToString();
                    if (!string.IsNullOrEmpty(errorMsg))
                        throw new Exception(errorMsg);
                }
                catch (JsonException) { }
                
                return null;
            }
            
            CheckTokenExpiryAndRefresh();
            return JsonConvert.DeserializeObject<dynamic>(result);
        }
        catch (ApiUnauthorizedException) { throw; }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(ex.Message, ex);
        }
    }

    private async Task<bool> PutAsync(string endpoint, object body)
    {
        try
        {
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PutAsync($"{BaseUrl}/{endpoint}", content);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new ApiUnauthorizedException();
            if (!response.IsSuccessStatusCode)
            {
                // Extract error message for PUT failures too
                var result = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = JsonConvert.DeserializeObject<dynamic>(result);
                    string? errorMsg = errorObj?["message"]?.ToString();
                    if (!string.IsNullOrEmpty(errorMsg))
                        throw new Exception(errorMsg);
                }
                catch (JsonException) { }
            }
            CheckTokenExpiryAndRefresh();
            return response.IsSuccessStatusCode;
        }
        catch (ApiUnauthorizedException) { throw; }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw;
        }
        catch { return false; }
    }

    private async Task<dynamic?> PutWithResponseAsync(string endpoint, object body)
    {
        try
        {
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PutAsync($"{BaseUrl}/{endpoint}", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new ApiUnauthorizedException();

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorObj = JsonConvert.DeserializeObject<dynamic>(result);
                    string? errorMsg = errorObj?["message"]?.ToString();
                    if (!string.IsNullOrEmpty(errorMsg))
                        throw new Exception(errorMsg);
                }
                catch (JsonException) { }
                return null;
            }

            CheckTokenExpiryAndRefresh();
            return JsonConvert.DeserializeObject<dynamic>(result);
        }
        catch (ApiUnauthorizedException) { throw; }
        catch (HttpRequestException ex) { throw new HttpRequestException(ex.Message, ex); }
    }

    private async Task<dynamic?> PostMultipartAsync(string endpoint, string filePath)
    {
        try
        {
            using var form = new MultipartFormDataContent();
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            var response = await _http.PostAsync($"{BaseUrl}/{endpoint}", form);
            var result = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new ApiUnauthorizedException();

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorObj = JsonConvert.DeserializeObject<dynamic>(result);
                    string? errorMsg = errorObj?["message"]?.ToString()
                        ?? errorObj?["error"]?.ToString();
                    if (!string.IsNullOrEmpty(errorMsg))
                        throw new Exception(errorMsg);
                }
                catch (JsonException) { }
                return null;
            }

            CheckTokenExpiryAndRefresh();
            return JsonConvert.DeserializeObject<dynamic>(result);
        }
        catch (ApiUnauthorizedException) { throw; }
        catch (HttpRequestException ex) { throw new HttpRequestException(ex.Message, ex); }
    }

    private async Task<byte[]?> GetBytesAsync(string endpoint)
    {
        try
        {
            var response = await _http.GetAsync($"{BaseUrl}/{endpoint}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new ApiUnauthorizedException();
            if (!response.IsSuccessStatusCode) return null;
            CheckTokenExpiryAndRefresh();
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (ApiUnauthorizedException) { throw; }
        catch { return null; }
    }

    private async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await _http.DeleteAsync($"{BaseUrl}/{endpoint}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new ApiUnauthorizedException();
            CheckTokenExpiryAndRefresh();
            return response.IsSuccessStatusCode;
        }
        catch (ApiUnauthorizedException) { throw; }
        catch { return false; }
    }
}
