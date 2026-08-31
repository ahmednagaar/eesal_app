using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Models;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class NewSessionPage : UserControl
{
    private readonly ApiClient _api;
    private readonly List<ReceiptEntryRow> _rows = new();
    private int _nextReceiptNumber = 0;
    private int _selectedBookId = 0;
    private bool _isSaving = false;

    // Merchant search debounce
    private DispatcherTimer? _searchTimer;
    private TextBox? _activeSearchBox;

    public NewSessionPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        SessionDate.SelectedDate = DateTime.Today;
        Loaded += async (_, _) => await LoadFormDataAsync();
    }

    // ══════════════════════════════════════
    // INITIALIZATION
    // ══════════════════════════════════════

    private async Task LoadFormDataAsync()
    {
        try
        {
            // Load drivers
            var driversJson = await _api.GetDriversJsonAsync();
            if (driversJson != null)
            {
                var drivers = JArray.Parse(driversJson);
                var driverList = drivers.Select(d => new DriverComboItem
                {
                    driverId = d["driverId"]?.Value<int>() ?? 0,
                    fullName = d["fullName"]?.ToString() ?? ""
                }).ToList();
                DriverCombo.ItemsSource = driverList;
            }

            // Load available books
            var booksJson = await _api.GetAvailableBooksJsonAsync();
            if (booksJson != null)
            {
                var books = JArray.Parse(booksJson);
                var bookList = books.Select(b => new BookComboItem
                {
                    bookId = b["bookId"]?.Value<int>() ?? 0,
                    display = $"دفتر {b["bookNumber"]} ({b["startReceiptNumber"]}–{b["endReceiptNumber"]})"
                }).ToList();
                BookCombo.ItemsSource = bookList;
                if (bookList.Count > 0)
                    BookCombo.SelectedIndex = 0;
            }

            // Add first empty row
            AddReceiptRow();
        }
        catch (Exception ex)
        {
            ErrorMessageHelper.LogError(ex);
        }
    }

    // ══════════════════════════════════════
    // DRIVER SELECTION → RECEIPT HINT
    // ══════════════════════════════════════

    private async void DriverCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DriverCombo.SelectedValue == null) return;
        int driverId = (int)DriverCombo.SelectedValue;

        try
        {
            var lastJson = await _api.GetLastSessionForDriverJsonAsync(driverId);
            if (lastJson != null)
            {
                var last = JObject.Parse(lastJson);
                var lastNum = last["lastReceiptNumber"]?.Value<int>() ?? 0;
                _nextReceiptNumber = lastNum + 1;
                LastReceiptHint.Text = $"آخر إيصال: {lastNum} — الإيصال التالي المتوقع: {_nextReceiptNumber}";
                LastReceiptHint.Foreground = (SolidColorBrush)FindResource("AccentSuccessBrush");

                // Auto-fill first row if empty
                if (_rows.Count > 0 && _rows[0].ReceiptNumber == 0)
                {
                    _rows[0].ReceiptNumber = _nextReceiptNumber;
                    UpdateRowUI(_rows[0], 0);
                }
            }
            else
            {
                _nextReceiptNumber = 0;
                LastReceiptHint.Text = "لا توجد جلسات سابقة لهذا السائق";
                LastReceiptHint.Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush");
            }
        }
        catch (Exception ex)
        {
            ErrorMessageHelper.LogError(ex);
            LastReceiptHint.Text = "تعذر تحميل بيانات السائق";
        }
    }

    // ══════════════════════════════════════
    // RECEIPT ROW MANAGEMENT
    // ══════════════════════════════════════

    private void AddReceiptRow(int? presetReceiptNumber = null)
    {
        var row = new ReceiptEntryRow();
        int index = _rows.Count;

        // Auto-increment receipt number
        if (presetReceiptNumber.HasValue)
            row.ReceiptNumber = presetReceiptNumber.Value;
        else if (_rows.Count > 0 && _rows.Last().ReceiptNumber > 0)
            row.ReceiptNumber = _rows.Last().ReceiptNumber + 1;
        else if (_nextReceiptNumber > 0)
            row.ReceiptNumber = _nextReceiptNumber;

        _rows.Add(row);
        var rowUI = CreateRowUI(row, index);
        ReceiptRows.Children.Add(rowUI);

        // Focus receipt number of new row
        Dispatcher.BeginInvoke(() =>
        {
            var receiptBox = FindTextBoxInRow(rowUI, "ReceiptNumberBox");
            receiptBox?.Focus();
            receiptBox?.SelectAll();
        }, DispatcherPriority.Background);

        UpdateTotals();
    }

    private Border CreateRowUI(ReceiptEntryRow row, int index)
    {
        var border = new Border
        {
            Padding = new Thickness(16, 8, 16, 8),
            Margin = new Thickness(0, 1, 0, 0),
            Background = index % 2 == 0 ? (Brush)FindResource("BgCardBrush") : (Brush)FindResource("BgSurfaceBrush"),
            Tag = row
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

        // Index number
        var indexText = new TextBlock
        {
            Text = (index + 1).ToString(),
            FontSize = 12, Foreground = (Brush)FindResource("TextMutedBrush"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(indexText, 0);
        grid.Children.Add(indexText);

        // Receipt number
        var receiptBox = new TextBox
        {
            Text = row.ReceiptNumber > 0 ? row.ReceiptNumber.ToString() : "",
            Style = (Style)FindResource("ModernTextBox"),
            Height = 34, FontSize = 13, Name = "ReceiptNumberBox",
            Tag = row
        };
        receiptBox.LostFocus += ReceiptNumber_LostFocus;
        receiptBox.TextChanged += (s, e) =>
        {
            if (int.TryParse(receiptBox.Text, out int num))
                row.ReceiptNumber = num;
        };
        Grid.SetColumn(receiptBox, 1);
        grid.Children.Add(receiptBox);

        // Merchant search (TextBox + Popup)
        var merchantPanel = new Grid();
        var merchantBox = new TextBox
        {
            Text = row.MerchantDisplayName,
            Style = (Style)FindResource("ModernTextBox"),
            Height = 34, FontSize = 13, Name = "MerchantBox",
            Tag = row
        };
        merchantBox.TextChanged += MerchantBox_TextChanged;
        merchantBox.GotFocus += (s, e) =>
        {
            var popup = FindPopupInParent(merchantBox);
            if (popup != null && popup.Child is ListBox lb && lb.Items.Count > 0)
                popup.IsOpen = true;
        };

        var merchantPopup = new System.Windows.Controls.Primitives.Popup
        {
            PlacementTarget = merchantBox,
            Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
            Width = 300, MaxHeight = 200,
            StaysOpen = false,
            Name = "MerchantPopup"
        };
        var merchantList = new ListBox
        {
            Background = (Brush)FindResource("BgCardBrush"),
            Foreground = (Brush)FindResource("TextPrimaryBrush"),
            BorderBrush = (Brush)FindResource("BorderSubtleBrush"),
            BorderThickness = new Thickness(1),
            FontSize = 13,
            Tag = row
        };
        merchantList.MouseDoubleClick += MerchantList_Selected;
        merchantList.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter) MerchantList_Selected(s, null!);
        };
        merchantPopup.Child = merchantList;

        merchantPanel.Children.Add(merchantBox);
        merchantPanel.Children.Add(merchantPopup);
        Grid.SetColumn(merchantPanel, 2);
        merchantPanel.Margin = new Thickness(8, 0, 8, 0);
        grid.Children.Add(merchantPanel);

        // Amount
        var amountBox = new TextBox
        {
            Text = row.Amount > 0 ? row.Amount.ToString("F2") : "",
            Style = (Style)FindResource("ModernTextBox"),
            Height = 34, FontSize = 13, Name = "AmountBox",
            FlowDirection = FlowDirection.LeftToRight,
            TextAlignment = TextAlignment.Right,
            Tag = row
        };
        amountBox.TextChanged += (s, e) =>
        {
            if (decimal.TryParse(amountBox.Text, out decimal amt))
                row.Amount = amt;
            else
                row.Amount = 0;
            UpdateTotals();
        };
        // Enter key → add new row
        amountBox.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                AddReceiptRow();
            }
        };
        Grid.SetColumn(amountBox, 3);
        grid.Children.Add(amountBox);

        // Partial payment checkbox
        var partialCheck = new CheckBox
        {
            IsChecked = row.IsPartialPayment,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Tag = row
        };
        partialCheck.Checked += (s, e) => row.IsPartialPayment = true;
        partialCheck.Unchecked += (s, e) => row.IsPartialPayment = false;
        Grid.SetColumn(partialCheck, 4);
        grid.Children.Add(partialCheck);

        // Notes
        var notesBox = new TextBox
        {
            Text = row.Notes ?? "",
            Style = (Style)FindResource("ModernTextBox"),
            Height = 34, FontSize = 12, Name = "NotesBox",
            Tag = row
        };
        notesBox.TextChanged += (s, e) => row.Notes = notesBox.Text;
        Grid.SetColumn(notesBox, 5);
        notesBox.Margin = new Thickness(8, 0, 0, 0);
        grid.Children.Add(notesBox);

        // Delete button
        var deleteBtn = new Button
        {
            Content = "✕", FontSize = 14,
            Background = Brushes.Transparent,
            Foreground = (Brush)FindResource("AccentDangerBrush"),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center,
            Tag = row
        };
        deleteBtn.Click += DeleteRow_Click;
        Grid.SetColumn(deleteBtn, 6);
        grid.Children.Add(deleteBtn);

        border.Child = grid;
        return border;
    }

    private void UpdateRowUI(ReceiptEntryRow row, int index)
    {
        if (index < ReceiptRows.Children.Count)
        {
            var border = (Border)ReceiptRows.Children[index];
            var grid = (Grid)border.Child;
            var receiptBox = FindTextBoxInRow(border, "ReceiptNumberBox");
            if (receiptBox != null && row.ReceiptNumber > 0)
                receiptBox.Text = row.ReceiptNumber.ToString();
        }
    }

    private void DeleteRow_Click(object sender, RoutedEventArgs e)
    {
        if (_rows.Count <= 1) return; // Keep at least one row

        var btn = (Button)sender;
        var row = (ReceiptEntryRow)btn.Tag;
        int idx = _rows.IndexOf(row);
        if (idx >= 0)
        {
            _rows.RemoveAt(idx);
            ReceiptRows.Children.RemoveAt(idx);
            // Reindex remaining rows
            for (int i = 0; i < ReceiptRows.Children.Count; i++)
            {
                var border = (Border)ReceiptRows.Children[i];
                border.Background = i % 2 == 0 ? (Brush)FindResource("BgCardBrush") : (Brush)FindResource("BgSurfaceBrush");
                var grid = (Grid)border.Child;
                if (grid.Children[0] is TextBlock tb) tb.Text = (i + 1).ToString();
            }
            UpdateTotals();
        }
    }

    private void AddRow_Click(object sender, RoutedEventArgs e)
    {
        AddReceiptRow();
    }

    // ══════════════════════════════════════
    // MERCHANT SEARCH
    // ══════════════════════════════════════

    private void MerchantBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var box = (TextBox)sender;
        _activeSearchBox = box;

        _searchTimer?.Stop();
        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _searchTimer.Tick += async (s, _) =>
        {
            _searchTimer!.Stop();
            await SearchMerchants(box);
        };
        _searchTimer.Start();
    }

    private async Task SearchMerchants(TextBox box)
    {
        var query = box.Text.Trim();
        if (query.Length < 2) return;

        try
        {
            var json = await _api.SearchMerchantsJsonAsync(query);
            if (json == null) return;

            var results = JArray.Parse(json);
            var popup = FindPopupInParent(box);
            if (popup?.Child is ListBox lb)
            {
                lb.Items.Clear();
                foreach (var m in results)
                {
                    lb.Items.Add(new ListBoxItem
                    {
                        Content = m["merchantName"]?.ToString(),
                        Tag = m["merchantId"]?.Value<int>() ?? 0,
                        Foreground = (Brush)FindResource("TextPrimaryBrush"),
                        Padding = new Thickness(8, 6, 8, 6)
                    });
                }

                if (lb.Items.Count > 0)
                    popup.IsOpen = true;
                else
                    popup.IsOpen = false;
            }
        }
        catch { }
    }

    private void MerchantList_Selected(object sender, MouseButtonEventArgs e)
    {
        var lb = (ListBox)sender;
        if (lb.SelectedItem is ListBoxItem item)
        {
            var row = (ReceiptEntryRow)lb.Tag;
            row.MerchantId = (int)item.Tag;
            row.MerchantDisplayName = item.Content?.ToString() ?? "";

            // Update TextBox
            var popup = FindPopupInParent(lb);
            if (popup != null)
            {
                popup.IsOpen = false;
                var merchantBox = popup.PlacementTarget as TextBox;
                if (merchantBox != null)
                    merchantBox.Text = row.MerchantDisplayName;
            }

            // Move focus to amount box
            var parentBorder = FindParentBorder(lb);
            if (parentBorder != null)
            {
                var amountBox = FindTextBoxInRow(parentBorder, "AmountBox");
                amountBox?.Focus();
            }
        }
    }

    // ══════════════════════════════════════
    // DUPLICATE CHECK
    // ══════════════════════════════════════

    private async void ReceiptNumber_LostFocus(object sender, RoutedEventArgs e)
    {
        var box = (TextBox)sender;
        var row = (ReceiptEntryRow)box.Tag;

        if (!int.TryParse(box.Text, out int num) || num <= 0) return;

        // Check for duplicates within current list
        var dupeInList = _rows.Count(r => r != row && r.ReceiptNumber == num) > 0;
        if (dupeInList)
        {
            box.BorderBrush = (Brush)FindResource("AccentDangerBrush");
            row.IsDuplicate = true;
            row.DuplicateWarning = "مكرر في القائمة الحالية";
            box.ToolTip = row.DuplicateWarning;
            return;
        }

        // Check server
        try
        {
            var json = await _api.CheckDuplicateJsonAsync(num);
            if (json != null)
            {
                var result = JObject.Parse(json);
                bool exists = result["exists"]?.Value<bool>() ?? false;
                if (exists)
                {
                    box.BorderBrush = (Brush)FindResource("AccentDangerBrush");
                    row.IsDuplicate = true;
                    row.DuplicateWarning = $"الإيصال {num} موجود بالفعل في النظام";
                    box.ToolTip = row.DuplicateWarning;
                }
                else
                {
                    box.BorderBrush = (Brush)FindResource("BorderSubtleBrush");
                    row.IsDuplicate = false;
                    row.DuplicateWarning = null;
                    box.ToolTip = null;
                }
            }
        }
        catch { }
    }

    // ══════════════════════════════════════
    // TOTALS
    // ══════════════════════════════════════

    private void UpdateTotals()
    {
        var validRows = _rows.Where(r => r.ReceiptNumber > 0 && r.Amount > 0).ToList();
        TotalCount.Text = validRows.Count.ToString();
        TotalAmount.Text = validRows.Sum(r => r.Amount).ToString("N2");
    }

    // ══════════════════════════════════════
    // SAVE
    // ══════════════════════════════════════

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_isSaving) return;

        // Validation
        if (DriverCombo.SelectedValue == null)
        {
            ToastHelper.ShowError(RootGrid, "يرجى اختيار السائق");
            return;
        }
        if (SessionDate.SelectedDate == null)
        {
            ToastHelper.ShowError(RootGrid, "يرجى اختيار التاريخ");
            return;
        }
        if (string.IsNullOrWhiteSpace(RouteAreaBox.Text))
        {
            ToastHelper.ShowError(RootGrid, "يرجى إدخال المنطقة / الخط");
            return;
        }

        var validRows = _rows.Where(r => r.ReceiptNumber > 0 && r.MerchantId > 0 && r.Amount > 0).ToList();
        if (validRows.Count == 0)
        {
            ToastHelper.ShowError(RootGrid, "يرجى إدخال إيصال واحد على الأقل (رقم + تاجر + مبلغ)");
            return;
        }

        // Check for duplicates
        var dupes = validRows.Where(r => r.IsDuplicate).ToList();
        if (dupes.Count > 0)
        {
            ToastHelper.ShowError(RootGrid, $"يوجد {dupes.Count} إيصال مكرر — يرجى تصحيحها أولاً");
            return;
        }

        // Get book ID
        int? bookId = null;
        if (BookCombo.SelectedValue is int bid && bid > 0)
            bookId = bid;

        // Build DTO
        var dto = new
        {
            DriverId = (int)DriverCombo.SelectedValue,
            SessionDate = SessionDate.SelectedDate!.Value.ToString("yyyy-MM-dd"),
            RouteArea = RouteAreaBox.Text.Trim(),
            Notes = (string?)null,
            Receipts = validRows.Select(r => new
            {
                ReceiptNumber = r.ReceiptNumber,
                BookId = bookId,
                MerchantId = r.MerchantId,
                Amount = r.Amount,
                IsPartialPayment = r.IsPartialPayment,
                Notes = r.Notes
            }).ToList()
        };

        // Double-submit protection
        _isSaving = true;
        SaveBtn.IsEnabled = false;
        LoadingOverlay.Visibility = Visibility.Visible;

        try
        {
            var result = await _api.CreateSessionAsync(dto);
            if (result != null)
            {
                bool hasGaps = result["hasGaps"]?.Value<bool>() ?? false;
                if (hasGaps)
                {
                    var missing = result["missingReceipts"] as JArray;
                    var nums = missing?.Select(n => n.Value<int>()).ToList() ?? new List<int>();
                    GapWarningText.Text = result["message"]?.ToString() ?? "تم اكتشاف إيصالات مفقودة";
                    GapWarningNumbers.Text = $"الأرقام المفقودة: {string.Join(", ", nums)}";
                    GapWarningOverlay.Visibility = Visibility.Visible;
                }
                else
                {
                    ToastHelper.ShowSuccess(RootGrid, "✓ تم حفظ الجلسة بنجاح");
                    ClearForm();
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessageHelper.LogError(ex);
            var msg = ex.Message.Contains("مسجل") || ex.Message.Contains("مكرر") || ex.Message.Contains("موجود")
                ? ex.Message
                : ErrorMessageHelper.GetArabicMessage(ex);
            ToastHelper.ShowError(RootGrid, msg);
        }
        finally
        {
            _isSaving = false;
            SaveBtn.IsEnabled = true;
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void DismissGapWarning_Click(object sender, RoutedEventArgs e)
    {
        GapWarningOverlay.Visibility = Visibility.Collapsed;
        ToastHelper.ShowSuccess(RootGrid, "✓ تم حفظ الجلسة بنجاح (مع فجوات)");
        ClearForm();
    }

    private void ClearForm()
    {
        _rows.Clear();
        ReceiptRows.Children.Clear();
        RouteAreaBox.Text = "";
        UpdateTotals();

        // Re-trigger driver hint to get next receipt number
        if (DriverCombo.SelectedValue != null)
            DriverCombo_SelectionChanged(DriverCombo, null!);

        AddReceiptRow();
    }

    // ══════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════

    private TextBox? FindTextBoxInRow(DependencyObject parent, string name)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is TextBox tb && tb.Name == name)
                return tb;
            var result = FindTextBoxInRow(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private System.Windows.Controls.Primitives.Popup? FindPopupInParent(DependencyObject element)
    {
        var parent = VisualTreeHelper.GetParent(element);
        while (parent != null)
        {
            if (parent is Grid g)
            {
                foreach (UIElement child in g.Children)
                {
                    if (child is System.Windows.Controls.Primitives.Popup popup)
                        return popup;
                }
            }
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    private Border? FindParentBorder(DependencyObject element)
    {
        var parent = VisualTreeHelper.GetParent(element);
        while (parent != null)
        {
            if (parent is Border b && b.Tag is ReceiptEntryRow)
                return b;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
}
