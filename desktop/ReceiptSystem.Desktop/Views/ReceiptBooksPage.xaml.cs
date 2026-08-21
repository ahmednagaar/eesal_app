using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class ReceiptBooksPage : UserControl
{
    private readonly ApiClient _api;
    private int _selectedBookId;

    public ReceiptBooksPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        AssignDate.SelectedDate = DateTime.Today;
        Loaded += async (_, _) =>
        {
            await LoadDriversAsync();
            await LoadBooksAsync();
            await LoadLowStockAsync();
        };
    }

    private async Task LoadDriversAsync()
    {
        try
        {
            var json = await _api.GetDriversJsonAsync();
            if (json == null) return;
            var drivers = JArray.Parse(json);
            AssignDriverCombo.ItemsSource = drivers.Select(d => new
            {
                driverId = d["driverId"]?.Value<int>() ?? 0,
                fullName = d["fullName"]?.ToString() ?? ""
            }).ToList();
        }
        catch { }
    }

    private async Task LoadBooksAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            var json = await _api.GetBooksJsonAsync();
            if (json == null) return;
            var books = JArray.Parse(json);
            var rows = books.Select(b =>
            {
                var st = b["status"]?.ToString() ?? "Available";
                return new
                {
                    bookId = b["bookId"]?.Value<int>() ?? 0,
                    bookNumber = b["bookNumber"]?.Value<int>() ?? 0,
                    startReceipt = b["startReceiptNumber"]?.Value<int>() ?? 0,
                    endReceipt = b["endReceiptNumber"]?.Value<int>() ?? 0,
                    statusDisplay = st switch
                    {
                        "Available" => "متاح",
                        "Assigned" => "مُعيّن",
                        "Depleted" => "مُنتهي",
                        "Deactivated" => "مُلغى",
                        _ => st
                    },
                    rawStatus = st,
                    driverName = b["driverName"]?.ToString() ?? "—",
                    remaining = (b["remaining"]?.Value<int?>())?.ToString() ?? "—",
                    notes = b["notes"]?.ToString() ?? ""
                };
            }).ToList();
            BooksGrid.ItemsSource = rows;
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

    private async Task LoadLowStockAsync()
    {
        try
        {
            var json = await _api.GetLowStockBooksJsonAsync();
            if (json == null) return;
            var books = JArray.Parse(json);
            if (books.Count > 0)
            {
                var items = books.Select(b =>
                    $"دفتر {b["bookNumber"]} — متبقي {b["remaining"]} إيصال" +
                    (b["driverName"] != null ? $" ({b["driverName"]})" : "")).ToList();
                LowStockText.Text = string.Join("  •  ", items);
                LowStockSection.Visibility = Visibility.Visible;
            }
            else
            {
                LowStockSection.Visibility = Visibility.Collapsed;
            }
        }
        catch
        {
            LowStockSection.Visibility = Visibility.Collapsed;
        }
    }

    // ── Tab Toggle ──
    private void Tab_Changed(object sender, RoutedEventArgs e)
    {
        if (TabCreate?.IsChecked == true)
        {
            CreatePanel.Visibility = Visibility.Visible;
            AssignPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            CreatePanel.Visibility = Visibility.Collapsed;
            AssignPanel.Visibility = Visibility.Visible;
        }
    }

    // ── Selection ──
    private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BooksGrid.SelectedItem == null) { AssignBtn.IsEnabled = false; EditPanel.Visibility = Visibility.Collapsed; return; }
        dynamic row = BooksGrid.SelectedItem;
        _selectedBookId = (int)row.bookId;
        AssignBookLabel.Text = $"دفتر {row.bookNumber} ({row.startReceipt}–{row.endReceipt})";
        AssignBtn.IsEnabled = ((string)row.rawStatus) == "Available";
        EditNotes.Text = (string)row.notes;
        EditPanel.Visibility = ((string)row.rawStatus) != "Deactivated" ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── Create ──
    private async void Create_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(NewBookNumber.Text, out int bookNum) || bookNum <= 0)
        { ToastHelper.ShowError(RootGrid, "رقم الدفتر غير صحيح"); return; }
        if (!int.TryParse(NewStartReceipt.Text, out int start) || start <= 0)
        { ToastHelper.ShowError(RootGrid, "رقم بداية الإيصال غير صحيح"); return; }
        if (!int.TryParse(NewEndReceipt.Text, out int end) || end <= start)
        { ToastHelper.ShowError(RootGrid, "رقم نهاية الإيصال يجب أن يكون أكبر من البداية"); return; }

        CreateBtn.IsEnabled = false;
        try
        {
            var result = await _api.CreateBookAsync(bookNum, start, end, NewBookNotes.Text.Trim());
            if (result != null)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم إنشاء الدفتر");
                NewBookNumber.Text = ""; NewStartReceipt.Text = ""; NewEndReceipt.Text = ""; NewBookNotes.Text = "";
                await LoadBooksAsync();
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { CreateBtn.IsEnabled = true; }
    }

    // ── Assign ──
    private async void Assign_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBookId <= 0) { ToastHelper.ShowError(RootGrid, "اختر دفتر أولاً"); return; }
        if (AssignDriverCombo.SelectedValue is not int driverId || driverId <= 0)
        { ToastHelper.ShowError(RootGrid, "اختر السائق"); return; }

        AssignBtn.IsEnabled = false;
        try
        {
            bool ok = await _api.AssignBookAsync(_selectedBookId, driverId, AssignDate.SelectedDate);
            if (ok)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم تعيين الدفتر للسائق");
                await LoadBooksAsync();
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { AssignBtn.IsEnabled = true; }
    }

    // ── Update ──
    private async void UpdateBook_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBookId <= 0) return;
        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            bool ok = await _api.UpdateBookAsync(_selectedBookId, notes: EditNotes.Text.Trim());
            if (ok) { ToastHelper.ShowSuccess(RootGrid, "✓ تم تحديث الدفتر"); await LoadBooksAsync(); }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }

    // ── Deactivate ──
    private async void Deactivate_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBookId <= 0) return;
        var result = MessageBox.Show("هل أنت متأكد من إلغاء تفعيل هذا الدفتر؟", "تأكيد الإلغاء",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            bool ok = await _api.DeactivateBookAsync(_selectedBookId);
            if (ok) { ToastHelper.ShowSuccess(RootGrid, "✓ تم إلغاء تفعيل الدفتر"); await LoadBooksAsync(); }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadBooksAsync();
        await LoadLowStockAsync();
    }
}
