using System.Windows;
using System.Windows.Threading;
using ReceiptSystem.Desktop.Helpers;

namespace ReceiptSystem.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Catch all unhandled exceptions and show a message instead of crashing
        DispatcherUnhandledException += (s, args) =>
        {
            // Log to file for support troubleshooting
            ErrorMessageHelper.LogError(args.Exception);

            // Show Arabic-friendly message with technical details below
            var arabicMsg = ErrorMessageHelper.GetArabicMessage(args.Exception);
            var technical = ErrorMessageHelper.GetTechnicalDetails(args.Exception);

            var fullMsg = $"{arabicMsg}\n\n———— التفاصيل الفنية ————\n{technical}";

            MessageBox.Show(fullMsg, "خطأ — نظام البستاوي", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true; // Prevent crash
        };
    }
}
