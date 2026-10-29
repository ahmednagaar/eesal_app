using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class GoodsReturnsPage : UserControl
{
    private readonly ApiClient _api;
    private ObservableCollection<ReturnItemRow> _items = new();
    private bool _isLoaded = false;

    public GoodsReturnsPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDriversAsync();
        await LoadReturnsAsync();
        _isLoaded = true;
    }

    private async Task LoadDriversAsync()
    {
        try
        {
            var json = await _api.GetDriversJsonAsync();
            if (json != null)
            {
                var drivers = JsonConvert.DeserializeObject<JArray>(json);
                var allDrivers = new JArray();
                allDrivers.Add(JObject.FromObject(new { driverId = 0, fullName = "كل السائقين" }));
                if (drivers != null) foreach (var d in drivers) allDrivers.Add(d);
                DriverFilter.ItemsSource = allDrivers;
                DriverFilter.SelectedIndex = 0;
            }
        }
        catch { }
    }

    private async Task LoadReturnsAsync()
    {
        try
        {
            int? driverId = null;
            if (DriverFilter.SelectedValue is int did && did > 0) driverId = did;
            string? status = null;
            if (ReturnStatusFilter.SelectedItem is ComboBoxItem item && item.Tag is string tag && !string.IsNullOrEmpty(tag))
                status = tag;
            string? from = FromDate.SelectedDate?.ToString("yyyy-MM-dd");
            string? to = ToDate.SelectedDate?.ToString("yyyy-MM-dd");

            var json = await _api.GetGoodsReturnsJsonAsync(driverId, status, from, to);
            if (json != null)
            {
                var returns = JsonConvert.DeserializeObject<JArray>(json);
                ReturnsGrid.ItemsSource = returns;
            }
        }
        catch (ApiUnauthorizedException) { MessageBox.Show("انتهت صلاحية الجلسة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void Filter_Changed(object sender, object e) { if (_isLoaded) await LoadReturnsAsync(); }
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadReturnsAsync();

    private void ReturnsGrid_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // Could open detail view — for now just show info
        if (ReturnsGrid.SelectedItem is JObject ret)
        {
            MessageBox.Show($"مرتجع رقم {ret["returnReceiptNumber"]}\nالسائق: {ret["driverName"]}\nالمبلغ: {ret["totalAmount"]:N2}\nالأصناف: {ret["itemCount"]}",
                "تفاصيل المرتجع", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // ═══ CREATE RETURN ═══
    private async void AddReturn_Click(object sender, RoutedEventArgs e)
    {
        _items.Clear();
        ReturnReceiptNumBox.Text = "";
        CustomerNameBox.Text = "";
        ReturnNotesBox.Text = "";
        ReturnDatePicker.SelectedDate = DateTime.Today;
        ReturnDialogError.Visibility = Visibility.Collapsed;
        ReturnItemsGrid.ItemsSource = _items;

        // Load dropdowns
        try
        {
            var driversJson = await _api.GetDriversJsonAsync();
            if (driversJson != null) ReturnDriverCombo.ItemsSource = JsonConvert.DeserializeObject<JArray>(driversJson);

            var reasonsJson = await _api.GetReturnReasonsJsonAsync();
            if (reasonsJson != null) ReturnReasonCombo.ItemsSource = JsonConvert.DeserializeObject<JArray>(reasonsJson);

            var productsJson = await _api.GetProductsJsonAsync();
            if (productsJson != null) ItemProductCombo.ItemsSource = JsonConvert.DeserializeObject<JArray>(productsJson);

            // Load return books for linking
            try
            {
                var booksJson = await _api.GetAllReturnBooksJsonAsync();
                if (booksJson != null)
                {
                    var books = JsonConvert.DeserializeObject<JArray>(booksJson);
                    if (books != null)
                    {
                        var bookList = new JArray();
                        bookList.Add(JObject.FromObject(new { bookId = 0, bookLabel = "— بدون دفتر —" }));
                        foreach (var b in books)
                        {
                            b["bookLabel"] = $"دفتر {b["bookNumber"]} ({b["startReceiptNumber"]}-{b["endReceiptNumber"]})"; 
                            bookList.Add(b);
                        }
                        ReturnBookCombo.ItemsSource = bookList;
                        ReturnBookCombo.SelectedIndex = 0;
                    }
                }
            }
            catch { }
        }
        catch { }

        ReturnDialog.Visibility = Visibility.Visible;
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        if (ItemProductCombo.SelectedItem is not JObject product) return;
        if (!decimal.TryParse(ItemQtyBox.Text.Trim(), out decimal qty) || qty <= 0) return;
        if (!decimal.TryParse(ItemPriceBox.Text.Trim(), out decimal price)) price = 0;
        if (!decimal.TryParse(ItemTotalBox.Text.Trim(), out decimal total)) total = qty * price;

        _items.Add(new ReturnItemRow
        {
            ProductId = product["productId"]!.Value<int>(),
            ProductName = product["productName"]!.ToString(),
            ProductUnitId = product["defaultUnitId"]?.Value<int>() ?? 0,
            Quantity = qty,
            UnitPrice = price,
            TotalPrice = total
        });

        ItemQtyBox.Text = "";
        ItemPriceBox.Text = "";
        ItemTotalBox.Text = "";
    }

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is ReturnItemRow item)
            _items.Remove(item);
    }

    private async void SaveReturn_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(ReturnReceiptNumBox.Text.Trim(), out int receiptNum))
        {
            ReturnDialogError.Text = "أدخل رقم الإيصال";
            ReturnDialogError.Visibility = Visibility.Visible;
            return;
        }
        if (ReturnDriverCombo.SelectedValue == null)
        {
            ReturnDialogError.Text = "اختر السائق";
            ReturnDialogError.Visibility = Visibility.Visible;
            return;
        }
        if (ReturnReasonCombo.SelectedValue == null)
        {
            ReturnDialogError.Text = "اختر السبب";
            ReturnDialogError.Visibility = Visibility.Visible;
            return;
        }
        if (_items.Count == 0)
        {
            ReturnDialogError.Text = "أضف صنف واحد على الأقل";
            ReturnDialogError.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            var data = new
            {
                ReturnReceiptNumber = receiptNum,
                DriverId = (int)ReturnDriverCombo.SelectedValue,
                ReasonId = (int)ReturnReasonCombo.SelectedValue,
                BookId = ReturnBookCombo.SelectedValue is int bid && bid > 0 ? (int?)bid : null,
                ReturnDate = ReturnDatePicker.SelectedDate ?? DateTime.Today,
                CustomerName = CustomerNameBox.Text.Trim(),
                Notes = ReturnNotesBox.Text.Trim(),
                Items = _items.Select(i => new
                {
                    i.ProductId,
                    i.ProductUnitId,
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice
                }).ToList()
            };

            var result = await _api.CreateGoodsReturnAsync(data);
            if (result != null)
            {
                ReturnDialog.Visibility = Visibility.Collapsed;
                await LoadReturnsAsync();
            }
        }
        catch (Exception ex)
        {
            ReturnDialogError.Text = ex.Message;
            ReturnDialogError.Visibility = Visibility.Visible;
        }
    }

    private void CancelReturnDialog_Click(object sender, RoutedEventArgs e) => ReturnDialog.Visibility = Visibility.Collapsed;

    // ═══ DELETE ═══
    private async void DeleteReturn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not JObject ret) return;
        var confirm = MessageBox.Show($"هل تريد حذف مرتجع رقم {ret["returnReceiptNumber"]}؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            int id = ret["returnId"]!.Value<int>();
            bool deleted = await _api.DeleteGoodsReturnAsync(id);
            if (deleted) await LoadReturnsAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }
}

public class ReturnItemRow
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int ProductUnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
