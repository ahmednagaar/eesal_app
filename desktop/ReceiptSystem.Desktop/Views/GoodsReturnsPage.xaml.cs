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
    private List<ProductComboItem> _productsList = new();
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
                var drivers = JArray.Parse(json);
                var allDrivers = new List<DriverComboItem>
                {
                    new DriverComboItem { driverId = 0, fullName = "كل السائقين" }
                };
                foreach (var d in drivers)
                {
                    allDrivers.Add(new DriverComboItem
                    {
                        driverId = d["driverId"]?.Value<int>() ?? 0,
                        fullName = d["fullName"]?.ToString() ?? ""
                    });
                }
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

        // Load dropdowns with strongly-typed items
        try
        {
            // Drivers
            var driversJson = await _api.GetDriversJsonAsync();
            if (driversJson != null)
            {
                var drivers = JArray.Parse(driversJson);
                ReturnDriverCombo.ItemsSource = drivers.Select(d => new DriverComboItem
                {
                    driverId = d["driverId"]?.Value<int>() ?? 0,
                    fullName = d["fullName"]?.ToString() ?? ""
                }).ToList();
            }

            // Return reasons
            var reasonsJson = await _api.GetReturnReasonsJsonAsync();
            if (reasonsJson != null)
            {
                var reasons = JArray.Parse(reasonsJson);
                ReturnReasonCombo.ItemsSource = reasons.Select(r => new ReasonComboItem
                {
                    reasonId = r["reasonId"]?.Value<int>() ?? 0,
                    reasonName = r["reasonName"]?.ToString() ?? ""
                }).ToList();
            }

            // Products (use search endpoint to get units)
            var productsJson = await _api.SearchProductsJsonAsync("");
            // Search with empty query returns empty — use GetAll instead
            if (productsJson == null || productsJson == "[]")
                productsJson = await _api.GetProductsJsonAsync();

            if (productsJson != null)
            {
                var products = JArray.Parse(productsJson);
                _productsList = products.Select(p =>
                {
                    var item = new ProductComboItem
                    {
                        productId = p["productId"]?.Value<int>() ?? 0,
                        productName = p["productName"]?.ToString() ?? "",
                        categoryName = p["categoryName"]?.ToString(),
                        productCode = p["productCode"]?.ToString()
                    };
                    // If the response includes units (from search endpoint)
                    var units = p["units"] as JArray;
                    if (units != null)
                    {
                        item.units = units.Select(u => new ProductUnitComboItem
                        {
                            productUnitId = u["productUnitId"]?.Value<int>() ?? 0,
                            unitId = u["unitId"]?.Value<int>() ?? 0,
                            unitName = u["unitName"]?.ToString() ?? "",
                            isDefault = u["isDefault"]?.Value<bool>() ?? false
                        }).ToList();
                    }
                    return item;
                }).ToList();
                ItemProductCombo.ItemsSource = _productsList;
            }

            // Return books
            try
            {
                var booksJson = await _api.GetAllReturnBooksJsonAsync();
                if (booksJson != null)
                {
                    var books = JArray.Parse(booksJson);
                    var bookList = new List<ReturnBookComboItem>
                    {
                        new ReturnBookComboItem { bookId = 0, bookLabel = "— بدون دفتر —" }
                    };
                    foreach (var b in books)
                    {
                        bookList.Add(new ReturnBookComboItem
                        {
                            bookId = b["bookId"]?.Value<int>() ?? 0,
                            bookLabel = $"دفتر {b["bookNumber"]} ({b["startReceiptNumber"]}-{b["endReceiptNumber"]})"
                        });
                    }
                    ReturnBookCombo.ItemsSource = bookList;
                    ReturnBookCombo.SelectedIndex = 0;
                }
            }
            catch { }
        }
        catch { }

        ReturnDialog.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// When a product is selected, load its available units into ItemUnitCombo.
    /// If the product was loaded without units (from list endpoint), fetch them individually.
    /// </summary>
    private async void ItemProductCombo_Changed(object sender, SelectionChangedEventArgs e)
    {
        ItemUnitCombo.ItemsSource = null;
        ItemUnitCombo.SelectedIndex = -1;

        if (ItemProductCombo.SelectedItem is not ProductComboItem product) return;

        var units = product.units;

        // If units weren't loaded (product came from list endpoint without units), fetch individually
        if (units == null || units.Count == 0)
        {
            try
            {
                var json = await _api.GetProductJsonAsync(product.productId);
                if (json != null)
                {
                    var prod = JObject.Parse(json);
                    var unitsArr = prod["units"] as JArray;
                    if (unitsArr != null)
                    {
                        units = unitsArr.Select(u => new ProductUnitComboItem
                        {
                            productUnitId = u["productUnitId"]?.Value<int>() ?? 0,
                            unitId = u["unitId"]?.Value<int>() ?? 0,
                            unitName = u["unitName"]?.ToString() ?? "",
                            isDefault = u["isDefault"]?.Value<bool>() ?? false
                        }).ToList();
                        product.units = units; // Cache for subsequent selections
                    }
                }
            }
            catch { }
        }

        if (units != null && units.Count > 0)
        {
            ItemUnitCombo.ItemsSource = units;
            // Select the default unit if available, otherwise the first
            var defaultUnit = units.FirstOrDefault(u => u.isDefault);
            ItemUnitCombo.SelectedItem = defaultUnit ?? units[0];
        }
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        if (ItemProductCombo.SelectedItem is not ProductComboItem product)
        {
            ShowReturnError("اختر صنف");
            return;
        }
        if (ItemUnitCombo.SelectedItem is not ProductUnitComboItem unit)
        {
            ShowReturnError("اختر وحدة الصنف");
            return;
        }
        if (!decimal.TryParse(ItemQtyBox.Text.Trim(), out decimal qty) || qty <= 0)
        {
            ShowReturnError("أدخل كمية صحيحة");
            return;
        }
        decimal.TryParse(ItemPriceBox.Text.Trim(), out decimal price);
        decimal.TryParse(ItemTotalBox.Text.Trim(), out decimal total);
        if (total == 0 && price > 0) total = qty * price;

        _items.Add(new ReturnItemRow
        {
            ProductId = product.productId,
            ProductName = product.productName,
            ProductUnitId = unit.productUnitId,
            UnitName = unit.unitName,
            Quantity = qty,
            UnitPrice = price,
            TotalPrice = total
        });

        ReturnDialogError.Visibility = Visibility.Collapsed;
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
            ShowReturnError("أدخل رقم الإيصال");
            return;
        }
        if (ReturnDriverCombo.SelectedValue is not int driverId || driverId <= 0)
        {
            ShowReturnError("اختر السائق");
            return;
        }
        if (ReturnReasonCombo.SelectedValue is not int reasonId || reasonId <= 0)
        {
            ShowReturnError("اختر السبب");
            return;
        }
        if (_items.Count == 0)
        {
            ShowReturnError("أضف صنف واحد على الأقل");
            return;
        }

        try
        {
            int? bookId = null;
            if (ReturnBookCombo.SelectedValue is int bid && bid > 0)
                bookId = bid;

            var data = new
            {
                ReturnReceiptNumber = receiptNum,
                DriverId = driverId,
                ReasonId = reasonId,
                BookId = bookId,
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
            ShowReturnError(ex.Message);
        }
    }

    private void ShowReturnError(string msg)
    {
        ReturnDialogError.Text = msg;
        ReturnDialogError.Visibility = Visibility.Visible;
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
    public string UnitName { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
