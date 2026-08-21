using System.Windows;
using System.Windows.Controls;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Models;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class RouteHistoryPage : UserControl
{
    private readonly ApiClient _api;
    private bool _isInitialized = false;

    public RouteHistoryPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        LoadFiltersAsync();
        _isInitialized = true;
        LoadHistoryAsync();
    }

    private async void LoadFiltersAsync()
    {
        var routes = await _api.GetRoutesAsync();
        routes.Insert(0, new Models.Route { RouteId = 0, RouteName = "جميع الخطوط" });
        RouteFilterBox.ItemsSource = routes;
        RouteFilterBox.SelectedIndex = 0;
    }

    private async void LoadHistoryAsync()
    {
        if (!_isInitialized) return;

        HistoryGrid.Visibility = Visibility.Collapsed;
        LoadingText.Visibility = Visibility.Visible;

        int? routeId = null;
        if (RouteFilterBox.SelectedValue is int rId && rId > 0)
        {
            routeId = rId;
        }

        string? dateStr = null;
        if (DateFilterBox.SelectedDate.HasValue)
        {
            dateStr = DateFilterBox.SelectedDate.Value.ToString("yyyy-MM-dd");
        }

        var results = await _api.GetDeliveryDaysAsync(routeId, dateStr);
        HistoryGrid.ItemsSource = results;

        LoadingText.Visibility = Visibility.Collapsed;
        HistoryGrid.Visibility = Visibility.Visible;
    }

    private void Filter_Changed(object sender, SelectionChangedEventArgs e)
    {
        LoadHistoryAsync();
    }

    private void ClearFilters_Click(object sender, RoutedEventArgs e)
    {
        _isInitialized = false; // Prevent double load
        RouteFilterBox.SelectedIndex = 0;
        DateFilterBox.SelectedDate = null;
        _isInitialized = true;
        LoadHistoryAsync();
    }

    private async void PrintLoading_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is DeliveryDayItem item)
        {
            await PrintHelper.PrintSheetAsync(_api, item.DeliveryDayId, "loading");
        }
    }

    private async void PrintDelivery_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is DeliveryDayItem item)
        {
            await PrintHelper.PrintSheetAsync(_api, item.DeliveryDayId, "delivery");
        }
    }
}
