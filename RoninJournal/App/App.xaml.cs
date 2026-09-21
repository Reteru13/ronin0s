using System.IO;
using System.Windows;
using RoninJournal.Core;

namespace RoninJournal.Desktop;
public partial class App : Application
{
    private JournalStore? store;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            store = new JournalStore(Path.Combine(AppContext.BaseDirectory, "Data"));
            var window = new MainWindow(store); MainWindow = window; window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ronin Journal could not open your records. If it is already running, use that window. Otherwise, preserve the Data folder before recovery.\n\n" + ex.Message, "Ronin Journal", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
    protected override void OnExit(ExitEventArgs e) { store?.Dispose(); base.OnExit(e); }
}
