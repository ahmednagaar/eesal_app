using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly ApiClient _api;
    private bool _isLoaded = false;

    // Cached Module 2 pages
    private DailyInvoicesPage? _dailyInvoicesPage;
    private DriversPage? _driversPage;
    private MerchantsPage? _merchantsPage;
    private RouteSetupPage? _routeSetupPage;
    private RouteHistoryPage? _routeHistoryPage;

    // Cached Module 1 pages
    private ReceiptDashboardPage? _receiptDashboardPage;
    private NewSessionPage? _newSessionPage;
    private SessionsPage? _sessionsPage;
    private GapReviewPage? _gapReviewPage;
    private ReceiptBooksPage? _receiptBooksPage;
    private ErpImportPage? _erpImportPage;
    private DailyReportPage? _dailyReportPage;
    private ReceiptSearchPage? _receiptSearchPage;
    private DriverPerformancePage? _driverPerformancePage;

    // Timers
    private DispatcherTimer? _connectionTimer;

    private string _currentPage = "ReceiptDashboard";

    public MainWindow(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        _isLoaded = true;
        UserNameText.Text = $"مرحباً، {_api.CurrentUserName}";

        // Load the default page
        LoadPage("ReceiptDashboard");

        // Start connection check timer (every 60s)
        _connectionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
        _connectionTimer.Tick += async (_, _) => await CheckConnectionAsync();
        _connectionTimer.Start();

        // Token refresh is now handled inside ApiClient (check-after-call, not flat timer)
    }

    // ══════════════════════════════════════
    // CONNECTION & TOKEN
    // ══════════════════════════════════════

    private async Task CheckConnectionAsync()
    {
        try
        {
            bool connected = await _api.CheckConnectionAsync();
            Dispatcher.Invoke(() =>
            {
                if (connected)
                {
                    ConnectionDot.Fill = (Brush)FindResource("AccentSuccessBrush");
                    ConnectionText.Text = "متصل";
                    ConnectionText.Foreground = (Brush)FindResource("AccentSuccessBrush");
                }
                else
                {
                    ConnectionDot.Fill = (Brush)FindResource("AccentDangerBrush");
                    ConnectionText.Text = "غير متصل";
                    ConnectionText.Foreground = (Brush)FindResource("AccentDangerBrush");
                }
            });
        }
        catch
        {
            Dispatcher.Invoke(() =>
            {
                ConnectionDot.Fill = (Brush)FindResource("AccentDangerBrush");
                ConnectionText.Text = "غير متصل";
                ConnectionText.Foreground = (Brush)FindResource("AccentDangerBrush");
            });
        }
    }

    // ══════════════════════════════════════
    // NAVIGATION
    // ══════════════════════════════════════

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string tag)
            LoadPage(tag);
    }

    private void LoadPage(string pageName)
    {
        // Guard: don't navigate during InitializeComponent (RadioButton IsChecked fires early)
        if (!_isLoaded) return;

        if (pageName == "ConnectionSettings")
        {
            // Open settings window as dialog
            var settingsWindow = new ConnectionSettingsWindow(_api);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
            
            // Re-select the actual current page to avoid leaving the toggle stuck
            LoadPage(_currentPage);
            return;
        }

        _currentPage = pageName;

        switch (pageName)
        {
            // ── Module 1: Receipts ──
            case "ReceiptDashboard":
                _receiptDashboardPage ??= new ReceiptDashboardPage(_api);
                PageContent.Content = _receiptDashboardPage;
                PageTitle.Text = "لوحة التحكم";
                break;
            case "NewSession":
                _newSessionPage ??= new NewSessionPage(_api);
                PageContent.Content = _newSessionPage;
                PageTitle.Text = "جلسة جديدة";
                break;
            case "Sessions":
                _sessionsPage ??= new SessionsPage(_api);
                PageContent.Content = _sessionsPage;
                PageTitle.Text = "الجلسات";
                break;
            case "GapReview":
                _gapReviewPage ??= new GapReviewPage(_api);
                PageContent.Content = _gapReviewPage;
                PageTitle.Text = "الفجوات";
                break;
            case "ReceiptBooks":
                _receiptBooksPage ??= new ReceiptBooksPage(_api);
                PageContent.Content = _receiptBooksPage;
                PageTitle.Text = "دفاتر الإيصالات";
                break;
            case "ErpImport":
                _erpImportPage ??= new ErpImportPage(_api);
                PageContent.Content = _erpImportPage;
                PageTitle.Text = "استيراد ERP";
                break;
            case "DailyReport":
                _dailyReportPage ??= new DailyReportPage(_api);
                PageContent.Content = _dailyReportPage;
                PageTitle.Text = "التقرير اليومي";
                break;
            case "ReceiptSearch":
                _receiptSearchPage ??= new ReceiptSearchPage(_api);
                PageContent.Content = _receiptSearchPage;
                PageTitle.Text = "بحث الإيصالات";
                break;
            case "DriverPerformance":
                _driverPerformancePage ??= new DriverPerformancePage(_api);
                PageContent.Content = _driverPerformancePage;
                PageTitle.Text = "أداء السائقين";
                break;

            // ── Module 2: Routes ──
            case "RouteSetup":
                _routeSetupPage ??= new RouteSetupPage(_api);
                PageContent.Content = _routeSetupPage;
                PageTitle.Text = "🗺️ الخطوط والتجار";
                break;
            case "DailyInvoices":
                _dailyInvoicesPage ??= new DailyInvoicesPage(_api);
                PageContent.Content = _dailyInvoicesPage;
                PageTitle.Text = "📋 فواتير اليوم";
                break;
            case "RouteHistory":
                _routeHistoryPage ??= new RouteHistoryPage(_api);
                PageContent.Content = _routeHistoryPage;
                PageTitle.Text = "📜 سجل الخطوط";
                break;

            // ── Master Data ──
            case "Drivers":
                _driversPage ??= new DriversPage(_api);
                PageContent.Content = _driversPage;
                PageTitle.Text = "🚛 السائقون";
                break;
            case "Merchants":
                _merchantsPage ??= new MerchantsPage(_api);
                PageContent.Content = _merchantsPage;
                PageTitle.Text = "🏪 التجار";
                break;
        }
    }

    /// <summary>
    /// Creates a placeholder panel for pages not yet built (Phases 5-10).
    /// </summary>
    private StackPanel CreatePlaceholder(string title, string subtitle)
    {
        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        panel.Children.Add(new TextBlock
        {
            Text = "🚧",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12)
        });
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = (Brush)FindResource("TextPrimaryBrush")
        });
        panel.Children.Add(new TextBlock
        {
            Text = subtitle,
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = (Brush)FindResource("TextSecondaryBrush"),
            Margin = new Thickness(0, 8, 0, 0)
        });
        return panel;
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        _connectionTimer?.Stop();
        var login = new LoginWindow();
        login.Show();
        Close();
    }
}
