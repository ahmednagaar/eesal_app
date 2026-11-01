using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class AjalEntryPage : UserControl
{
    private readonly ApiClient _api;
    private string _prefix = "441";
    private int _totalDigits = 6;
    private List<Models.Route> _routes = new();
    private List<DriverComboItem> _drivers = new();
    private List<MerchantComboItem> _merchants = new();

    // Selected items for header combos
    private Models.Route? _selectedRoute;
    private DriverComboItem? _selectedDriver;
    private bool _suppressRouteSearch;
    private bool _suppressDriverSearch;

    public AjalEntryPage(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        Loaded += AjalEntryPage_Loaded;
    }

    private async void AjalEntryPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadLookups();
        AddEntryRow(); // Start with one empty row
    }

    // ═══════════════════════════════════════
    // LOAD LOOKUPS
    // ═══════════════════════════════════════
    private async Task LoadLookups()
    {
        var failed = new List<string>();

        try
        {
            _routes = await _api.GetRoutesAsync();
            if (_routes.Count > 0)
            {
                _selectedRoute = _routes[0];
                _suppressRouteSearch = true;
                RouteSearchBox.Text = _selectedRoute.RouteName;
                _suppressRouteSearch = false;
                ClearRouteBtn.Visibility = Visibility.Visible;
            }
        }
        catch { failed.Add("الخطوط"); }

        try
        {
            var driversJson = await _api.GetDriversJsonAsync();
            if (driversJson != null)
            {
                _drivers = ParseJsonArray(driversJson).Select(d => new DriverComboItem
                {
                    driverId = d["driverId"]?.Value<int>() ?? 0,
                    fullName = d["fullName"]?.ToString() ?? ""
                }).ToList();
                if (_drivers.Count > 0)
                {
                    _selectedDriver = _drivers[0];
                    _suppressDriverSearch = true;
                    DriverSearchBox.Text = _selectedDriver.fullName;
                    _suppressDriverSearch = false;
                    ClearDriverBtn.Visibility = Visibility.Visible;
                }
            }
        }
        catch { failed.Add("السائقين"); }

        try
        {
            var merchantsJson = await _api.GetMerchantsJsonAsync(1, 10000);
            if (merchantsJson != null)
            {
                _merchants = ParseJsonArray(merchantsJson).Select(m => new MerchantComboItem
                {
                    merchantId = m["merchantId"]?.Value<int>() ?? 0,
                    merchantName = m["merchantName"]?.ToString() ?? ""
                }).ToList();
            }
        }
        catch { failed.Add("العملاء"); }

        try
        {
            var prefixJson = await _api.GetAjalPrefixSettingsJsonAsync();
            if (prefixJson != null)
            {
                var settings = UnwrapObject(prefixJson);
                _prefix = settings["prefix"]?.ToString() ?? "441";
                _totalDigits = settings["totalDigits"]?.Value<int>() ?? 6;
                txtPrefix.Text = _prefix;
            }
        }
        catch { failed.Add("البادئة"); }

        if (failed.Count > 0)
            ToastHelper.ShowError(RootGrid, "تعذر تحميل: " + string.Join("، ", failed));
    }

    // ═══════════════════════════════════════
    // ROUTE SEARCH (header)
    // ═══════════════════════════════════════
    private void RouteSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressRouteSearch) return;
        var query = RouteSearchBox.Text.Trim();
        if (string.IsNullOrEmpty(query)) { _selectedRoute = null; ClearRouteBtn.Visibility = Visibility.Collapsed; RoutePopup.IsOpen = false; return; }
        if (_selectedRoute != null && query == _selectedRoute.RouteName) return;
        _selectedRoute = null;
        var filtered = _routes.Where(r => r.RouteName.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(50).ToList();
        RouteResults.ItemsSource = filtered;
        RoutePopup.IsOpen = filtered.Count > 0;
    }

    private void RouteSearch_GotFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RouteSearchBox.Text) && _routes.Count > 0)
        { RouteResults.ItemsSource = _routes.Take(50).ToList(); RoutePopup.IsOpen = true; }
    }

    private void RouteResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RouteResults.SelectedItem is Models.Route item)
        {
            _selectedRoute = item;
            _suppressRouteSearch = true;
            RouteSearchBox.Text = item.RouteName;
            _suppressRouteSearch = false;
            ClearRouteBtn.Visibility = Visibility.Visible;
            RoutePopup.IsOpen = false;
            RouteResults.SelectedItem = null;
        }
    }

    private void ClearRoute_Click(object sender, RoutedEventArgs e)
    {
        _selectedRoute = null; _suppressRouteSearch = true; RouteSearchBox.Text = ""; _suppressRouteSearch = false;
        ClearRouteBtn.Visibility = Visibility.Collapsed; RouteSearchBox.Focus();
    }

    // ═══════════════════════════════════════
    // DRIVER SEARCH (header)
    // ═══════════════════════════════════════
    private void DriverSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressDriverSearch) return;
        var query = DriverSearchBox.Text.Trim();
        if (string.IsNullOrEmpty(query)) { _selectedDriver = null; ClearDriverBtn.Visibility = Visibility.Collapsed; DriverPopup.IsOpen = false; return; }
        if (_selectedDriver != null && query == _selectedDriver.fullName) return;
        _selectedDriver = null;
        var filtered = _drivers.Where(d => d.fullName.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(50).ToList();
        DriverResults.ItemsSource = filtered;
        DriverPopup.IsOpen = filtered.Count > 0;
    }

    private void DriverSearch_GotFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DriverSearchBox.Text) && _drivers.Count > 0)
        { DriverResults.ItemsSource = _drivers.Take(50).ToList(); DriverPopup.IsOpen = true; }
    }

    private void DriverResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DriverResults.SelectedItem is DriverComboItem item)
        {
            _selectedDriver = item;
            _suppressDriverSearch = true;
            DriverSearchBox.Text = item.fullName;
            _suppressDriverSearch = false;
            ClearDriverBtn.Visibility = Visibility.Visible;
            DriverPopup.IsOpen = false;
            DriverResults.SelectedItem = null;
        }
    }

    private void ClearDriver_Click(object sender, RoutedEventArgs e)
    {
        _selectedDriver = null; _suppressDriverSearch = true; DriverSearchBox.Text = ""; _suppressDriverSearch = false;
        ClearDriverBtn.Visibility = Visibility.Collapsed; DriverSearchBox.Focus();
    }

    // ═══════════════════════════════════════
    // ENTRY ROW MANAGEMENT
    // ═══════════════════════════════════════
    private void AddEntryRow()
    {
        int rowIndex = pnlRows.Children.Count + 1;

        var row = new Grid { Margin = new Thickness(0, 0, 0, 4) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });      // #
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });     // suffix
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });     // full number
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // merchant
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });     // amount
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // notes
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });      // delete

        // Row number
        var lblNum = new TextBlock
        {
            Text = rowIndex.ToString(),
            Foreground = (Brush)FindResource("TextSecondaryBrush"),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 14
        };
        Grid.SetColumn(lblNum, 0);
        row.Children.Add(lblNum);

        // Invoice suffix input
        var txtSuffix = new TextBox
        {
            Style = (Style)FindResource("ModernTextBox"),
            FontSize = 16, FontWeight = FontWeights.Bold,
            Tag = "suffix",
            MaxLength = 6,
            Margin = new Thickness(2)
        };
        txtSuffix.TextChanged += TxtSuffix_TextChanged;
        txtSuffix.KeyDown += TxtField_KeyDown;
        Grid.SetColumn(txtSuffix, 1);
        row.Children.Add(txtSuffix);

        // Full number display
        var lblFull = new TextBlock
        {
            Foreground = (Brush)FindResource("AccentPrimaryBrush"),
            FontSize = 16, FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
            Tag = "fullNumber"
        };
        Grid.SetColumn(lblFull, 2);
        row.Children.Add(lblFull);

        // Merchant — searchable TextBox + Popup + ListBox
        var merchantContainer = CreateSearchableMerchantField();
        Grid.SetColumn(merchantContainer, 3);
        row.Children.Add(merchantContainer);

        // Amount
        var txtAmount = new TextBox
        {
            Style = (Style)FindResource("ModernTextBox"),
            Tag = "amount",
            Margin = new Thickness(2)
        };
        txtAmount.KeyDown += TxtField_KeyDown;
        Grid.SetColumn(txtAmount, 4);
        row.Children.Add(txtAmount);

        // Notes
        var txtNotes = new TextBox
        {
            Style = (Style)FindResource("ModernTextBox"),
            Tag = "notes",
            Margin = new Thickness(2)
        };
        txtNotes.KeyDown += TxtField_KeyDown;
        Grid.SetColumn(txtNotes, 5);
        row.Children.Add(txtNotes);

        // Delete button
        var btnDel = new Button
        {
            Content = "✕", FontSize = 14,
            Width = 30, Height = 30,
            Background = Brushes.Transparent,
            Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center
        };
        btnDel.Click += (s, ev) =>
        {
            pnlRows.Children.Remove(row);
            RenumberRows();
            UpdateCount();
        };
        Grid.SetColumn(btnDel, 6);
        row.Children.Add(btnDel);

        pnlRows.Children.Add(row);
        UpdateCount();

        // Focus the suffix field
        Dispatcher.BeginInvoke(() => txtSuffix.Focus());
    }

    /// <summary>
    /// Creates a searchable merchant field (TextBox + Popup + ListBox) for each entry row.
    /// The selected MerchantComboItem is stored in the TextBox.DataContext.
    /// </summary>
    private Grid CreateSearchableMerchantField()
    {
        var container = new Grid { Tag = "merchantContainer", Margin = new Thickness(2) };

        var searchBox = new TextBox
        {
            Style = (Style)FindResource("ModernTextBox"),
            Tag = "merchantSearch",
            FontSize = 13
        };

        var popup = new Popup
        {
            PlacementTarget = searchBox,
            Placement = PlacementMode.Bottom,
            StaysOpen = false,
            AllowsTransparency = true
        };

        var popupBorder = new Border
        {
            Background = (Brush)FindResource("BgCardBrush"),
            CornerRadius = new CornerRadius(8),
            BorderBrush = (Brush)FindResource("AccentPrimaryBrush"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(0),
            Margin = new Thickness(0, 2, 0, 0),
            MinWidth = 250,
            Effect = new DropShadowEffect { BlurRadius = 12, Opacity = 0.3, ShadowDepth = 4 }
        };

        var listBox = new ListBox
        {
            MaxHeight = 200,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Foreground = (Brush)FindResource("TextPrimaryBrush"),
            FontSize = 12
        };

        // ListBox item style
        var itemStyle = new Style(typeof(ListBoxItem));
        itemStyle.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(8, 6, 8, 6)));
        itemStyle.Setters.Add(new Setter(ListBoxItem.CursorProperty, Cursors.Hand));
        listBox.ItemContainerStyle = itemStyle;

        popupBorder.Child = listBox;
        popup.Child = popupBorder;

        bool suppressSearch = false;

        // TextChanged — filter merchants
        searchBox.TextChanged += (s, e) =>
        {
            if (suppressSearch) return;
            var query = searchBox.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                searchBox.DataContext = null;
                popup.IsOpen = false;
                return;
            }
            if (searchBox.DataContext is MerchantComboItem sel && query == sel.merchantName) return;
            searchBox.DataContext = null;
            var filtered = _merchants.Where(m => m.merchantName.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(50).ToList();
            listBox.ItemsSource = filtered;
            popup.IsOpen = filtered.Count > 0;
        };

        // GotFocus — show all if empty
        searchBox.GotFocus += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(searchBox.Text) && _merchants.Count > 0)
            {
                listBox.ItemsSource = _merchants.Take(50).ToList();
                popup.IsOpen = true;
            }
        };

        // Selection — pick merchant
        listBox.SelectionChanged += (s, e) =>
        {
            if (listBox.SelectedItem is MerchantComboItem item)
            {
                searchBox.DataContext = item;
                suppressSearch = true;
                searchBox.Text = item.merchantName;
                suppressSearch = false;
                popup.IsOpen = false;
                listBox.SelectedItem = null;
                // Move focus to amount field
                var parentRow = container.Parent as Grid;
                if (parentRow != null)
                {
                    var amountBox = parentRow.Children.OfType<TextBox>().FirstOrDefault(t => t.Tag?.ToString() == "amount");
                    amountBox?.Focus();
                }
            }
        };

        // Enter key on search box moves to amount
        searchBox.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                popup.IsOpen = false;
                var parentRow = container.Parent as Grid;
                if (parentRow != null)
                {
                    var amountBox = parentRow.Children.OfType<TextBox>().FirstOrDefault(t => t.Tag?.ToString() == "amount");
                    amountBox?.Focus();
                }
            }
        };

        container.Children.Add(searchBox);
        container.Children.Add(popup);
        return container;
    }

    private void RenumberRows()
    {
        for (int i = 0; i < pnlRows.Children.Count; i++)
        {
            if (pnlRows.Children[i] is Grid row)
            {
                var lbl = row.Children.OfType<TextBlock>().FirstOrDefault();
                if (lbl != null) lbl.Text = (i + 1).ToString();
            }
        }
    }

    private void UpdateCount()
    {
        int validCount = 0;
        foreach (var child in pnlRows.Children)
        {
            if (child is Grid row)
            {
                var suffix = GetFieldValue(row, "suffix");
                if (!string.IsNullOrWhiteSpace(suffix)) validCount++;
            }
        }
        txtCount.Text = validCount.ToString();
    }

    // ═══════════════════════════════════════
    // INVOICE NUMBER LOGIC
    // ═══════════════════════════════════════
    private void TxtSuffix_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox txt) return;
        var row = txt.Parent as Grid;
        if (row == null) return;

        var lblFull = row.Children.OfType<TextBlock>().FirstOrDefault(t => t.Tag?.ToString() == "fullNumber");
        if (lblFull == null) return;

        string input = txt.Text.Trim();
        if (string.IsNullOrEmpty(input))
        {
            lblFull.Text = "";
            lblFull.Foreground = (Brush)FindResource("AccentPrimaryBrush");
            return;
        }

        string fullNumber;
        int suffixDigits = _totalDigits - _prefix.Length;

        if (input.Length > suffixDigits)
        {
            // User typed full number (4-6 digits) — use as-is
            fullNumber = input;
        }
        else
        {
            // User typed suffix — prepend prefix
            fullNumber = _prefix + input.PadLeft(suffixDigits, '0');
            // Don't pad if they're still typing
            if (input.Length < suffixDigits)
                fullNumber = _prefix + input;
        }

        lblFull.Text = fullNumber;
        lblFull.Foreground = (Brush)FindResource("AccentPrimaryBrush");

        // Check for duplicate in current batch
        bool isDuplicateInBatch = false;
        foreach (var child in pnlRows.Children)
        {
            if (child is Grid otherRow && otherRow != row)
            {
                var otherFull = otherRow.Children.OfType<TextBlock>()
                    .FirstOrDefault(t => t.Tag?.ToString() == "fullNumber");
                if (otherFull?.Text == fullNumber)
                {
                    isDuplicateInBatch = true;
                    break;
                }
            }
        }

        if (isDuplicateInBatch)
        {
            lblFull.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            txtWarning.Text = $"⚠️ رقم الفاتورة {fullNumber} مكرر في هذا الإدخال";
            txtWarning.Visibility = Visibility.Visible;
        }
        else
        {
            txtWarning.Visibility = Visibility.Collapsed;
        }

        UpdateCount();
    }

    // ═══════════════════════════════════════
    // KEYBOARD NAVIGATION — Enter = next field/row
    // ═══════════════════════════════════════
    private void TxtField_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;

        var element = sender as FrameworkElement;
        var row = element?.Parent as Grid;
        if (row == null) return;

        string? tag = element?.Tag?.ToString();

        // If Enter on notes field (last column) — add new row
        if (tag == "notes")
        {
            int rowIndex = pnlRows.Children.IndexOf(row);
            if (rowIndex == pnlRows.Children.Count - 1)
            {
                // Last row — add new row
                AddEntryRow();
            }
            else
            {
                // Move to next row's suffix field
                if (pnlRows.Children[rowIndex + 1] is Grid nextRow)
                {
                    var nextSuffix = nextRow.Children.OfType<TextBox>()
                        .FirstOrDefault(t => t.Tag?.ToString() == "suffix");
                    nextSuffix?.Focus();
                }
            }
            return;
        }

        // Move to next field in same row (skip merchant container — handled by its own Enter)
        var focusable = row.Children.OfType<FrameworkElement>()
            .Where(c => c is TextBox)
            .ToList();
        int idx = focusable.IndexOf(element!);
        if (idx < focusable.Count - 1)
            focusable[idx + 1].Focus();
    }

    // ═══════════════════════════════════════
    // SAVE SESSION
    // ═══════════════════════════════════════
    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRoute == null)
        {
            ToastHelper.ShowError(RootGrid, "يجب اختيار الخط");
            return;
        }

        var entries = new List<object>();
        var errors = new List<string>();

        for (int i = 0; i < pnlRows.Children.Count; i++)
        {
            if (pnlRows.Children[i] is not Grid row) continue;

            string suffix = GetFieldValue(row, "suffix");
            if (string.IsNullOrWhiteSpace(suffix)) continue; // Skip empty rows

            // Get full number
            var lblFull = row.Children.OfType<TextBlock>()
                .FirstOrDefault(t => t.Tag?.ToString() == "fullNumber");
            string fullNumber = lblFull?.Text ?? "";

            if (string.IsNullOrWhiteSpace(fullNumber))
            {
                errors.Add($"سطر {i + 1}: رقم الفاتورة فارغ");
                continue;
            }

            // Get merchant from searchable field
            var merchantContainer = row.Children.OfType<Grid>()
                .FirstOrDefault(g => g.Tag?.ToString() == "merchantContainer");
            var merchantSearchBox = merchantContainer?.Children.OfType<TextBox>()
                .FirstOrDefault(t => t.Tag?.ToString() == "merchantSearch");
            var selectedMerchant = merchantSearchBox?.DataContext as MerchantComboItem;

            if (selectedMerchant == null)
            {
                errors.Add($"سطر {i + 1}: يجب اختيار العميل");
                continue;
            }

            // Get amount
            decimal? amount = null;
            string amountStr = GetFieldValue(row, "amount");
            if (!string.IsNullOrWhiteSpace(amountStr))
            {
                if (decimal.TryParse(amountStr, out decimal amt))
                    amount = amt;
                else
                    errors.Add($"سطر {i + 1}: المبلغ غير صالح");
            }

            entries.Add(new
            {
                InvoiceNumber = fullNumber,
                MerchantId = selectedMerchant.merchantId,
                Amount = amount,
                Notes = GetFieldValue(row, "notes")
            });
        }

        if (errors.Count > 0)
        {
            ToastHelper.ShowError(RootGrid, string.Join("\n", errors));
            return;
        }

        if (entries.Count == 0)
        {
            ToastHelper.ShowError(RootGrid, "لا توجد فواتير للحفظ");
            return;
        }

        try
        {
            btnSave.IsEnabled = false;
            var dto = new
            {
                SessionDate = dpDate.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd"),
                RouteId = _selectedRoute.RouteId,
                DriverId = _selectedDriver?.driverId > 0 ? (int?)_selectedDriver.driverId : null,
                Notes = (string?)null,
                Entries = entries
            };

            var result = await _api.CreateAjalSessionAsync(dto);
            if (result != null)
            {
                bool success = (bool)(result.success ?? false);
                if (success)
                {
                    int saved = (int)(result.entriesSaved ?? 0);
                    ToastHelper.ShowSuccess(RootGrid, $"تم حفظ {saved} فاتورة بنجاح ✅");

                    // Check for warnings
                    var warnings = result.warnings as JArray;
                    if (warnings != null && warnings.Count > 0)
                    {
                        txtWarning.Text = "⚠️ " + string.Join(" | ", warnings);
                        txtWarning.Visibility = Visibility.Visible;
                    }

                    // Clear rows for next entry
                    pnlRows.Children.Clear();
                    AddEntryRow();
                }
                else
                {
                    var errs = result.errors as JArray;
                    ToastHelper.ShowError(RootGrid, errs != null ? string.Join("\n", errs) : "خطأ في الحفظ");
                }
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, $"خطأ: {ex.Message}");
        }
        finally
        {
            btnSave.IsEnabled = true;
        }
    }

    // ═══════════════════════════════════════
    // NEW SESSION (clear for another route)
    // ═══════════════════════════════════════
    private void BtnNewSession_Click(object sender, RoutedEventArgs e)
    {
        pnlRows.Children.Clear();
        AddEntryRow();
        txtWarning.Visibility = Visibility.Collapsed;
        RouteSearchBox.Focus();
    }

    // ═══════════════════════════════════════
    // CHANGE PREFIX
    // ═══════════════════════════════════════
    private async void BtnChangePrefix_Click(object sender, RoutedEventArgs e)
    {
        string? newPrefix = Microsoft.VisualBasic.Interaction.InputBox(
            "أدخل البادئة الجديدة:", "تغيير البادئة", _prefix);

        if (!string.IsNullOrWhiteSpace(newPrefix))
        {
            newPrefix = newPrefix.Trim();
            if (newPrefix.Length >= 2 && newPrefix.Length <= 4 && int.TryParse(newPrefix, out _))
            {
                bool ok = await _api.UpdateAjalPrefixAsync(newPrefix);
                if (ok)
                {
                    _prefix = newPrefix;
                    txtPrefix.Text = _prefix;
                    ToastHelper.ShowSuccess(RootGrid, $"تم تغيير البادئة إلى: {_prefix}");
                }
            }
            else
            {
                ToastHelper.ShowError(RootGrid, "البادئة يجب أن تكون 2-4 أرقام");
            }
        }
    }

    // ═══════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════
    private static string GetFieldValue(Grid row, string tag)
    {
        var txt = row.Children.OfType<TextBox>().FirstOrDefault(t => t.Tag?.ToString() == tag);
        return txt?.Text?.Trim() ?? "";
    }

    /// <summary>
    /// Accepts either a raw JSON array or an object with a <c>data</c> array property.
    /// </summary>
    private static JArray ParseJsonArray(string json)
    {
        var token = JToken.Parse(json);
        if (token is JArray arr) return arr;
        if (token is JObject obj)
        {
            foreach (var name in new[] { "data", "Data", "items", "results", "merchants", "drivers" })
            {
                var child = obj[name];
                if (child is JArray childArr) return childArr;
                if (child is JObject nested)
                {
                    var inner = nested["items"] as JArray
                        ?? nested["results"] as JArray
                        ?? nested["data"] as JArray
                        ?? nested["merchants"] as JArray;
                    if (inner != null) return inner;
                }
            }

            foreach (var prop in obj.Properties())
            {
                if (prop.Value is JArray nestedArr) return nestedArr;
            }
        }
        return new JArray();
    }

    private static JObject UnwrapObject(string json)
    {
        var token = JToken.Parse(json);
        if (token is not JObject obj) return new JObject();
        if (obj["prefix"] != null || obj["totalDigits"] != null) return obj;
        if (obj["data"] is JObject inner) return inner;
        return obj;
    }
}
