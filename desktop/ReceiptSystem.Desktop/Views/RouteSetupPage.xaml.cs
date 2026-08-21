using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ReceiptSystem.Desktop.Models;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class RouteSetupPage : UserControl
{
    private readonly ApiClient _api;
    private ObservableCollection<Models.Route> _routes = new();
    private ObservableCollection<RouteMerchant> _merchants = new();
    private List<RouteMerchant> _allMerchantsCache = new(); // For local filtering
    
    private Models.Route? _selectedRoute;

    public RouteSetupPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        LoadRoutesAsync();
    }

    // ══════════════════════════════════════
    // ROUTES LIST
    // ══════════════════════════════════════

    private async void LoadRoutesAsync()
    {
        var list = await _api.GetRoutesAsync();
        _routes = new ObservableCollection<Models.Route>(list);
        RoutesList.ItemsSource = _routes;
        
        EmptyRoutesState.Visibility = _routes.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ShowAddRouteBtn_Click(object sender, RoutedEventArgs e)
    {
        ShowAddRouteBtn.Visibility = Visibility.Collapsed;
        AddRoutePanel.Visibility = Visibility.Visible;
        NewRouteNameBox.Text = "";
        NewRouteNameBox.Focus();
    }

    private void CancelAddRoute_Click(object sender, RoutedEventArgs e)
    {
        AddRoutePanel.Visibility = Visibility.Collapsed;
        ShowAddRouteBtn.Visibility = Visibility.Visible;
    }

    private async void ConfirmAddRoute_Click(object sender, RoutedEventArgs e)
    {
        var name = NewRouteNameBox.Text.Trim();
        if (string.IsNullOrEmpty(name)) return;

        var result = await _api.CreateRouteAsync(name);
        if (result != null)
        {
            CancelAddRoute_Click(null!, null!);
            LoadRoutesAsync(); // reload to get new ID
        }
    }

    private void NewRouteNameBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) ConfirmAddRoute_Click(sender, e);
        if (e.Key == Key.Escape) CancelAddRoute_Click(sender, e);
    }

    private void RoutesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RoutesList.SelectedItem is Models.Route route)
        {
            _selectedRoute = route;
            NoRouteSelectedState.Visibility = Visibility.Collapsed;
            MerchantsGrid.Visibility = Visibility.Visible;
            SelectedRouteTitle.Text = route.RouteName;
            
            // Hide add merchant card when switching routes
            AddMerchantCard.Visibility = Visibility.Collapsed;
            
            LoadMerchantsAsync(route.RouteId);
        }
    }

    // ══════════════════════════════════════
    // MERCHANTS IN ROUTE
    // ══════════════════════════════════════

    private async void LoadMerchantsAsync(int routeId)
    {
        _allMerchantsCache = await _api.GetRouteMerchantsAsync(routeId);
        
        // Update local counter
        if (_selectedRoute != null)
        {
            _selectedRoute.MerchantCount = _allMerchantsCache.Count;
            // Force property change on route? _routes is observable but properties aren't unless INotifyPropertyChanged
            // For now, reload routes list quietly or just ignore since it's a minor UI detail
        }

        FilterMerchants();
    }

    private void FilterRouteMerchantsBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = FilterRouteMerchantsBox.Text.Trim();
        FilterPlaceholder.Visibility = string.IsNullOrEmpty(query) ? Visibility.Visible : Visibility.Collapsed;
        FilterMerchants();
    }

    private void FilterMerchants()
    {
        var query = FilterRouteMerchantsBox.Text.Trim();
        
        var filtered = string.IsNullOrEmpty(query)
            ? _allMerchantsCache
            : _allMerchantsCache.Where(m => m.MerchantName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        _merchants = new ObservableCollection<RouteMerchant>(filtered);
        RouteMerchantsList.ItemsSource = _merchants;
        
        EmptyMerchantsState.Visibility = _allMerchantsCache.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        
        // Update position hint
        PositionHintText.Text = $"(يوجد حالياً {_allMerchantsCache.Count} تاجر)";
        if (!AddMerchantCard.IsVisible || string.IsNullOrEmpty(MerchantSearchBox.Text))
            PositionBox.Text = (_allMerchantsCache.Count + 1).ToString();
    }

    private async void RemoveMerchant_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is RouteMerchant rm && _selectedRoute != null)
        {
            var res = MessageBox.Show($"هل أنت متأكد من حذف '{rm.MerchantName}' من الخط؟", "تأكيد الحذف", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (res == MessageBoxResult.Yes)
            {
                var success = await _api.RemoveMerchantFromRouteAsync(_selectedRoute.RouteId, rm.RouteMerchantId);
                if (success) LoadMerchantsAsync(_selectedRoute.RouteId);
            }
        }
    }

    // ══════════════════════════════════════
    // ADD MERCHANT TO ROUTE
    // ══════════════════════════════════════

    private MerchantSearchResult? _selectedNewMerchant;

    private void ShowAddMerchantCard_Click(object sender, RoutedEventArgs e)
    {
        AddMerchantCard.Visibility = Visibility.Visible;
        MerchantSearchBox.Text = "";
        _selectedNewMerchant = null;
        PositionBox.Text = (_allMerchantsCache.Count + 1).ToString();
        MerchantSearchBox.Focus();
    }

    private void CancelAddMerchant_Click(object sender, RoutedEventArgs e)
    {
        AddMerchantCard.Visibility = Visibility.Collapsed;
    }

    private async void MerchantSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = MerchantSearchBox.Text.Trim();
        if (query.Length < 2)
        {
            MerchantSearchPopup.IsOpen = false;
            return;
        }

        // Only search if user typed (not if we just filled the box from selection)
        if (_selectedNewMerchant != null && MerchantSearchBox.Text == _selectedNewMerchant.MerchantName)
            return;

        var results = await _api.SearchMerchantsForRouteAsync(query);
        if (results.Count > 0)
        {
            MerchantSearchResults.ItemsSource = results;
            MerchantSearchPopup.IsOpen = true;
        }
        else
        {
            MerchantSearchPopup.IsOpen = false;
        }
    }

    private void MerchantSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MerchantSearchResults.SelectedItem is MerchantSearchResult result)
        {
            _selectedNewMerchant = result;
            MerchantSearchBox.Text = result.MerchantName;
            MerchantSearchPopup.IsOpen = false;
        }
    }

    private async void ConfirmAddMerchant_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRoute == null) return;
        if (_selectedNewMerchant == null)
        {
            MessageBox.Show("يرجى اختيار التاجر من قائمة البحث", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(PositionBox.Text, out int position))
        {
            MessageBox.Show("الترتيب غير صحيح", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = await _api.AddMerchantToRouteAsync(_selectedRoute.RouteId, _selectedNewMerchant.MerchantId, position);
        if (result != null)
        {
            AddMerchantCard.Visibility = Visibility.Collapsed;
            LoadMerchantsAsync(_selectedRoute.RouteId);
        }
    }

    // ══════════════════════════════════════
    // DRAG AND DROP REORDERING
    // ══════════════════════════════════════

    private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock handle && handle.DataContext is RouteMerchant item)
        {
            // Start drag only from the handle
            DragDrop.DoDragDrop(handle, item, DragDropEffects.Move);
        }
    }

    private async void RouteMerchantsList_Drop(object sender, DragEventArgs e)
    {
        if (_selectedRoute == null) return;
        
        if (e.Data.GetDataPresent(typeof(RouteMerchant)))
        {
            var droppedData = e.Data.GetData(typeof(RouteMerchant)) as RouteMerchant;
            if (droppedData == null) return;

            // Find target element
            var targetItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (targetItem == null) return;

            var targetData = targetItem.DataContext as RouteMerchant;
            if (targetData == null || targetData.RouteMerchantId == droppedData.RouteMerchantId) return;

            int oldIndex = _merchants.IndexOf(droppedData);
            int newIndex = _merchants.IndexOf(targetData);

            // Move locally for immediate visual feedback
            _merchants.Move(oldIndex, newIndex);

            // Update position numbers visually
            for (int i = 0; i < _merchants.Count; i++)
            {
                _merchants[i].PositionOrder = i + 1;
            }

            // Call API
            // The API expects the new position order (1-indexed based on the UI)
            await _api.UpdateMerchantPositionAsync(_selectedRoute.RouteId, droppedData.RouteMerchantId, newIndex + 1);
            
            // Reload to ensure sync
            LoadMerchantsAsync(_selectedRoute.RouteId);
        }
    }

    // Helper to find parent of type
    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        do
        {
            if (current is T ancestor) return ancestor;
            current = VisualTreeHelper.GetParent(current);
        } while (current != null);
        return null;
    }
}
