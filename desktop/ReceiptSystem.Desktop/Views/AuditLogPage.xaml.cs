using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class AuditLogPage : UserControl
{
    private readonly ApiClient _api;
    private int _currentPage = 1;
    private int _totalCount = 0;
    private const int PageSize = 50;

    public AuditLogPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        DateFrom.SelectedDate = DateTime.Today.AddDays(-7);
        DateTo.SelectedDate = DateTime.Today;
        Loaded += async (_, _) => await SearchAsync();
    }

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        _currentPage = 1;
        await SearchAsync();
    }

    private async void Clear_Click(object sender, RoutedEventArgs e)
    {
        DateFrom.SelectedDate = DateTime.Today.AddDays(-7);
        DateTo.SelectedDate = DateTime.Today;
        EntityTypeCombo.SelectedIndex = 0;
        ActionFilter.Text = "";
        _currentPage = 1;
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            string? entityType = null;
            if (EntityTypeCombo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
                entityType = tag;

            string? dateFrom = DateFrom.SelectedDate?.ToString("yyyy-MM-dd");
            string? dateTo = DateTo.SelectedDate?.ToString("yyyy-MM-dd");
            string? action = string.IsNullOrWhiteSpace(ActionFilter.Text) ? null : ActionFilter.Text.Trim();

            var json = await _api.GetAuditLogsJsonAsync(
                entityType: entityType,
                action: action,
                dateFrom: dateFrom,
                dateTo: dateTo,
                page: _currentPage,
                pageSize: PageSize);

            if (json == null)
            {
                AuditGrid.ItemsSource = null;
                PageInfoText.Text = "لا توجد نتائج";
                return;
            }

            var result = JObject.Parse(json);
            _totalCount = result["totalCount"]?.Value<int>() ?? 0;
            var results = result["results"] as JArray;

            if (results != null)
            {
                var rows = results.Select(r => new
                {
                    CreatedAt = r["createdAt"]?.Value<DateTime>().ToString("yyyy-MM-dd HH:mm") ?? "",
                    UserName = r["userName"]?.ToString() ?? "",
                    Action = r["action"]?.ToString() ?? "",
                    EntityType = r["entityType"]?.ToString() ?? "",
                    EntityId = r["entityId"]?.Value<int>() ?? 0,
                    IpAddress = r["ipAddress"]?.ToString() ?? "",
                    NewValues = r["newValues"]?.ToString() ?? ""
                }).ToList();

                AuditGrid.ItemsSource = rows;
            }

            UpdatePagination();
        }
        catch (ApiUnauthorizedException)
        {
            MessageBox.Show("انتهت صلاحية الجلسة — يرجى تسجيل الدخول مرة أخرى",
                "انتهاء الجلسة", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            var msg = ErrorMessageHelper.GetArabicMessage(ex);
            ToastHelper.ShowError((Grid)Content, msg);
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdatePagination()
    {
        int totalPages = Math.Max(1, (int)Math.Ceiling((double)_totalCount / PageSize));
        PageInfoText.Text = $"صفحة {_currentPage} من {totalPages} — إجمالي {_totalCount} سجل";
        PrevBtn.IsEnabled = _currentPage > 1;
        NextBtn.IsEnabled = _currentPage < totalPages;
    }

    private async void PrevPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            await SearchAsync();
        }
    }

    private async void NextPage_Click(object sender, RoutedEventArgs e)
    {
        int totalPages = Math.Max(1, (int)Math.Ceiling((double)_totalCount / PageSize));
        if (_currentPage < totalPages)
        {
            _currentPage++;
            await SearchAsync();
        }
    }
}
