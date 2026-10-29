using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class ProductsPage : UserControl
{
    private readonly ApiClient _api;
    private JArray _allProducts = new();
    private List<CategoryComboItem> _categories = new();
    private List<UnitComboItem> _units = new();
    private int? _editingProductId;
    private int? _editingCategoryId;
    private int _managingProductId;

    public ProductsPage(ApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    // ══════════════════════════════════════
    // LOADING
    // ══════════════════════════════════════

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadCategoriesAsync();
        await LoadUnitsAsync();
        await LoadProductsAsync();
    }

    public async Task RefreshAsync()
    {
        await LoadCategoriesAsync();
        await LoadUnitsAsync();
        await LoadProductsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var json = await _api.GetProductCategoriesJsonAsync();
            if (json == null) return;
            var jArray = JArray.Parse(json);
            _categories = jArray.Select(c => new CategoryComboItem
            {
                categoryId = c["categoryId"]?.Value<int>() ?? 0,
                categoryName = c["categoryName"]?.ToString() ?? ""
            }).ToList();

            var filterItems = new List<CategoryComboItem>
            {
                new CategoryComboItem { categoryId = 0, categoryName = "— الكل —" }
            };
            filterItems.AddRange(_categories);

            CategoryFilter.ItemsSource = filterItems;
            CategoryFilter.SelectedIndex = 0;

            // Also update product dialog combo
            ProductCategoryCombo.ItemsSource = _categories;
        }
        catch { }
    }

    private async Task LoadUnitsAsync()
    {
        try
        {
            var json = await _api.GetUnitsOfMeasureJsonAsync();
            if (json == null) return;
            var jArray = JArray.Parse(json);
            _units = jArray.Select(u => new UnitComboItem
            {
                unitId = u["unitId"]?.Value<int>() ?? 0,
                unitName = u["unitName"]?.ToString() ?? ""
            }).ToList();
        }
        catch { }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            int? catId = null;
            if (CategoryFilter.SelectedValue is int id && id > 0)
                catId = id;

            var json = await _api.GetProductsJsonAsync(catId);
            if (json == null) return;
            _allProducts = JArray.Parse(json);
            ApplyFilter();
        }
        catch { }
    }

    private void ApplyFilter()
    {
        var search = SearchBox?.Text?.Trim() ?? "";
        IEnumerable<JToken> filtered = _allProducts;

        if (!string.IsNullOrEmpty(search))
        {
            filtered = filtered.Where(p =>
                (p["productName"]?.ToString() ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (p["categoryName"]?.ToString() ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (p["productCode"]?.ToString() ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        ProductsGrid.ItemsSource = filtered.ToList();
    }

    private void CategoryFilter_Changed(object sender, SelectionChangedEventArgs e)
    {
        _ = LoadProductsAsync();
    }

    private void SearchBox_Changed(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    // ══════════════════════════════════════
    // PRODUCT CRUD
    // ══════════════════════════════════════

    private void AddProduct_Click(object sender, RoutedEventArgs e)
    {
        _editingProductId = null;
        ProductDialogTitle.Text = "إضافة صنف جديد";
        ProductNameBox.Text = "";
        ProductCodeBox.Text = "";
        ProductCategoryCombo.SelectedIndex = _categories.Count > 0 ? 0 : -1;
        ProductDialogError.Visibility = Visibility.Collapsed;
        ProductDialog.Visibility = Visibility.Visible;
    }

    private void EditProduct_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is JToken product)
        {
            _editingProductId = product["productId"]?.Value<int>();
            ProductDialogTitle.Text = "تعديل صنف";
            ProductNameBox.Text = product["productName"]?.ToString() ?? "";
            ProductCodeBox.Text = product["productCode"]?.ToString() ?? "";

            var catId = product["categoryId"]?.Value<int>() ?? 0;
            ProductCategoryCombo.SelectedValue = catId;

            ProductDialogError.Visibility = Visibility.Collapsed;
            ProductDialog.Visibility = Visibility.Visible;
        }
    }

    private async void SaveProduct_Click(object sender, RoutedEventArgs e)
    {
        var name = ProductNameBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            ShowProductError("اسم الصنف مطلوب");
            return;
        }

        if (ProductCategoryCombo.SelectedValue is not int catId || catId <= 0)
        {
            ShowProductError("اختر نوع الصنف");
            return;
        }

        var code = string.IsNullOrWhiteSpace(ProductCodeBox.Text) ? null : ProductCodeBox.Text.Trim();

        try
        {
            if (_editingProductId == null)
            {
                var result = await _api.CreateProductAsync(catId, name, code);
                if (result == null) { ShowProductError("حدث خطأ أثناء الحفظ"); return; }
            }
            else
            {
                var success = await _api.UpdateProductAsync(_editingProductId.Value, catId, name, code, true, 0);
                if (!success) { ShowProductError("حدث خطأ أثناء التحديث"); return; }
            }

            ProductDialog.Visibility = Visibility.Collapsed;
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            ShowProductError(ex.Message);
        }
    }

    private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is JToken product)
        {
            var name = product["productName"]?.ToString() ?? "";
            var result = MessageBox.Show($"هل تريد حذف الصنف \"{name}\"؟", "تأكيد الحذف",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var id = product["productId"]?.Value<int>() ?? 0;
                await _api.DeleteProductAsync(id);
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void CancelProductDialog_Click(object sender, RoutedEventArgs e)
    {
        ProductDialog.Visibility = Visibility.Collapsed;
    }

    private void ProductsGrid_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (ProductsGrid.SelectedItem is JToken product)
        {
            _editingProductId = product["productId"]?.Value<int>();
            ProductDialogTitle.Text = "تعديل صنف";
            ProductNameBox.Text = product["productName"]?.ToString() ?? "";
            ProductCodeBox.Text = product["productCode"]?.ToString() ?? "";
            var catId = product["categoryId"]?.Value<int>() ?? 0;
            ProductCategoryCombo.SelectedValue = catId;
            ProductDialogError.Visibility = Visibility.Collapsed;
            ProductDialog.Visibility = Visibility.Visible;
        }
    }

    private void ShowProductError(string msg)
    {
        ProductDialogError.Text = msg;
        ProductDialogError.Visibility = Visibility.Visible;
    }

    // ══════════════════════════════════════
    // CATEGORY CRUD
    // ══════════════════════════════════════

    private void AddCategory_Click(object sender, RoutedEventArgs e)
    {
        _editingCategoryId = null;
        CategoryDialogTitle.Text = "إضافة نوع صنف جديد";
        CategoryNameBox.Text = "";
        CategoryDialogError.Visibility = Visibility.Collapsed;
        CategoryDialog.Visibility = Visibility.Visible;
    }

    private async void SaveCategory_Click(object sender, RoutedEventArgs e)
    {
        var name = CategoryNameBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            CategoryDialogError.Text = "اسم النوع مطلوب";
            CategoryDialogError.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            if (_editingCategoryId == null)
            {
                var result = await _api.CreateProductCategoryAsync(name);
                if (result == null)
                {
                    CategoryDialogError.Text = "حدث خطأ أثناء الحفظ";
                    CategoryDialogError.Visibility = Visibility.Visible;
                    return;
                }
            }
            else
            {
                var success = await _api.UpdateProductCategoryAsync(_editingCategoryId.Value, name, true, 0);
                if (!success)
                {
                    CategoryDialogError.Text = "حدث خطأ أثناء التحديث";
                    CategoryDialogError.Visibility = Visibility.Visible;
                    return;
                }
            }

            CategoryDialog.Visibility = Visibility.Collapsed;
            await LoadCategoriesAsync();
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            CategoryDialogError.Text = ex.Message;
            CategoryDialogError.Visibility = Visibility.Visible;
        }
    }

    private void CancelCategoryDialog_Click(object sender, RoutedEventArgs e)
    {
        CategoryDialog.Visibility = Visibility.Collapsed;
    }

    // ══════════════════════════════════════
    // PRODUCT UNITS MANAGEMENT
    // ══════════════════════════════════════

    private async void ManageUnits_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is JToken product)
        {
            _managingProductId = product["productId"]?.Value<int>() ?? 0;
            var name = product["productName"]?.ToString() ?? "";
            UnitsDialogTitle.Text = $"وحدات الصنف: {name}";
            UnitsDialogError.Visibility = Visibility.Collapsed;

            await LoadProductUnitsAsync();

            // Populate available units combo (exclude already assigned)
            RefreshAvailableUnitsCombo();

            UnitsDialog.Visibility = Visibility.Visible;
        }
    }

    private JArray _productUnits = new();

    private async Task LoadProductUnitsAsync()
    {
        try
        {
            var json = await _api.GetProductJsonAsync(_managingProductId);
            if (json == null) return;
            var product = JObject.Parse(json);
            _productUnits = product["units"] as JArray ?? new JArray();

            // Add a display-friendly "isDefaultText" property
            foreach (var pu in _productUnits)
            {
                pu["isDefaultText"] = pu["isDefault"]?.Value<bool>() == true ? "✅" : "";
            }

            UnitsGrid.ItemsSource = _productUnits.ToList();
        }
        catch { }
    }

    private void RefreshAvailableUnitsCombo()
    {
        var assignedUnitIds = _productUnits.Select(pu => pu["unitId"]?.Value<int>() ?? 0).ToHashSet();
        var available = _units.Where(u => !assignedUnitIds.Contains(u.unitId)).ToList();
        AddUnitCombo.ItemsSource = available;
        AddUnitCombo.SelectedIndex = available.Count > 0 ? 0 : -1;
    }

    private async void AddUnitToProduct_Click(object sender, RoutedEventArgs e)
    {
        if (AddUnitCombo.SelectedValue is not int unitId || unitId <= 0)
        {
            UnitsDialogError.Text = "اختر وحدة";
            UnitsDialogError.Visibility = Visibility.Visible;
            return;
        }

        var isDefault = AddUnitDefault.IsChecked == true;

        try
        {
            var result = await _api.AddProductUnitAsync(_managingProductId, unitId, isDefault);
            if (result == null)
            {
                UnitsDialogError.Text = "حدث خطأ أثناء الإضافة";
                UnitsDialogError.Visibility = Visibility.Visible;
                return;
            }

            UnitsDialogError.Visibility = Visibility.Collapsed;
            AddUnitDefault.IsChecked = false;
            await LoadProductUnitsAsync();
            RefreshAvailableUnitsCombo();
            await LoadProductsAsync(); // Refresh main grid to show updated unit count
        }
        catch (Exception ex)
        {
            UnitsDialogError.Text = ex.Message;
            UnitsDialogError.Visibility = Visibility.Visible;
        }
    }

    private async void RemoveUnit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is JToken unit)
        {
            var unitName = unit["unitName"]?.ToString() ?? "";
            var result = MessageBox.Show($"هل تريد حذف الوحدة \"{unitName}\"؟", "تأكيد",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var puId = unit["productUnitId"]?.Value<int>() ?? 0;
                await _api.RemoveProductUnitAsync(_managingProductId, puId);
                await LoadProductUnitsAsync();
                RefreshAvailableUnitsCombo();
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void CloseUnitsDialog_Click(object sender, RoutedEventArgs e)
    {
        UnitsDialog.Visibility = Visibility.Collapsed;
    }
}
