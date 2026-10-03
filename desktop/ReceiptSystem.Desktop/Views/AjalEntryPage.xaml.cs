using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class AjalEntryPage : Page
{
    private readonly ApiClient _api;
    private string _prefix = "441";
    private int _totalDigits = 6;
    private List<dynamic> _routes = new();
    private List<dynamic> _drivers = new();
    private List<dynamic> _merchants = new();

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
        try
        {
            // Routes
            var routes = await _api.GetRoutesAsync();
            _routes = routes.Select(r => (dynamic)new { r.RouteId, r.RouteName }).ToList();
            cmbRoute.ItemsSource = _routes;
            if (_routes.Count > 0) cmbRoute.SelectedIndex = 0;

            // Drivers
            var driversJson = await _api.GetDriversJsonAsync();
            if (driversJson != null)
            {
                _drivers = JsonConvert.DeserializeObject<List<dynamic>>(driversJson) ?? new();
                cmbDriver.ItemsSource = _drivers;
                if (_drivers.Count > 0) cmbDriver.SelectedIndex = 0;
            }

            // Merchants
            var merchantsJson = await _api.GetMerchantsJsonAsync(1, 10000);
            if (merchantsJson != null)
                _merchants = JsonConvert.DeserializeObject<List<dynamic>>(merchantsJson) ?? new();

            // Prefix settings
            var prefixJson = await _api.GetAjalPrefixSettingsJsonAsync();
            if (prefixJson != null)
            {
                var settings = JObject.Parse(prefixJson);
                _prefix = settings["prefix"]?.ToString() ?? "441";
                _totalDigits = (int)(settings["totalDigits"] ?? 6);
                txtPrefix.Text = _prefix;
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, $"خطأ في تحميل البيانات: {ex.Message}");
        }
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

        // Merchant ComboBox
        var cmbMerchant = new ComboBox
        {
            
            IsEditable = true, IsTextSearchEnabled = true,
            DisplayMemberPath = "MerchantName",
            SelectedValuePath = "MerchantId",
            ItemsSource = _merchants,
            Tag = "merchant",
            Margin = new Thickness(2)
        };
        cmbMerchant.KeyDown += TxtField_KeyDown;
        Grid.SetColumn(cmbMerchant, 3);
        row.Children.Add(cmbMerchant);

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
    // KEYBOARD NAVIGATION — Enter = next row
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

        // Move to next field in same row
        var focusable = row.Children.OfType<FrameworkElement>()
            .Where(c => c is TextBox || c is ComboBox)
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
        if (cmbRoute.SelectedValue == null)
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

            // Get merchant
            var cmbMerchant = row.Children.OfType<ComboBox>()
                .FirstOrDefault(c => c.Tag?.ToString() == "merchant");
            if (cmbMerchant?.SelectedValue == null)
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
                MerchantId = (int)cmbMerchant.SelectedValue,
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
                RouteId = (int)cmbRoute.SelectedValue,
                DriverId = cmbDriver.SelectedValue != null ? (int?)cmbDriver.SelectedValue : null,
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
        cmbRoute.Focus();
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
}
