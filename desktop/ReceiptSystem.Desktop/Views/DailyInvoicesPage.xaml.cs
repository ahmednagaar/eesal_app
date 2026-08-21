using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Models;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class DailyInvoicesPage : UserControl
{
    private readonly ApiClient _api;
    private List<Models.Route> _routes = new();
    private ObservableCollection<ChecklistItem> _checklist = new();
    private int _deliveryDayId;
    private bool _isStarted;

    public DailyInvoicesPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        DatePicker.SelectedDate = DateTime.Today;
        LoadRoutes();
    }

    // ══════════════════════════════════════
    // SETUP
    // ══════════════════════════════════════

    private async void LoadRoutes()
    {
        _routes = await _api.GetRoutesAsync();
        RouteCombo.ItemsSource = _routes;
        if (_routes.Count > 0)
            RouteCombo.SelectedIndex = 0;
    }

    private async void Start_Click(object sender, RoutedEventArgs e)
    {
        if (RouteCombo.SelectedItem is not Models.Route selectedRoute)
        {
            ShowSetupError("يرجى اختيار الخط");
            return;
        }
        if (DatePicker.SelectedDate is not DateTime selectedDate)
        {
            ShowSetupError("يرجى اختيار التاريخ");
            return;
        }

        StartBtn.IsEnabled = false;
        SetupError.Visibility = Visibility.Collapsed;

        try
        {
            // 1. Create or find delivery day
            var existingDays = await _api.GetDeliveryDaysAsync(selectedDate, selectedRoute.RouteId);
            JArray? invoices = null;

            if (existingDays.Count > 0)
            {
                var existing = existingDays[0] as JObject;
                if (existing != null)
                    _deliveryDayId = existing["deliveryDayId"]?.Value<int>() ?? 0;

                // GET /delivery-days/{id} returns { day: {...}, invoices: [...] }
                var dayResponse = await _api.GetDeliveryDayAsync(_deliveryDayId);
                if (dayResponse is JObject dayObj)
                    invoices = dayObj["invoices"] as JArray;
            }
            else
            {
                // POST /delivery-days returns the full delivery day object
                var created = await _api.CreateDeliveryDayAsync(
                    selectedRoute.RouteId, selectedDate, DriverBox.Text.Trim());
                if (created == null)
                {
                    ShowSetupError("فشل إنشاء يوم التسليم");
                    return;
                }

                var createdObj = created as JObject;
                if (createdObj?["day"] != null)
                    _deliveryDayId = createdObj["day"]?["deliveryDayId"]?.Value<int>() ?? 0;
                else
                    _deliveryDayId = createdObj?["deliveryDayId"]?.Value<int>() ?? 0;

                invoices = createdObj?["invoices"] as JArray;
            }

            if (_deliveryDayId == 0)
            {
                ShowSetupError("لم يتم العثور على معرف يوم التسليم");
                return;
            }

            // 2. Load route merchants
            var merchants = await _api.GetRouteMerchantsAsync(selectedRoute.RouteId);

            // 3. Build checklist, merging with existing invoices
            _checklist.Clear();

            foreach (var m in merchants)
            {
                var item = new ChecklistItem
                {
                    RouteMerchantId = m.RouteMerchantId,
                    MerchantId = m.MerchantId,
                    MerchantName = m.MerchantName,
                    City = m.City,
                    PositionOrder = m.PositionOrder
                };

                // Check if this merchant has an existing invoice for today
                if (invoices != null)
                {
                    var existingInv = invoices.FirstOrDefault(
                        inv => inv["routeMerchantId"]?.Value<int>() == m.RouteMerchantId);
                    if (existingInv != null)
                    {
                        item.IsSelected = true;
                        item.DayInvoiceId = existingInv["dayInvoiceId"]?.Value<int>();
                        item.InvoiceNumber = existingInv["invoiceNumber"]?.ToString() ?? "";
                        item.Quantity = existingInv["quantity"]?.ToString() ?? "";
                        item.Amount = existingInv["amount"]?.Type == JTokenType.Null
                            ? null : existingInv["amount"]?.Value<decimal>();
                        item.Notes = existingInv["notes"]?.ToString() ?? "";
                    }
                }

                _checklist.Add(item);
            }

            MerchantList.ItemsSource = _checklist;

            // 4. Switch to checklist view
            SetupPanel.Visibility = Visibility.Collapsed;
            MainTabControl.Visibility = Visibility.Visible;
            MainTabControl.SelectedIndex = 0;
            ActionBar.Visibility = Visibility.Visible;
            _isStarted = true;

            UpdateCounter();
        }
        catch (Exception ex)
        {
            ShowSetupError(ErrorMessageHelper.GetArabicMessage(ex));
        }
        finally
        {
            StartBtn.IsEnabled = true;
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        SetupPanel.Visibility = Visibility.Visible;
        MainTabControl.Visibility = Visibility.Collapsed;
        ActionBar.Visibility = Visibility.Collapsed;
        _isStarted = false;
    }

    private void ShowSetupError(string msg)
    {
        SetupError.Text = msg;
        SetupError.Visibility = Visibility.Visible;
    }

    // ══════════════════════════════════════
    // MERCHANT CHECK / UNCHECK
    // ══════════════════════════════════════

    private async void Merchant_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox cb || cb.DataContext is not ChecklistItem item) return;
        if (item.DayInvoiceId != null) return; // Already saved

        var result = await _api.AddDayInvoiceAsync(
            _deliveryDayId, item.RouteMerchantId, item.MerchantId,
            item.InvoiceNumber, item.Quantity, item.Amount, item.Notes);

        if (result != null)
        {
            item.DayInvoiceId = (int)result.dayInvoiceId;
        }

        UpdateCounter();
    }

    private async void Merchant_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox cb || cb.DataContext is not ChecklistItem item) return;
        if (item.DayInvoiceId == null) return;

        var success = await _api.RemoveDayInvoiceAsync(_deliveryDayId, item.DayInvoiceId.Value);
        if (success)
        {
            item.DayInvoiceId = null;
            item.InvoiceNumber = "";
            item.Quantity = "";
            item.Amount = null;
            item.Notes = "";
        }

        UpdateCounter();
    }

    private void UpdateCounter()
    {
        var selected = _checklist.Count(c => c.IsSelected);
        var total = _checklist.Count;
        SelectedCountText.Text = $"{selected} تاجر محدد";
        CounterText.Text = $"✅ تم تحديد {selected} من {total} تاجر";

        var totalAmount = _checklist.Where(c => c.IsSelected && c.Amount.HasValue).Sum(c => c.Amount!.Value);
        TotalAmountText.Text = totalAmount > 0 ? $"إجمالي: {totalAmount:N2} جنيه" : "";
    }

    // ══════════════════════════════════════
    // QUICK ADD SEARCH
    // ══════════════════════════════════════

    private void QuickAdd_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = QuickAddBox.Text.Trim();
        QuickAddPlaceholder.Visibility = string.IsNullOrEmpty(query)
            ? Visibility.Visible : Visibility.Collapsed;
        QuickAddClear.Visibility = string.IsNullOrEmpty(query)
            ? Visibility.Collapsed : Visibility.Visible;

        if (query.Length < 2)
        {
            QuickAddPopup.IsOpen = false;
            return;
        }

        var results = _checklist
            .Where(c => !c.IsSelected && c.MerchantName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(8)
            .ToList();

        if (results.Count > 0)
        {
            QuickAddResults.ItemsSource = results;
            QuickAddPopup.IsOpen = true;
        }
        else
        {
            QuickAddPopup.IsOpen = false;
        }
    }

    private void QuickAdd_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Select the first result
            if (QuickAddResults.ItemsSource is List<ChecklistItem> results && results.Count > 0)
            {
                SelectQuickAddItem(results[0]);
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            QuickAddPopup.IsOpen = false;
            QuickAddBox.Text = "";
        }
        else if (e.Key == Key.Down && QuickAddPopup.IsOpen)
        {
            QuickAddResults.Focus();
            if (QuickAddResults.Items.Count > 0)
                QuickAddResults.SelectedIndex = 0;
        }
    }

    private void QuickAddResult_Selected(object sender, SelectionChangedEventArgs e)
    {
        if (QuickAddResults.SelectedItem is ChecklistItem item)
        {
            SelectQuickAddItem(item);
            QuickAddResults.SelectedIndex = -1;
        }
    }

    private async void SelectQuickAddItem(ChecklistItem item)
    {
        item.IsSelected = true;

        var result = await _api.AddDayInvoiceAsync(
            _deliveryDayId, item.RouteMerchantId, item.MerchantId);
        if (result != null)
            item.DayInvoiceId = (int)result.dayInvoiceId;

        // Clear search and close popup
        QuickAddPopup.IsOpen = false;
        QuickAddBox.Text = "";
        QuickAddBox.Focus();

        UpdateCounter();

        // Highlight row animation (flash green)
        FlashRowBackground(item);

        // Scroll to the item
        MerchantList.ScrollIntoView(item);
    }

    private async void FlashRowBackground(ChecklistItem item)
    {
        // Find the ListViewItem Container
        var container = MerchantList.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
        if (container != null)
        {
            var originalBg = container.Background;
            container.Background = (SolidColorBrush)FindResource("FlashHighlightBrush");
            await Task.Delay(1500);
            container.Background = originalBg;
        }
    }

    private void QuickAddClear_Click(object sender, RoutedEventArgs e)
    {
        QuickAddBox.Text = "";
        QuickAddPopup.IsOpen = false;
        QuickAddBox.Focus();
    }

    // ══════════════════════════════════════
    // PREVIEW & REORDER TAB
    // ══════════════════════════════════════

    private ObservableCollection<ChecklistPreviewItem> _previewItems = new();

    private void PreviewTab_Selected(object sender, RoutedEventArgs e)
    {
        // Load selected items into preview
        var selected = _checklist.Where(c => c.IsSelected && c.DayInvoiceId.HasValue).ToList();
        
        var previewList = new List<ChecklistPreviewItem>();
        int seq = 1;
        foreach (var s in selected)
        {
            previewList.Add(new ChecklistPreviewItem
            {
                DayInvoiceId = s.DayInvoiceId!.Value,
                MerchantName = s.MerchantName,
                Quantity = s.Quantity,
                InvoiceNumber = s.InvoiceNumber,
                Sequence = seq++
            });
        }

        _previewItems = new ObservableCollection<ChecklistPreviewItem>(previewList);
        PreviewList.ItemsSource = _previewItems;

        EmptyPreviewState.Visibility = _previewItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void PreviewDragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock handle && handle.DataContext is ChecklistPreviewItem item)
        {
            DragDrop.DoDragDrop(handle, item, DragDropEffects.Move);
        }
    }

    private async void PreviewList_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(ChecklistPreviewItem)))
        {
            var droppedData = e.Data.GetData(typeof(ChecklistPreviewItem)) as ChecklistPreviewItem;
            if (droppedData == null) return;

            var targetItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (targetItem == null) return;

            var targetData = targetItem.DataContext as ChecklistPreviewItem;
            if (targetData == null || targetData.DayInvoiceId == droppedData.DayInvoiceId) return;

            int oldIndex = _previewItems.IndexOf(droppedData);
            int newIndex = _previewItems.IndexOf(targetData);

            _previewItems.Move(oldIndex, newIndex);

            // Update sequences locally
            var reorders = new List<object>();
            for (int i = 0; i < _previewItems.Count; i++)
            {
                _previewItems[i].Sequence = i + 1;
                reorders.Add(new { dayInvoiceId = _previewItems[i].DayInvoiceId, newSequenceNumber = i + 1 });
            }

            // Call API
            await _api.ReorderDayInvoicesAsync(_deliveryDayId, reorders);
        }
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        do
        {
            if (current is T ancestor) return ancestor;
            current = VisualTreeHelper.GetParent(current);
        } while (current != null);
        return null;
    }

    public class ChecklistPreviewItem : System.ComponentModel.INotifyPropertyChanged
    {
        public int DayInvoiceId { get; set; }
        public string MerchantName { get; set; } = "";
        public string Quantity { get; set; } = "";
        public string InvoiceNumber { get; set; } = "";

        private int _sequence;
        public int Sequence
        {
            get => _sequence;
            set { _sequence = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Sequence))); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    // ══════════════════════════════════════
    // PRINTING (Native Windows Print)
    // ══════════════════════════════════════

    private async void PrintLoading_Click(object sender, RoutedEventArgs e)
    {
        await PrintHelper.PrintSheetAsync(_api, _deliveryDayId, "loading");
    }

    private async void PrintDelivery_Click(object sender, RoutedEventArgs e)
    {
        await PrintHelper.PrintSheetAsync(_api, _deliveryDayId, "delivery");
    }
}
