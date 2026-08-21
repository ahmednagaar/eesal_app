using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class MerchantsPage : UserControl
{
    private readonly ApiClient _api;
    private JArray _merchants = new();
    private JObject? _editingMerchant;
    private int _currentPage = 1;
    private int _totalMerchants;
    private const int PageSize = 50;
    private string _searchQuery = "";

    public MerchantsPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        LoadMerchants();
    }

    private async void LoadMerchants()
    {
        string? json;

        if (!string.IsNullOrWhiteSpace(_searchQuery))
        {
            json = await _api.SearchMerchantsJsonAsync(_searchQuery);
            if (json != null)
            {
                _merchants = JArray.Parse(json);
                _totalMerchants = _merchants.Count;
            }
        }
        else
        {
            json = await _api.GetMerchantsJsonAsync(_currentPage, PageSize);
            if (json != null)
            {
                var result = JObject.Parse(json);
                _merchants = result["data"] as JArray ?? new JArray();
                _totalMerchants = result["total"]?.Value<int>() ?? 0;
            }
        }

        MerchantsGrid.ItemsSource = _merchants;
        UpdatePagination();
    }

    private void UpdatePagination()
    {
        var totalPages = (_totalMerchants + PageSize - 1) / PageSize;
        PageInfoText.Text = $"صفحة {_currentPage} من {Math.Max(1, totalPages)}  —  إجمالي: {_totalMerchants} تاجر";
        PrevBtn.IsEnabled = _currentPage > 1;
        NextBtn.IsEnabled = _currentPage < totalPages;
    }

    // ── Search ──
    private System.Timers.Timer? _debounceTimer;
    private void Search_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = SearchBox.Text.Trim();
        SearchPlaceholder.Visibility = string.IsNullOrEmpty(query)
            ? Visibility.Visible : Visibility.Collapsed;

        // Debounce: wait 400ms after user stops typing
        _debounceTimer?.Stop();
        _debounceTimer = new System.Timers.Timer(400);
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += (_, _) =>
        {
            Dispatcher.Invoke(() =>
            {
                _searchQuery = SearchBox.Text.Trim();
                _currentPage = 1;
                LoadMerchants();
            });
        };
        _debounceTimer.Start();
    }

    // ── Pagination ──
    private void PrevPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            LoadMerchants();
        }
    }

    private void NextPage_Click(object sender, RoutedEventArgs e)
    {
        var totalPages = (_totalMerchants + PageSize - 1) / PageSize;
        if (_currentPage < totalPages)
        {
            _currentPage++;
            LoadMerchants();
        }
    }

    // ── CRUD ──
    private void AddMerchant_Click(object sender, RoutedEventArgs e)
    {
        _editingMerchant = null;
        DialogTitle.Text = "إضافة تاجر جديد";
        MerchantNameBox.Text = "";
        CityBox.Text = "";
        MerchantPhoneBox.Text = "";
        MerchantNotesBox.Text = "";
        DialogError.Visibility = Visibility.Collapsed;
        DialogOverlay.Visibility = Visibility.Visible;
        MerchantNameBox.Focus();
    }

    private void EditMerchant_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is JObject merchant)
        {
            _editingMerchant = merchant;
            DialogTitle.Text = "تعديل تاجر";
            MerchantNameBox.Text = merchant["merchantName"]?.ToString() ?? "";
            CityBox.Text = merchant["city"]?.ToString() ?? "";
            MerchantPhoneBox.Text = merchant["phoneNumber"]?.ToString() ?? "";
            MerchantNotesBox.Text = merchant["notes"]?.ToString() ?? "";
            DialogError.Visibility = Visibility.Collapsed;
            DialogOverlay.Visibility = Visibility.Visible;
            MerchantNameBox.Focus();
        }
    }

    private async void SaveMerchant_Click(object sender, RoutedEventArgs e)
    {
        var name = MerchantNameBox.Text.Trim();
        var city = CityBox.Text.Trim();
        var phone = MerchantPhoneBox.Text.Trim();
        var notes = MerchantNotesBox.Text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            DialogError.Text = "يرجى إدخال اسم التاجر";
            DialogError.Visibility = Visibility.Visible;
            return;
        }

        if (_editingMerchant != null)
        {
            var id = (int)_editingMerchant["merchantId"]!;
            var success = await _api.UpdateMerchantAsync(id, name, city, phone, notes);
            if (!success)
            {
                DialogError.Text = "حدث خطأ أثناء التعديل";
                DialogError.Visibility = Visibility.Visible;
                return;
            }
        }
        else
        {
            var result = await _api.CreateMerchantAsync(name, city, phone, notes);
            if (result == null)
            {
                DialogError.Text = "حدث خطأ أثناء الإضافة";
                DialogError.Visibility = Visibility.Visible;
                return;
            }
        }

        DialogOverlay.Visibility = Visibility.Collapsed;
        LoadMerchants();
    }

    private void CancelDialog_Click(object sender, RoutedEventArgs e)
    {
        DialogOverlay.Visibility = Visibility.Collapsed;
    }
}
