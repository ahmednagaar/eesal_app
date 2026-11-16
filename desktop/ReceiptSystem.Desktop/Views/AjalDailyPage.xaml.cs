using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class AjalDailyPage : UserControl
{
    private readonly ApiClient _api;
    private List<Models.Route> _allRoutes = new();
    private List<DriverComboItem> _allDrivers = new();

    public AjalDailyPage(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        dpDate.SelectedDate = DateTime.Today;
    }

    private async void DpDate_Changed(object? sender, SelectionChangedEventArgs e) => await LoadDataAsync();
    private async void BtnLoad_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

    // ═══════════════════════════════════════
    // LOAD LOOKUPS (routes + drivers for edit dialogs)
    // ═══════════════════════════════════════
    private async Task EnsureLookupsAsync()
    {
        if (_allRoutes.Count == 0)
        {
            try { _allRoutes = await _api.GetRoutesAsync(); } catch { }
        }
        if (_allDrivers.Count == 0)
        {
            try
            {
                var json = await _api.GetDriversJsonAsync();
                if (json != null)
                {
                    var arr = JArray.Parse(json);
                    _allDrivers = arr.Select(d => new DriverComboItem
                    {
                        driverId = d["driverId"]?.Value<int>() ?? 0,
                        fullName = d["fullName"]?.ToString() ?? ""
                    }).ToList();
                }
            }
            catch { }
        }
    }

    // ═══════════════════════════════════════
    // LOAD SESSION CARDS
    // ═══════════════════════════════════════
    private async Task LoadDataAsync()
    {
        if (dpDate.SelectedDate == null) return;
        pnlSessions.Children.Clear();

        try
        {
            var json = await _api.GetAjalSessionsJsonAsync(dpDate.SelectedDate.Value);
            if (json == null) return;

            var sessions = JArray.Parse(json);
            if (sessions.Count == 0)
            {
                pnlSessions.Children.Add(new TextBlock
                {
                    Text = "لا توجد جلسات لهذا التاريخ",
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    FontSize = 16, Margin = new Thickness(16),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                return;
            }

            foreach (var session in sessions)
            {
                string routeName   = session["routeName"]?.ToString()  ?? "—";
                string driverName  = session["driverName"]?.ToString() ?? "بدون سائق";
                int    entryCount  = (int)(session["entryCount"]   ?? 0);
                int    reviewed    = (int)(session["reviewedCount"] ?? 0);
                decimal total      = (decimal)(session["totalAmount"] ?? 0);
                int    sessionId   = (int)(session["sessionId"]    ?? 0);

                DateTime sessionDate = DateTime.TryParse(session["sessionDate"]?.ToString(), out var sd) ? sd.Date : DateTime.Today;
                pnlSessions.Children.Add(BuildSessionCard(sessionId, routeName, driverName, entryCount, reviewed, total, sessionDate));
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, $"خطأ: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════
    // BUILD SESSION CARD
    // ═══════════════════════════════════════
    private Border BuildSessionCard(int sessionId, string routeName, string driverName,
        int entryCount, int reviewed, decimal total, DateTime sessionDate = default)
    {
        var card = new Border
        {
            Background = (Brush)FindResource("BgCardBrush"),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(20, 16, 20, 16)
        };

        var stack = new StackPanel();

        // ── Header row ──
        var header = new Grid();
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Title: Route + Driver
        var title = new TextBlock { FontSize = 18, FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("AccentPrimaryBrush") };
        title.Inlines.Add(new System.Windows.Documents.Run($"🛣️ {routeName}"));
        title.Inlines.Add(new System.Windows.Documents.Run($"  —  🚐 {driverName}")
            { Foreground = (Brush)FindResource("TextSecondaryBrush"), FontSize = 15 });
        Grid.SetColumn(title, 0);
        header.Children.Add(title);

        // Stats
        var stats = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
        stats.Inlines.Add(new System.Windows.Documents.Run($"📦 {entryCount} فاتورة")
            { Foreground = (Brush)FindResource("TextPrimaryBrush") });
        stats.Inlines.Add(new System.Windows.Documents.Run($"  |  ✅ {reviewed}/{entryCount}")
            { Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)) });
        stats.Inlines.Add(new System.Windows.Documents.Run($"  |  💰 {total:N0}")
            { Foreground = (Brush)FindResource("TextSecondaryBrush") });
        Grid.SetColumn(stats, 1);
        header.Children.Add(stats);
        stack.Children.Add(header);

        // ── Action buttons row ──
        var btnRow = new WrapPanel { Margin = new Thickness(0, 10, 0, 0), Orientation = Orientation.Horizontal };

        // View detail
        var btnDetail = MakeButton("📋 عرض الفواتير", "PrimaryButton");
        btnDetail.Tag = sessionId;
        btnDetail.Click += BtnDetail_Click;
        btnRow.Children.Add(btnDetail);

        // Edit session (route / driver / notes)
        var btnEdit = MakeButton("✏️ تعديل الجلسة", "SecondaryButton");
        btnEdit.Margin = new Thickness(8, 0, 0, 0);
        btnEdit.Tag = sessionId;
        btnEdit.Click += (s, e) => OpenEditSessionDialog(sessionId, routeName, driverName, sessionDate);
        btnRow.Children.Add(btnEdit);

        // Delete session
        var btnDelete = MakeButton("🗑️ حذف الجلسة", "DangerButton");
        btnDelete.Margin = new Thickness(8, 0, 0, 0);
        btnDelete.Tag = sessionId;
        btnDelete.Click += async (s, e) => await DeleteSessionAsync(sessionId, routeName, reviewed > 0);
        btnRow.Children.Add(btnDelete);

        stack.Children.Add(btnRow);

        // ── Detail panel (collapsed by default) ──
        var detailPanel = new StackPanel
        {
            Tag = $"detail_{sessionId}",
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(0, 10, 0, 0)
        };
        stack.Children.Add(detailPanel);

        card.Child = stack;
        return card;
    }

    private Button MakeButton(string content, string style)
        => new Button
        {
            Content = content,
            Style = (Style)FindResource(style),
            MinWidth = 120,
            Padding = new Thickness(14, 6, 14, 6)
        };

    // ═══════════════════════════════════════
    // VIEW / EXPAND SESSION DETAIL
    // ═══════════════════════════════════════
    private async void BtnDetail_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        int sessionId = (int)btn.Tag;

        var parent = btn.Parent as WrapPanel;
        var stack  = parent?.Parent as StackPanel;
        var detailPanel = stack?.Children.OfType<StackPanel>()
            .FirstOrDefault(p => p.Tag?.ToString() == $"detail_{sessionId}");
        if (detailPanel == null) return;

        if (detailPanel.Visibility == Visibility.Visible)
        {
            detailPanel.Visibility = Visibility.Collapsed;
            btn.Content = "📋 عرض الفواتير";
            return;
        }

        btn.Content = "⏳ تحميل...";
        btn.IsEnabled = false;

        try
        {
            var json = await _api.GetAjalSessionDetailJsonAsync(sessionId);
            if (json == null) return;

            var detail  = JObject.Parse(json);
            var entries = detail["entries"] as JArray;
            detailPanel.Children.Clear();

            if (entries == null || entries.Count == 0)
            {
                detailPanel.Children.Add(new TextBlock
                {
                    Text = "لا توجد فواتير",
                    FontSize = 14,
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    Margin = new Thickness(0, 4, 0, 4)
                });
            }
            else
            {
                // Column header
                detailPanel.Children.Add(BuildEntryHeader());

                foreach (var entry in entries)
                {
                    int     entryId     = entry["entryId"]?.Value<int>() ?? 0;
                    string  invoiceNum  = entry["invoiceNumber"]?.ToString() ?? "";
                    int     merchantId  = entry["merchantId"]?.Value<int>() ?? 0;
                    string  merchant    = entry["merchantName"]?.ToString() ?? "";
                    decimal? amount     = entry["amount"]?.Type == JTokenType.Null ? null : entry["amount"]?.Value<decimal>();
                    bool    isReviewed  = entry["isReviewed"]?.Value<bool>() ?? false;
                    string? notes       = entry["notes"]?.ToString();

                    detailPanel.Children.Add(
                        BuildEntryRow(sessionId, entryId, invoiceNum, merchantId, merchant, amount, isReviewed, notes));
                }
            }

            detailPanel.Visibility = Visibility.Visible;
            btn.Content = "🔽 إخفاء الفواتير";
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, $"خطأ: {ex.Message}");
            btn.Content = "📋 عرض الفواتير";
        }
        finally
        {
            btn.IsEnabled = true;
        }
    }

    private Border BuildEntryHeader()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 102, 126, 234)),
            CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 2, 0, 2),
            Padding = new Thickness(10, 5, 10, 5)
        };
        var g = new Grid();
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

        g.Children.Add(MakeHeaderCell(0, "رقم الفاتورة"));
        g.Children.Add(MakeHeaderCell(1, "التاجر"));
        g.Children.Add(MakeHeaderCell(2, "المبلغ"));
        g.Children.Add(MakeHeaderCell(3, ""));
        g.Children.Add(MakeHeaderCell(4, "تعديل"));
        g.Children.Add(MakeHeaderCell(5, "حذف"));
        border.Child = g;
        return border;
    }

    private TextBlock MakeHeaderCell(int col, string text)
    {
        var tb = new TextBlock
        {
            Text = text, FontSize = 12, FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("AccentPrimaryBrush"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(tb, col);
        return tb;
    }

    private Border BuildEntryRow(int sessionId, int entryId, string invoiceNum,
        int merchantId, string merchant, decimal? amount, bool isReviewed, string? notes)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb((byte)(isReviewed ? 15 : 8), 255, 255, 255)),
            CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 2, 0, 2),
            Padding = new Thickness(10, 5, 10, 5),
            Tag = entryId
        };

        var g = new Grid();
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

        // Invoice number
        var tbInvoice = new TextBlock
        {
            Text = invoiceNum, FontSize = 13, FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("TextPrimaryBrush"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(tbInvoice, 0);
        g.Children.Add(tbInvoice);

        // Merchant
        var tbMerchant = new TextBlock
        {
            Text = merchant, FontSize = 13,
            Foreground = (Brush)FindResource("TextPrimaryBrush"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(tbMerchant, 1);
        g.Children.Add(tbMerchant);

        // Amount
        var tbAmount = new TextBlock
        {
            Text = amount?.ToString("N0") ?? "—", FontSize = 13,
            Foreground = (Brush)FindResource("TextSecondaryBrush"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(tbAmount, 2);
        g.Children.Add(tbAmount);

        // Reviewed badge
        var tbStatus = new TextBlock
        {
            Text = isReviewed ? "✅" : "⏳", FontSize = 15,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(tbStatus, 3);
        g.Children.Add(tbStatus);

        // Edit button
        var btnEdit = new Button
        {
            Content = "✏️",
            FontSize = 13,
            Padding = new Thickness(6, 3, 6, 3),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            ToolTip = "تعديل الفاتورة",
            Background = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        btnEdit.Click += (s, e) => OpenEditEntryDialog(entryId, invoiceNum, merchantId, merchant, amount, isReviewed, border);
        Grid.SetColumn(btnEdit, 4);
        g.Children.Add(btnEdit);

        // Delete button
        var btnDelete = new Button
        {
            Content = "🗑️",
            FontSize = 13,
            Padding = new Thickness(6, 3, 6, 3),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            ToolTip = isReviewed ? "لا يمكن حذف فاتورة مُراجَعة" : "حذف الفاتورة",
            IsEnabled = !isReviewed,
            Background = new SolidColorBrush(Color.FromRgb(239, 68, 68)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        btnDelete.Click += async (s, e) => await DeleteEntryAsync(entryId, invoiceNum, border);
        Grid.SetColumn(btnDelete, 5);
        g.Children.Add(btnDelete);

        border.Child = g;
        return border;
    }

    // ═══════════════════════════════════════
    // DELETE SESSION
    // ═══════════════════════════════════════
    private async Task DeleteSessionAsync(int sessionId, string routeName, bool hasReviewed)
    {
        if (hasReviewed)
        {
            MessageBox.Show(
                $"لا يمكن حذف جلسة خط «{routeName}» لأنها تحتوي على فواتير مُراجَعة.\n\nيمكنك حذف الفواتير غير المُراجَعة بشكل فردي.",
                "لا يمكن الحذف", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"هل تريد حذف جلسة خط «{routeName}» وجميع فواتيرها؟\n\nهذا الإجراء لا يمكن التراجع عنه.",
            "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            var ok = await _api.DeleteAjalSessionAsync(sessionId);
            if (ok)
            {
                ToastHelper.ShowSuccess(RootGrid, $"تم حذف جلسة خط «{routeName}» بنجاح ✅");
                await LoadDataAsync(); // Refresh
            }
            else
            {
                MessageBox.Show("لا يمكن حذف الجلسة — تحتوي على فواتير مُراجَعة.",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, $"خطأ: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════
    // EDIT SESSION DIALOG (Route / Driver / Notes)
    // ═══════════════════════════════════════
    private async void OpenEditSessionDialog(int sessionId, string currentRoute, string currentDriver, DateTime currentDate = default)
    {
        await EnsureLookupsAsync();

        // Build the dialog window
        var win = new Window
        {
            Title = "تعديل الجلسة",
            Width = 440, Height = 460,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize,
            FlowDirection = FlowDirection.RightToLeft,
            Background = (Brush)FindResource("BgDeepBrush")
        };

        var panel = new StackPanel { Margin = new Thickness(24) };

        // Title
        panel.Children.Add(new TextBlock
        {
            Text = "✏️ تعديل بيانات الجلسة",
            FontSize = 18, FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("TextPrimaryBrush"),
            Margin = new Thickness(0, 0, 0, 20)
        });

        // Route selector
        panel.Children.Add(MakeLabel("الخط الجديد:"));
        var routeCombo = new ComboBox
        {
            Margin = new Thickness(0, 4, 0, 14),
            ItemsSource = _allRoutes,
            DisplayMemberPath = "RouteName",
            SelectedValuePath = "RouteId",
            SelectedValue = _allRoutes.FirstOrDefault(r => r.RouteName == currentRoute)?.RouteId,
            Height = 34
        };
        panel.Children.Add(routeCombo);

        // Driver selector  
        panel.Children.Add(MakeLabel("السائق الجديد:"));
        var driverItems = new List<DriverComboItem> { new() { driverId = 0, fullName = "— بدون سائق —" } };
        driverItems.AddRange(_allDrivers);
        var driverCombo = new ComboBox
        {
            Margin = new Thickness(0, 4, 0, 14),
            ItemsSource = driverItems,
            DisplayMemberPath = "fullName",
            SelectedValuePath = "driverId",
            SelectedValue = driverItems.FirstOrDefault(d => d.fullName == currentDriver)?.driverId ?? 0,
            Height = 34
        };
        panel.Children.Add(driverCombo);

        // Notes
        panel.Children.Add(MakeLabel("ملاحظات (اختياري):"));
        var notesBox = new TextBox
        {
            Margin = new Thickness(0, 4, 0, 20),
            Height = 34, Padding = new Thickness(8, 6, 8, 6)
        };
        panel.Children.Add(notesBox);

        // Date picker
        panel.Children.Add(MakeLabel("التاريخ الجديد:"));
        var datePicker = new DatePicker
        {
            SelectedDate = currentDate == default ? DateTime.Today : currentDate,
            Margin = new Thickness(0, 4, 0, 20),
            Height = 34,
            FlowDirection = FlowDirection.LeftToRight   // dates always LTR
        };
        panel.Children.Add(datePicker);

        // Buttons row
        var btnRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left };

        var btnSave = new Button
        {
            Content = "💾 حفظ التعديلات",
            Style = (Style)FindResource("PrimaryButton"),
            MinWidth = 150, Margin = new Thickness(0, 0, 10, 0)
        };

        var btnCancel = new Button
        {
            Content = "إلغاء",
            Style = (Style)FindResource("SecondaryButton"),
            MinWidth = 100
        };
        btnCancel.Click += (s, e) => win.Close();

        btnSave.Click += async (s, e) =>
        {
            int? newRouteId  = routeCombo.SelectedValue as int?;
            int? newDriverId = driverCombo.SelectedValue as int?;
            if (newDriverId == 0) newDriverId = null;
            string? newNotes = string.IsNullOrWhiteSpace(notesBox.Text) ? null : notesBox.Text.Trim();

            btnSave.IsEnabled = false;
            btnSave.Content = "⏳ حفظ...";

            try
            {
                var ok = await _api.UpdateAjalSessionAsync(sessionId, new
                {
                    RouteId     = newRouteId,
                    DriverId    = newDriverId,
                    Notes       = newNotes,
                    SessionDate = datePicker.SelectedDate
                });

                if (ok)
                {
                    string successMsg = datePicker.SelectedDate.HasValue && datePicker.SelectedDate.Value.Date != currentDate.Date
                        ? "تم تعديل الجلسة ✅ — الجلسة انتقلت للتاريخ الجديد"
                        : "تم تعديل الجلسة بنجاح ✅";
                    ToastHelper.ShowSuccess(RootGrid, successMsg);
                    win.Close();
                    await LoadDataAsync(); // Refresh cards
                }
                else
                {
                    MessageBox.Show("حدث خطأ أثناء حفظ التعديلات.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnSave.IsEnabled = true;
                btnSave.Content = "💾 حفظ التعديلات";
            }
        };

        btnRow.Children.Add(btnSave);
        btnRow.Children.Add(btnCancel);
        panel.Children.Add(btnRow);

        win.Content = panel;
        win.ShowDialog();
    }

    private TextBlock MakeLabel(string text) => new TextBlock
    {
        Text = text, FontSize = 13,
        Foreground = (Brush)FindResource("TextSecondaryBrush"),
        Margin = new Thickness(0, 0, 0, 2)
    };

    // ═══════════════════════════════════════
    // DELETE ENTRY
    // ═══════════════════════════════════════
    private async Task DeleteEntryAsync(int entryId, string invoiceNum, Border entryRow)
    {
        var confirm = MessageBox.Show(
            $"هل تريد حذف الفاتورة رقم «{invoiceNum}»؟",
            "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            var ok = await _api.DeleteAjalEntryAsync(entryId);
            if (ok)
            {
                // Remove the row from the UI immediately
                if (entryRow.Parent is StackPanel parent)
                    parent.Children.Remove(entryRow);
                ToastHelper.ShowSuccess(RootGrid, $"تم حذف الفاتورة «{invoiceNum}» ✅");
            }
            else
            {
                MessageBox.Show("لا يمكن حذف فاتورة مُراجَعة.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, $"خطأ: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════
    // EDIT ENTRY DIALOG
    // ═══════════════════════════════════════
    private void OpenEditEntryDialog(int entryId, string invoiceNum, int merchantId, string merchant,
        decimal? amount, bool isReviewed, Border entryRow)
    {
        var win = new Window
        {
            Title = "تعديل الفاتورة",
            Width = 420, Height = 320,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize,
            FlowDirection = FlowDirection.RightToLeft,
            Background = (Brush)FindResource("BgDeepBrush")
        };

        var panel = new StackPanel { Margin = new Thickness(24) };

        panel.Children.Add(new TextBlock
        {
            Text = $"✏️ تعديل فاتورة رقم {invoiceNum}",
            FontSize = 17, FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("TextPrimaryBrush"),
            Margin = new Thickness(0, 0, 0, 16)
        });

        if (isReviewed)
        {
            panel.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(40, 234, 179, 8)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 7, 10, 7),
                Margin = new Thickness(0, 0, 0, 12),
                Child = new TextBlock
                {
                    Text = "⚠️ هذه الفاتورة مُراجَعة — ستُعدَّل بشكل استثنائي",
                    FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(234, 179, 8)),
                    TextWrapping = TextWrapping.Wrap
                }
            });
        }

        // Amount
        panel.Children.Add(MakeLabel("المبلغ:"));
        var amountBox = new TextBox
        {
            Text = amount?.ToString() ?? "",
            Margin = new Thickness(0, 4, 0, 12),
            Height = 34, Padding = new Thickness(8, 6, 8, 6)
        };
        panel.Children.Add(amountBox);

        // Notes
        panel.Children.Add(MakeLabel("ملاحظات:"));
        var notesBox = new TextBox
        {
            Margin = new Thickness(0, 4, 0, 18),
            Height = 34, Padding = new Thickness(8, 6, 8, 6)
        };
        panel.Children.Add(notesBox);

        var btnRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left };

        var btnSave = new Button
        {
            Content = "💾 حفظ",
            Style = (Style)FindResource("PrimaryButton"),
            MinWidth = 120, Margin = new Thickness(0, 0, 10, 0)
        };

        var btnCancel = new Button
        {
            Content = "إلغاء",
            Style = (Style)FindResource("SecondaryButton"),
            MinWidth = 90
        };
        btnCancel.Click += (s, e) => win.Close();

        btnSave.Click += async (s, e) =>
        {
            decimal? newAmount = null;
            if (!string.IsNullOrWhiteSpace(amountBox.Text))
            {
                if (!decimal.TryParse(amountBox.Text, out decimal parsed))
                {
                    MessageBox.Show("المبلغ المدخل غير صحيح.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                newAmount = parsed;
            }

            btnSave.IsEnabled = false;
            btnSave.Content = "⏳...";

            try
            {
                var ok = await _api.UpdateAjalEntryAsync(entryId, new
                {
                    Amount = newAmount,
                    Notes  = string.IsNullOrWhiteSpace(notesBox.Text) ? (string?)null : notesBox.Text.Trim()
                });

                if (ok)
                {
                    // Update the amount cell in the existing row immediately
                    if (entryRow.Child is Grid g)
                    {
                        foreach (UIElement child in g.Children)
                        {
                            if (child is TextBlock tb && Grid.GetColumn(tb) == 2)
                            {
                                tb.Text = newAmount?.ToString("N0") ?? "—";
                                break;
                            }
                        }
                    }
                    ToastHelper.ShowSuccess(RootGrid, $"تم تعديل الفاتورة «{invoiceNum}» ✅");
                    win.Close();
                }
                else
                {
                    MessageBox.Show("حدث خطأ أثناء الحفظ.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnSave.IsEnabled = true;
                btnSave.Content = "💾 حفظ";
            }
        };

        btnRow.Children.Add(btnSave);
        btnRow.Children.Add(btnCancel);
        panel.Children.Add(btnRow);

        win.Content = panel;
        win.ShowDialog();
    }

    // ═══════════════════════════════════════
    // EXPORT
    // ═══════════════════════════════════════
    private async void BtnExport_Click(object sender, RoutedEventArgs e)
    {
        if (dpDate.SelectedDate == null) return;
        var dlg = new SaveFileDialog
        {
            Filter = "Excel|*.xlsx",
            FileName = $"ajal_daily_{dpDate.SelectedDate:yyyyMMdd}.xlsx"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var bytes = await _api.ExportAjalDailyAsync(dpDate.SelectedDate.Value);
            if (bytes != null)
            {
                await System.IO.File.WriteAllBytesAsync(dlg.FileName, bytes);
                ToastHelper.ShowSuccess(RootGrid, "تم التصدير بنجاح ✅");
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, $"خطأ: {ex.Message}"); }
    }

    // Helper cell builder (kept for internal use)
    private TextBlock MakeCell(int col, string text, FontWeight weight)
    {
        var tb = new TextBlock
        {
            Text = text, FontSize = 14, FontWeight = weight,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = (Brush)FindResource("TextPrimaryBrush")
        };
        Grid.SetColumn(tb, col);
        return tb;
    }
}
