using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class ReturnBooksPage : UserControl
{
    private readonly ApiClient _api;
    private int _selectedBookId;
    private bool _isLoaded = false;

    public ReturnBooksPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_isLoaded)
            {
                await LoadSeriesAsync();
                _isLoaded = true;
                await LoadBooksAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadSeriesAsync()
    {
        try
        {
            var json = await _api.GetReturnBookSeriesJsonAsync();
            if (json != null)
            {
                var series = JArray.Parse(json);
                var list = series.Select(s => new SeriesComboItem
                {
                    seriesId = s["seriesId"]?.Value<int>() ?? 0,
                    seriesCode = s["seriesCode"]?.ToString() ?? ""
                }).ToList();
                SeriesCombo.ItemsSource = list;
                if (list.Count > 0)
                    SeriesCombo.SelectedIndex = 0;
            }
        }
        catch (ApiUnauthorizedException) { MessageBox.Show("انتهت صلاحية الجلسة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async Task LoadBooksAsync()
    {
        if (SeriesCombo.SelectedValue == null) return;
        try
        {
            int seriesId;
            if (SeriesCombo.SelectedValue is int sid)
                seriesId = sid;
            else if (int.TryParse(SeriesCombo.SelectedValue?.ToString(), out int parsed))
                seriesId = parsed;
            else
                return;
            string? statusFilter = null;
            if (StatusFilter.SelectedItem is ComboBoxItem item && item.Tag is string tag && !string.IsNullOrEmpty(tag))
                statusFilter = tag;

            var json = await _api.GetReturnBooksJsonAsync(seriesId, statusFilter);
            if (json != null)
            {
                var books = JsonConvert.DeserializeObject<JArray>(json);
                if (books != null)
                {
                    foreach (var book in books)
                    {
                        book["isVerifiedText"] = book["isVerified"]?.Value<bool>() == true ? "✅" : "—";
                    }
                }
                BooksGrid.ItemsSource = books;
            }
        }
        catch (ApiUnauthorizedException) { MessageBox.Show("انتهت صلاحية الجلسة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void SeriesCombo_Changed(object sender, SelectionChangedEventArgs e) { if (_isLoaded) await LoadBooksAsync(); }
    private async void StatusFilter_Changed(object sender, SelectionChangedEventArgs e) { if (_isLoaded) await LoadBooksAsync(); }
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadBooksAsync();

    // ═══ CREATE SERIES ═══
    private void AddSeries_Click(object sender, RoutedEventArgs e)
    {
        SeriesCodeBox.Text = "";
        TotalBooksBox.Text = "200";
        ReceiptsPerBookBox.Text = "50";
        SeriesDialogError.Visibility = Visibility.Collapsed;
        SeriesDialog.Visibility = Visibility.Visible;
    }

    private async void CreateSeries_Click(object sender, RoutedEventArgs e)
    {
        var code = SeriesCodeBox.Text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            SeriesDialogError.Text = "أدخل كود السلسلة";
            SeriesDialogError.Visibility = Visibility.Visible;
            return;
        }
        if (!int.TryParse(TotalBooksBox.Text.Trim(), out int totalBooks) || totalBooks <= 0)
        {
            SeriesDialogError.Text = "عدد الدفاتر غير صحيح";
            SeriesDialogError.Visibility = Visibility.Visible;
            return;
        }
        if (!int.TryParse(ReceiptsPerBookBox.Text.Trim(), out int receiptsPerBook) || receiptsPerBook <= 0)
        {
            SeriesDialogError.Text = "عدد الإيصالات غير صحيح";
            SeriesDialogError.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            var result = await _api.CreateReturnBookSeriesAsync(code, totalBooks, receiptsPerBook);
            if (result != null)
            {
                SeriesDialog.Visibility = Visibility.Collapsed;
                MessageBox.Show($"تم إنشاء السلسلة بنجاح — {result.BooksGenerated} دفتر", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadSeriesAsync();
            }
        }
        catch (Exception ex)
        {
            SeriesDialogError.Text = ex.Message;
            SeriesDialogError.Visibility = Visibility.Visible;
        }
    }

    private void CancelSeriesDialog_Click(object sender, RoutedEventArgs e) => SeriesDialog.Visibility = Visibility.Collapsed;

    // ═══ ASSIGN BOOK ═══
    private async void AssignBook_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not JObject book) return;
        var status = book["status"]?.ToString();
        if (status != "Available" && status != "Returned")
        {
            MessageBox.Show("لا يمكن تسليم هذا الدفتر في حالته الحالية", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _selectedBookId = book["bookId"]!.Value<int>();
        AssignDialogTitle.Text = $"تسليم دفتر رقم {book["bookNumber"]}";
        AssignNotesBox.Text = "";
        AssignDialogError.Visibility = Visibility.Collapsed;

        // Load drivers
        try
        {
            var driversJson = await _api.GetDriversJsonAsync();
            if (driversJson != null)
            {
                var drivers = JArray.Parse(driversJson);
                AssignDriverCombo.ItemsSource = drivers.Select(d => new DriverComboItem
                {
                    driverId = d["driverId"]?.Value<int>() ?? 0,
                    fullName = d["fullName"]?.ToString() ?? ""
                }).ToList();
            }
        }
        catch { }

        AssignDialog.Visibility = Visibility.Visible;
    }

    private async void ConfirmAssign_Click(object sender, RoutedEventArgs e)
    {
        if (AssignDriverCombo.SelectedValue == null)
        {
            AssignDialogError.Text = "اختر السائق";
            AssignDialogError.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            int driverId = (int)AssignDriverCombo.SelectedValue;
            var result = await _api.AssignReturnBookAsync(_selectedBookId, driverId, AssignNotesBox.Text.Trim());
            if (result != null)
            {
                AssignDialog.Visibility = Visibility.Collapsed;
                await LoadBooksAsync();
            }
        }
        catch (Exception ex)
        {
            AssignDialogError.Text = ex.Message;
            AssignDialogError.Visibility = Visibility.Visible;
        }
    }

    private void CancelAssignDialog_Click(object sender, RoutedEventArgs e) => AssignDialog.Visibility = Visibility.Collapsed;

    // ═══ RETURN BOOK ═══
    private async void ReturnBook_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not JObject book) return;
        var status = book["status"]?.ToString();
        if (status != "Assigned" && status != "InProgress")
        {
            MessageBox.Show("لا يمكن استرجاع هذا الدفتر في حالته الحالية", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int bookId = book["bookId"]!.Value<int>();
        var confirm = MessageBox.Show($"هل تريد استرجاع دفتر رقم {book["bookNumber"]}؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            var result = await _api.ReturnReturnBookAsync(bookId);
            if (result != null) await LoadBooksAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    // ═══ VERIFY BOOK ═══
    private async void VerifyBook_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not JObject book) return;

        int bookId = book["bookId"]!.Value<int>();
        var confirm = MessageBox.Show($"هل تريد مراجعة دفتر رقم {book["bookNumber"]}؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            var result = await _api.VerifyReturnBookAsync(bookId);
            if (result != null) await LoadBooksAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }
}
