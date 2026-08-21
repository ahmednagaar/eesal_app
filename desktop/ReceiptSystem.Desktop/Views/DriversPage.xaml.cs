using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class DriversPage : UserControl
{
    private readonly ApiClient _api;
    private JArray _drivers = new();
    private JObject? _editingDriver;

    public DriversPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        LoadDrivers();
    }

    private async void LoadDrivers()
    {
        var json = await _api.GetDriversJsonAsync();
        if (json != null)
        {
            _drivers = JArray.Parse(json);
            DriversGrid.ItemsSource = _drivers;
        }
    }

    private void AddDriver_Click(object sender, RoutedEventArgs e)
    {
        _editingDriver = null;
        DialogTitle.Text = "إضافة سائق جديد";
        NameBox.Text = "";
        PhoneBox.Text = "";
        NotesBox.Text = "";
        DialogError.Visibility = Visibility.Collapsed;
        DialogOverlay.Visibility = Visibility.Visible;
        NameBox.Focus();
    }

    private void EditDriver_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is JObject driver)
        {
            _editingDriver = driver;
            DialogTitle.Text = "تعديل سائق";
            NameBox.Text = driver["fullName"]?.ToString() ?? "";
            PhoneBox.Text = driver["phoneNumber"]?.ToString() ?? "";
            NotesBox.Text = driver["notes"]?.ToString() ?? "";
            DialogError.Visibility = Visibility.Collapsed;
            DialogOverlay.Visibility = Visibility.Visible;
            NameBox.Focus();
        }
    }

    private async void SaveDriver_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        var phone = PhoneBox.Text.Trim();
        var notes = NotesBox.Text.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
        {
            DialogError.Text = "يرجى إدخال الاسم ورقم الهاتف";
            DialogError.Visibility = Visibility.Visible;
            return;
        }

        if (_editingDriver != null)
        {
            var id = (int)_editingDriver["driverId"]!;
            var success = await _api.UpdateDriverAsync(id, name, phone, notes);
            if (!success)
            {
                DialogError.Text = "حدث خطأ أثناء التعديل";
                DialogError.Visibility = Visibility.Visible;
                return;
            }
        }
        else
        {
            var result = await _api.CreateDriverAsync(name, phone, notes);
            if (result == null)
            {
                DialogError.Text = "حدث خطأ أثناء الإضافة";
                DialogError.Visibility = Visibility.Visible;
                return;
            }
        }

        DialogOverlay.Visibility = Visibility.Collapsed;
        LoadDrivers();
    }

    private async void DeleteDriver_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is JObject driver)
        {
            var name = driver["fullName"]?.ToString() ?? "";
            var result = MessageBox.Show(
                $"هل تريد حذف السائق {name}؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var id = (int)driver["driverId"]!;
                await _api.DeleteDriverAsync(id);
                LoadDrivers();
            }
        }
    }

    private void CancelDialog_Click(object sender, RoutedEventArgs e)
    {
        DialogOverlay.Visibility = Visibility.Collapsed;
    }
}
