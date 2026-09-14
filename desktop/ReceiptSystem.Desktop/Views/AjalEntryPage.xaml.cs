using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class AjalEntryPage : UserControl
{
    private readonly ApiClient _api;
    private string _prefix = "";
    private List<string> _employeeNames = new();
    private int _rowCounter = 0;

    public AjalEntryPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        SessionDate.SelectedDate = DateTime.Today;
        Loaded += async (_, _) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            // Load routes
            var routes = await _api.GetRoutesAsync();
            var routeList = new List<object> { new { RouteId = (int?)null, RouteName = "— بدون خط —" } };
            routeList.AddRange(routes.Select(r => (object)new { RouteId = (int?)r.RouteId, RouteName = r.RouteName }));
            RouteCombo.ItemsSource = routeList;
            RouteCombo.SelectedIndex = 0;

            // Load prefix settings
            await LoadPrefixAsync();

            // Load employee names for autocomplete
            await LoadEmployeeNamesAsync();

            // Add initial rows
            for (int i = 0; i < 3; i++) AddEntryRow();
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, ErrorMessageHelper.GetArabicMessage(ex));
        }
    }

    private async Task LoadPrefixAsync()
    {
        var json = await _api.GetAjalPrefixSettingsJsonAsync();
        if (json != null)
        {
            var settings = JObject.Parse(json);
            _prefix = settings["prefix"]?.ToString() ?? "";
            PrefixDisplay.Text = _prefix;
        }
    }

    private async Task LoadEmployeeNamesAsync()
    {
        var json = await _api.GetAjalEmployeeNamesJsonAsync();
        if (json != null)
        {
            _employeeNames = JArray.Parse(json).Select(t => t.ToString()).ToList();
        }
    }

    // ══════════════════════════════════════
    // Entry Row Management
    // ══════════════════════════════════════

    private void AddRow_Click(object sender, RoutedEventArgs e) => AddEntryRow();

    private void AddFiveRows_Click(object sender, RoutedEventArgs e)
    {
        for (int i = 0; i < 5; i++) AddEntryRow();
    }

    private void AddEntryRow()
    {
        _rowCounter++;
        var rowBorder = new Border
        {
            Background = (Brush)FindResource("BgCardBrush"),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 8, 12, 8),
            Margin = new Thickness(0, 2, 0, 2),
            Tag = _rowCounter
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });   // #
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });  // Invoice suffix
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Merchant
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });  // Amount
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });  // Employee
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });  // Notes
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });   // Remove

        // Row number
        var numText = new TextBlock
        {
            Text = _rowCounter.ToString(),
            FontSize = 12, VerticalAlignment = VerticalAlignment.Center,
            Foreground = (Brush)FindResource("TextMutedBrush")
        };
        Grid.SetColumn(numText, 0);
        grid.Children.Add(numText);

        // Invoice number suffix (user types last digits, prefix auto-prepended)
        var suffixBox = new TextBox
        {
            Name = "SuffixBox",
            FontSize = 13, Padding = new Thickness(6, 4, 6, 4),
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = $"اكتب الأرقام الأخيرة فقط — البادئة '{_prefix}' تُضاف تلقائياً"
        };
        Grid.SetColumn(suffixBox, 1);
        grid.Children.Add(suffixBox);

        // Merchant search (TextBox + Popup pattern)
        var merchantPanel = new Grid();
        var merchantSearch = new TextBox
        {
            Name = "MerchantSearch",
            FontSize = 13, Padding = new Thickness(6, 4, 6, 4),
            Margin = new Thickness(0, 0, 8, 0),
            Tag = 0 // will store selected MerchantId
        };
        var merchantPopup = new Popup
        {
            PlacementTarget = merchantSearch,
            Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
            Width = 300, StaysOpen = false
        };
        var merchantList = new ListBox
        {
            MaxHeight = 200, FontSize = 12,
            Background = (Brush)FindResource("BgCardBrush"),
            Foreground = (Brush)FindResource("TextPrimaryBrush")
        };
        merchantPopup.Child = merchantList;
        merchantPanel.Children.Add(merchantSearch);
        merchantPanel.Children.Add(merchantPopup);
        Grid.SetColumn(merchantPanel, 2);
        grid.Children.Add(merchantPanel);

        // Wire merchant search
        merchantSearch.TextChanged += async (s, e) =>
        {
            string query = merchantSearch.Text.Trim();
            if (query.Length < 2)
            {
                merchantPopup.IsOpen = false;
                return;
            }
            try
            {
                var results = await _api.SearchMerchantsForRouteAsync(query);
                if (results.Count > 0)
                {
                    merchantList.Items.Clear();
                    foreach (var m in results)
                    {
                        merchantList.Items.Add(new ListBoxItem
                        {
                            Content = m.MerchantName,
                            Tag = m.MerchantId
                        });
                    }
                    merchantPopup.IsOpen = true;
                }
                else
                {
                    merchantPopup.IsOpen = false;
                }
            }
            catch { merchantPopup.IsOpen = false; }
        };

        merchantList.SelectionChanged += (s, e) =>
        {
            if (merchantList.SelectedItem is ListBoxItem selected)
            {
                merchantSearch.Text = selected.Content.ToString();
                merchantSearch.Tag = selected.Tag;
                merchantPopup.IsOpen = false;
            }
        };

        // Amount
        var amountBox = new TextBox
        {
            Name = "AmountBox",
            FontSize = 13, Padding = new Thickness(6, 4, 6, 4),
            Margin = new Thickness(0, 0, 8, 0)
        };
        amountBox.TextChanged += (s, e) => UpdateTotals();
        Grid.SetColumn(amountBox, 3);
        grid.Children.Add(amountBox);

        // Employee name (ComboBox editable)
        var empCombo = new ComboBox
        {
            Name = "EmployeeCombo",
            IsEditable = true, FontSize = 13,
            Padding = new Thickness(4, 2, 4, 2),
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = _employeeNames
        };
        Grid.SetColumn(empCombo, 4);
        grid.Children.Add(empCombo);

        // Notes
        var notesBox = new TextBox
        {
            Name = "NotesBox",
            FontSize = 12, Padding = new Thickness(6, 4, 6, 4),
            Margin = new Thickness(0, 0, 8, 0)
        };
        Grid.SetColumn(notesBox, 5);
        grid.Children.Add(notesBox);

        // Remove button
        var removeBtn = new Button
        {
            Content = "✕", FontSize = 12,
            Width = 28, Height = 28,
            Background = Brushes.Transparent,
            Foreground = (Brush)FindResource("AccentDangerBrush"),
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center
        };
        removeBtn.Click += (s, e) =>
        {
            EntryRows.Children.Remove(rowBorder);
            UpdateTotals();
        };
        Grid.SetColumn(removeBtn, 6);
        grid.Children.Add(removeBtn);

        rowBorder.Child = grid;
        EntryRows.Children.Add(rowBorder);
    }

    private void UpdateTotals()
    {
        int count = 0;
        decimal total = 0;
        foreach (Border rowBorder in EntryRows.Children)
        {
            var grid = rowBorder.Child as Grid;
            if (grid == null) continue;

            var amountBox = grid.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "AmountBox");
            var suffixBox = grid.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "SuffixBox");
            if (amountBox != null && suffixBox != null &&
                !string.IsNullOrWhiteSpace(suffixBox.Text) &&
                decimal.TryParse(amountBox.Text, out decimal amt) && amt > 0)
            {
                count++;
                total += amt;
            }
        }
        InvoiceCountText.Text = count.ToString();
        TotalAmountText.Text = total.ToString("N2");
    }

    // ══════════════════════════════════════
    // Save
    // ══════════════════════════════════════

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (SessionDate.SelectedDate == null)
        {
            ToastHelper.ShowError(RootGrid, "اختر التاريخ أولاً");
            return;
        }

        // Collect rows
        var invoices = new List<object>();
        var errors = new List<string>();
        int rowNum = 0;

        foreach (Border rowBorder in EntryRows.Children)
        {
            rowNum++;
            var grid = rowBorder.Child as Grid;
            if (grid == null) continue;

            var suffixBox = grid.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "SuffixBox");
            var amountBox = grid.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "AmountBox");
            var notesBox = grid.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "NotesBox");
            var empCombo = grid.Children.OfType<ComboBox>().FirstOrDefault(t => t.Name == "EmployeeCombo");

            // Find merchant search in the nested grid
            var merchantPanel = grid.Children.OfType<Grid>().FirstOrDefault();
            var merchantSearch = merchantPanel?.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "MerchantSearch");

            if (suffixBox == null || string.IsNullOrWhiteSpace(suffixBox.Text)) continue; // skip empty rows

            string fullNumber = _prefix + suffixBox.Text.Trim();

            if (!decimal.TryParse(amountBox?.Text, out decimal amount) || amount <= 0)
            {
                errors.Add($"سطر {rowNum}: المبلغ غير صحيح");
                continue;
            }

            int merchantId = 0;
            if (merchantSearch != null && merchantSearch.Tag is int mid) merchantId = mid;
            if (merchantId <= 0)
            {
                errors.Add($"سطر {rowNum}: اختر التاجر");
                continue;
            }

            string? empName = empCombo?.Text;
            if (string.IsNullOrWhiteSpace(empName)) empName = null;

            invoices.Add(new
            {
                InvoiceNumber = fullNumber,
                MerchantId = merchantId,
                CallCenterEmployeeName = empName,
                Amount = amount,
                Notes = notesBox?.Text
            });
        }

        if (errors.Count > 0)
        {
            ToastHelper.ShowError(RootGrid, string.Join("\n", errors));
            return;
        }

        if (invoices.Count == 0)
        {
            ToastHelper.ShowError(RootGrid, "لا توجد فواتير للحفظ — املأ سطراً واحداً على الأقل");
            return;
        }

        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            int? routeId = null;
            if (RouteCombo.SelectedItem != null)
            {
                dynamic selected = RouteCombo.SelectedItem;
                routeId = selected.RouteId;
            }

            var result = await _api.CreateAjalInvoicesAsync(SessionDate.SelectedDate.Value, routeId, invoices);
            if (result == null)
            {
                ToastHelper.ShowError(RootGrid, "فشل حفظ الفواتير");
                return;
            }

            bool success = result["success"]?.Value<bool>() ?? result["Success"]?.Value<bool>() ?? false;
            if (success)
            {
                int saved = result["saved"]?.Value<int>() ?? result["Saved"]?.Value<int>() ?? 0;
                ToastHelper.ShowSuccess(RootGrid, $"✓ تم حفظ {saved} فاتورة بنجاح");

                // Clear rows and add fresh ones
                EntryRows.Children.Clear();
                _rowCounter = 0;
                for (int i = 0; i < 3; i++) AddEntryRow();
                UpdateTotals();

                // Refresh employee names (new names may have been added)
                await LoadEmployeeNamesAsync();
            }
            else
            {
                var apiErrors = result["errors"] ?? result["Errors"];
                if (apiErrors is JArray arr)
                {
                    string msg = string.Join("\n", arr.Select(e2 => e2.ToString()));
                    ToastHelper.ShowError(RootGrid, msg);
                }
                else
                {
                    ToastHelper.ShowError(RootGrid, "فشل حفظ الفواتير");
                }
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, ErrorMessageHelper.GetArabicMessage(ex));
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
