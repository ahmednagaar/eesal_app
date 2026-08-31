using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class ReceiptDashboardPage : UserControl
{
    private readonly ApiClient _api;

    public ReceiptDashboardPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        Loaded += async (_, _) => await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            // Load dashboard data
            var dashJson = await _api.GetDashboardJsonAsync();
            if (dashJson != null)
            {
                var dash = JObject.Parse(dashJson);
                StatTotalAmount.Text = (dash["totalAmount"]?.Value<decimal>() ?? 0).ToString("N0");
                StatDrivers.Text = (dash["totalDrivers"]?.Value<int>() ?? 0).ToString();
                StatReceipts.Text = (dash["totalReceipts"]?.Value<int>() ?? 0).ToString();
                StatOpenGaps.Text = (dash["openGaps"]?.Value<int>() ?? 0).ToString();

                // Load today's sessions
                var sessions = dash["todaySessions"] as JArray;
                if (sessions != null && sessions.Count > 0)
                {
                    var rows = sessions.Select(s => new
                    {
                        sessionId = s["sessionId"]?.Value<int>() ?? 0,
                        driverName = s["driverName"]?.ToString() ?? "",
                        routeArea = s["routeArea"]?.ToString() ?? "",
                        receiptRange = $"{s["firstReceiptNumber"]}–{s["lastReceiptNumber"]}",
                        totalReceiptsCount = s["totalReceiptsCount"]?.Value<int>() ?? 0,
                        totalAmountFormatted = (s["totalAmountCollected"]?.Value<decimal>() ?? 0).ToString("N2"),
                        gapStatus = (s["hasGaps"]?.Value<bool>() ?? false) ? "⚠️" : "✓",
                        importSource = s["importSource"]?.ToString() ?? "Manual"
                    }).ToList();

                    SessionsGrid.ItemsSource = rows;
                    EmptyState.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SessionsGrid.ItemsSource = null;
                    EmptyState.Visibility = Visibility.Visible;
                }
            }

            // Load missing drivers (after 3pm)
            if (DateTime.Now.Hour >= 15)
            {
                try
                {
                    var missingJson = await _api.GetMissingDriversTodayJsonAsync();
                    if (missingJson != null)
                    {
                        var missing = JArray.Parse(missingJson);
                        if (missing.Count > 0)
                        {
                            var names = missing.Select(d =>
                                $"{d["fullName"]} ({d["phoneNumber"]})").ToList();
                            MissingDriversText.Text = string.Join("  •  ", names);
                            MissingDriversBanner.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            MissingDriversBanner.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                catch
                {
                    MissingDriversBanner.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MissingDriversBanner.Visibility = Visibility.Collapsed;
            }

            // Load low-stock alerts (from book-series alerts)
            try
            {
                var alertsJson = await _api.GetSeriesAlertsJsonAsync();
                if (alertsJson != null)
                {
                    var alertsObj = JObject.Parse(alertsJson);
                    var alerts = alertsObj["alerts"] as JArray;
                    if (alerts != null && alerts.Count > 0)
                    {
                        var bookAlerts = alerts
                            .Where(a => a["type"]?.ToString() == "book_low" || a["type"]?.ToString() == "book_finished")
                            .Select(a => a["message"]?.ToString() ?? "")
                            .ToList();
                        if (bookAlerts.Count > 0)
                        {
                            LowStockText.Text = string.Join("  •  ", bookAlerts);
                            LowStockBanner.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            LowStockBanner.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        LowStockBanner.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch
            {
                LowStockBanner.Visibility = Visibility.Collapsed;
            }
        }
        catch (ApiUnauthorizedException)
        {
            MessageBox.Show("انتهت صلاحية الجلسة — يرجى تسجيل الدخول مرة أخرى",
                "انتهاء الجلسة", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            ErrorMessageHelper.LogError(ex);
            var msg = ErrorMessageHelper.GetArabicMessage(ex);
            ToastHelper.ShowError((Grid)Content, msg);
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadDashboardAsync();
    }

    private void SessionsGrid_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SessionsGrid.SelectedItem == null) return;
        dynamic row = SessionsGrid.SelectedItem;
        try
        {
            int sessionId = (int)row.sessionId;
            // Navigate to Sessions page — this will be wired when SessionsPage exists
            // For now, show the session ID in a toast
            ToastHelper.ShowInfo((Grid)Content, $"جلسة رقم {sessionId} — سيتم فتحها قريباً");
        }
        catch { }
    }
}
