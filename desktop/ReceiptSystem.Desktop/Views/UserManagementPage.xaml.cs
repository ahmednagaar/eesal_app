using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class UserManagementPage : UserControl
{
    private readonly ApiClient _api;

    public UserManagementPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        Loaded += async (_, _) => await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            var json = await _api.GetUsersJsonAsync();
            if (json == null) return;

            var users = JArray.Parse(json);
            UsersGrid.ItemsSource = users.Select(u => new
            {
                userId = u["userId"]?.Value<int>() ?? 0,
                username = u["username"]?.ToString() ?? "",
                fullName = u["fullName"]?.ToString() ?? "",
                role = u["role"]?.ToString() ?? "",
                roleAr = u["role"]?.ToString() switch
                {
                    "Admin" => "مدير",
                    "Treasury" => "خزينة",
                    "CallCenter" => "مركز اتصال",
                    _ => u["role"]?.ToString() ?? ""
                },
                isActive = u["isActive"]?.Value<bool>() ?? false,
                statusText = u["isActive"]?.Value<bool>() == true ? "🟢 نشط" : "🔴 معطّل",
                lastLogin = u["lastLoginAt"]?.Value<DateTime?>()?.ToString("dd/MM/yyyy HH:mm") ?? "لم يسجل دخول"
            }).ToList();
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ErrorMessageHelper.GetArabicMessage(ex)); }
        finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
    }

    private void AddUser_Click(object sender, RoutedEventArgs e) => ShowUserDialog(null);

    private void UsersGrid_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (UsersGrid.SelectedItem != null)
            ShowUserDialog(UsersGrid.SelectedItem);
    }

    private void ShowUserDialog(object? existingUser)
    {
        bool isEdit = existingUser != null;
        dynamic? user = existingUser;

        var dlg = new Window
        {
            Title = isEdit ? "تعديل مستخدم" : "مستخدم جديد",
            Width = 420, Height = isEdit ? 460 : 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this), FlowDirection = FlowDirection.RightToLeft,
            Background = (Brush)FindResource("BgDeepBrush"),
            Foreground = (Brush)FindResource("TextPrimaryBrush")
        };

        var sp = new StackPanel { Margin = new Thickness(20) };

        // Username
        sp.Children.Add(new TextBlock { Text = "اسم المستخدم:", FontSize = 13, Margin = new Thickness(0, 0, 0, 4) });
        var usernameBox = new TextBox { FontSize = 14, Padding = new Thickness(8, 6, 8, 6), IsEnabled = !isEdit };
        if (isEdit) usernameBox.Text = user.username;
        sp.Children.Add(usernameBox);

        // Full Name
        sp.Children.Add(new TextBlock { Text = "الاسم الكامل:", FontSize = 13, Margin = new Thickness(0, 12, 0, 4) });
        var fullNameBox = new TextBox { FontSize = 14, Padding = new Thickness(8, 6, 8, 6) };
        if (isEdit) fullNameBox.Text = user.fullName;
        sp.Children.Add(fullNameBox);

        // Role
        sp.Children.Add(new TextBlock { Text = "الدور:", FontSize = 13, Margin = new Thickness(0, 12, 0, 4) });
        var roleCombo = new ComboBox { FontSize = 14, Padding = new Thickness(6, 4, 6, 4) };
        roleCombo.Items.Add(new ComboBoxItem { Content = "مدير", Tag = "Admin" });
        roleCombo.Items.Add(new ComboBoxItem { Content = "خزينة", Tag = "Treasury" });
        roleCombo.Items.Add(new ComboBoxItem { Content = "مركز اتصال", Tag = "CallCenter" });
        if (isEdit)
        {
            for (int i = 0; i < roleCombo.Items.Count; i++)
            {
                if (((ComboBoxItem)roleCombo.Items[i]).Tag?.ToString() == user.role)
                {
                    roleCombo.SelectedIndex = i; break;
                }
            }
        }
        else roleCombo.SelectedIndex = 1;
        sp.Children.Add(roleCombo);

        // Password (only for new)
        TextBox? passwordBox = null;
        if (!isEdit)
        {
            sp.Children.Add(new TextBlock { Text = "كلمة المرور:", FontSize = 13, Margin = new Thickness(0, 12, 0, 4) });
            passwordBox = new TextBox { FontSize = 14, Padding = new Thickness(8, 6, 8, 6) };
            sp.Children.Add(passwordBox);
            sp.Children.Add(new TextBlock
            {
                Text = "8+ أحرف، رقم واحد، حرف كبير واحد",
                FontSize = 10, Foreground = (Brush)FindResource("TextMutedBrush"), Margin = new Thickness(0, 2, 0, 0)
            });
        }

        // Active toggle (only for edit)
        CheckBox? activeCheck = null;
        if (isEdit)
        {
            activeCheck = new CheckBox
            {
                Content = "نشط", FontSize = 14, Margin = new Thickness(0, 16, 0, 0),
                IsChecked = user.isActive
            };
            sp.Children.Add(activeCheck);
        }

        // Buttons row
        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 16, 0, 0) };
        var saveBtn = new Button
        {
            Content = isEdit ? "💾 حفظ التعديل" : "➕ إنشاء",
            Style = (Style)FindResource("PrimaryButton"),
            Padding = new Thickness(16, 8, 16, 8), FontSize = 14
        };
        btnPanel.Children.Add(saveBtn);

        if (isEdit)
        {
            var resetPwdBtn = new Button
            {
                Content = "🔑 إعادة كلمة المرور",
                Style = (Style)FindResource("SecondaryButton"),
                Padding = new Thickness(12, 6, 12, 6), FontSize = 12, Margin = new Thickness(8, 0, 0, 0)
            };
            resetPwdBtn.Click += async (s, ev) =>
            {
                var pwdDlg = new Window
                {
                    Title = "إعادة كلمة المرور", Width = 350, Height = 180,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = dlg, FlowDirection = FlowDirection.RightToLeft,
                    Background = (Brush)FindResource("BgDeepBrush"),
                    Foreground = (Brush)FindResource("TextPrimaryBrush")
                };
                var pwdSp = new StackPanel { Margin = new Thickness(20) };
                pwdSp.Children.Add(new TextBlock { Text = "كلمة المرور الجديدة:", FontSize = 13, Margin = new Thickness(0, 0, 0, 4) });
                var newPwdBox = new TextBox { FontSize = 14, Padding = new Thickness(8, 6, 8, 6) };
                pwdSp.Children.Add(newPwdBox);
                var resetBtn = new Button
                {
                    Content = "🔑 تعيين", Style = (Style)FindResource("PrimaryButton"),
                    Padding = new Thickness(16, 8, 16, 8), FontSize = 14,
                    Margin = new Thickness(0, 12, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch
                };
                pwdSp.Children.Add(resetBtn);
                pwdDlg.Content = pwdSp;

                resetBtn.Click += async (s2, ev2) =>
                {
                    if (string.IsNullOrWhiteSpace(newPwdBox.Text)) return;
                    try
                    {
                        bool ok = await _api.ResetUserPasswordAsync(user.userId, newPwdBox.Text);
                        if (ok)
                        {
                            pwdDlg.Close();
                            ToastHelper.ShowSuccess(RootGrid, "✓ تم إعادة تعيين كلمة المرور");
                        }
                    }
                    catch (Exception ex2) { MessageBox.Show(ex2.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
                };
                pwdDlg.ShowDialog();
            };
            btnPanel.Children.Add(resetPwdBtn);

            var deactivateBtn = new Button
            {
                Content = "🚫 تعطيل",
                Padding = new Thickness(10, 6, 10, 6), FontSize = 12, Margin = new Thickness(8, 0, 0, 0),
                Background = (Brush)FindResource("AccentDangerBrush"), Foreground = Brushes.White
            };
            deactivateBtn.Click += async (s, ev) =>
            {
                if (MessageBox.Show("هل تريد تعطيل هذا المستخدم؟", "تأكيد", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
                try
                {
                    bool ok = await _api.DeactivateUserAsync(user.userId);
                    if (ok)
                    {
                        dlg.Close();
                        ToastHelper.ShowSuccess(RootGrid, "✓ تم تعطيل المستخدم");
                        await LoadUsersAsync();
                    }
                }
                catch (Exception ex3) { MessageBox.Show(ex3.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            };
            btnPanel.Children.Add(deactivateBtn);
        }

        sp.Children.Add(btnPanel);
        dlg.Content = sp;

        saveBtn.Click += async (s, ev) =>
        {
            string role = (roleCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Treasury";
            try
            {
                if (isEdit)
                {
                    bool ok = await _api.UpdateUserAsync(user.userId, fullNameBox.Text, role, activeCheck?.IsChecked ?? true);
                    if (ok) { dlg.Close(); ToastHelper.ShowSuccess(RootGrid, "✓ تم تحديث المستخدم"); await LoadUsersAsync(); }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(usernameBox.Text) || string.IsNullOrWhiteSpace(passwordBox?.Text))
                    {
                        MessageBox.Show("جميع الحقول مطلوبة", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    var result = await _api.CreateUserAsync(usernameBox.Text, fullNameBox.Text, role, passwordBox!.Text);
                    if (result != null) { dlg.Close(); ToastHelper.ShowSuccess(RootGrid, "✓ تم إنشاء المستخدم"); await LoadUsersAsync(); }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
        };

        dlg.ShowDialog();
    }
}
