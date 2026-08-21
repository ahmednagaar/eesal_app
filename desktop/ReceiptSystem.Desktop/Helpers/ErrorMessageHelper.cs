using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Helpers;

/// <summary>
/// Translates .NET exceptions into user-friendly Arabic messages
/// for display to non-technical staff. Technical details are preserved
/// separately for troubleshooting.
/// </summary>
public static class ErrorMessageHelper
{
    /// <summary>
    /// Returns a concise, Arabic-language description of the error
    /// suitable for display in a MessageBox to non-technical users.
    /// </summary>
    public static string GetArabicMessage(Exception ex)
    {
        // Unwrap wrapper exceptions to find the real cause
        var inner = ex.InnerException ?? ex;

        return inner switch
        {
            ApiUnauthorizedException =>
                "انتهت صلاحية الجلسة — يرجى تسجيل الدخول مرة أخرى",
            HttpRequestException =>
                "تعذر الاتصال بالسيرفر — تحقق من اتصال الشبكة أو إعدادات السيرفر",
            TaskCanceledException or OperationCanceledException =>
                "انتهت مهلة الاتصال بالسيرفر — حاول مرة أخرى",
            JsonException or JsonReaderException or JsonSerializationException =>
                "حدث خطأ في قراءة البيانات من السيرفر",
            IOException =>
                "حدث خطأ في القراءة/الكتابة على القرص",
            _ => ex switch
            {
                // Also check the outer exception type
                ApiUnauthorizedException =>
                    "انتهت صلاحية الجلسة — يرجى تسجيل الدخول مرة أخرى",
                HttpRequestException =>
                    "تعذر الاتصال بالسيرفر — تحقق من اتصال الشبكة أو إعدادات السيرفر",
                _ => "حدث خطأ غير متوقع"
            }
        };
    }

    /// <summary>
    /// Formats the full exception chain for logging or an
    /// expandable "technical details" section.
    /// </summary>
    public static string GetTechnicalDetails(Exception ex)
    {
        var details = $"{ex.GetType().Name}: {ex.Message}";
        if (ex.InnerException != null)
            details += $"\n↳ {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";

        var stackLine = ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim();
        if (!string.IsNullOrEmpty(stackLine))
            details += $"\n📍 {stackLine}";

        return details;
    }

    /// <summary>
    /// Writes error details to a local log file for support purposes.
    /// Logs to %AppData%/ReceiptSystemDesktop/logs/errors.log
    /// </summary>
    public static void LogError(Exception ex)
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var logDir = Path.Combine(appData, "ReceiptSystemDesktop", "logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            var logPath = Path.Combine(logDir, "errors.log");
            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().Name}: {ex.Message}\n";
            if (ex.InnerException != null)
                entry += $"  Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}\n";
            entry += $"  Stack: {ex.StackTrace}\n---\n";

            File.AppendAllText(logPath, entry);
        }
        catch
        {
            // Logging should never cause a secondary crash
        }
    }
}
