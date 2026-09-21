using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using RoninJournal.Core;

namespace RoninJournal.Desktop;
public partial class MainWindow : Window
{
    private readonly JournalStore store;
    private Action? flushDraft;
    private readonly DispatcherTimer draftTimer = new() { Interval = TimeSpan.FromMilliseconds(700) };
    private readonly Dictionary<string, Button> navigation = [];
    private DateOnly reviewWeek = Monday(Today);
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
    private static DateOnly Monday(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
    private static Brush Color(string hex) => (Brush)new BrushConverter().ConvertFromString(hex)!;
    public MainWindow(JournalStore store)
    {
        this.store = store; InitializeComponent();
        DateLabel.Text = DateTime.Today.ToString("dddd, MMMM d");
        string[] names = ["Today", "Notebook", "Operations", "Inventory", "Journal", "Photos", "Recipes", "Formulas", "Family tree", "Vision", "Genetics", "Batch planner", "Library", "Weekly review", "Backup"];
        string[] glyphs = ["◈", "▣", "✦", "▤", "≡", "▧", "▦", "⚗", "≈", "⌁", "∿", "⚙", "◇", "◷", "↗"];
        for (int i = 0; i < names.Length; i++)
        {
            var name = names[i]; var b = Button(glyphs[i] + "   " + name, () => Navigate(name), "Nav" + name.Replace(" ", ""));
            b.HorizontalContentAlignment = HorizontalAlignment.Left; b.HorizontalAlignment = HorizontalAlignment.Stretch; b.Margin = new Thickness(0, 0, 0, 9); b.Background = Brushes.Transparent; b.BorderThickness = new Thickness(0); Navigation.Children.Add(b); navigation[name] = b;
        }
        draftTimer.Tick += (_, _) => { draftTimer.Stop(); Safe(() => flushDraft?.Invoke()); };
        Closing += (_, e) => { draftTimer.Stop(); try { flushDraft?.Invoke(); } catch (Exception ex) { Status.Text = "Save failed: " + ex.Message; MessageBox.Show(this, "Your latest changes could not be saved. Keep this window open and check available disk space.\n\n" + ex.Message, "Save failed"); e.Cancel = true; } };
        Navigate("Today");
    }
    public void Navigate(string name)
    {
        Safe(() =>
        {
            draftTimer.Stop(); flushDraft?.Invoke(); flushDraft = null;
            Page.Children.Clear(); PageScroll.ScrollToTop(); PageTitle.Text = name;
            DateLabel.Text = DateTime.Today.ToString("dddd, MMMM d");
            foreach (var pair in navigation) { pair.Value.Background = Color(pair.Key == name ? "#20354C" : "#0C1626"); pair.Value.Foreground = Color(pair.Key == name ? "#83F3EB" : "#B6C5DA"); }
            Eyebrow.Text = name == "Today" ? "THE DAILY PRACTICE" : "RONIN MUSHROOMS CO. / YOUR RECORDS";
            switch (name) { case "Today": Dashboard(); break; case "Notebook": Notebook(); break; case "Operations": Operations(); break; case "Inventory": Inventory(); break; case "Journal": Journal(Today); break; case "Photos": Photos(); break; case "Recipes": Recipes(); break; case "Formulas": Formulas(); break; case "Family tree": FamilyTree(); break; case "Vision": Vision(); break; case "Genetics": Genetics(); break; case "Batch planner": BatchPlanner(); break; case "Library": Library(); break; case "Weekly review": Review(); break; case "Backup": Backup(); break; }
        });
    }
    private void BrandHome_Click(object sender, RoutedEventArgs e) => Navigate("Today");
    private void Safe(Action action)
    { try { action(); } catch (Exception e) { Status.Text = "Could not complete: " + e.Message; Status.Foreground = Color("#FFBABE"); } }
    private void Success(string text) { Status.Text = text; Status.Foreground = Color("#8AE7CD"); }
    private static T Clone<T>(T source) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(source))!;
    private static TextBlock Text(string value, double size = 14, string color = "#EDEFE9", bool bold = false) => new() { Text = value, FontSize = size, Foreground = Color(color), FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal, TextWrapping = TextWrapping.Wrap };
    private static StackPanel Stack(params UIElement[] items) { var s = new StackPanel(); foreach (var item in items) s.Children.Add(item); return s; }
    private static Border Card(UIElement child, double padding = 22) => new() { Child = child, Background = Color("#101E30"), BorderBrush = Color("#263A52"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(13), Padding = new Thickness(padding), Margin = new Thickness(0, 0, 0, 16) };
    private Button Button(string title, Action action, string name = "", bool primary = false)
    {
        var b = new Button { Content = title, Name = name, HorizontalContentAlignment = HorizontalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left };
        if (primary) { b.Background = Color("#276064"); b.BorderBrush = Color("#79EBDF"); b.Foreground = Color("#F1FFFB"); }
        b.Click += (_, _) => Safe(action); return b;
    }
    private static TextBox Input(string name, string value = "", bool multiline = false, int max = 120)
    {
        var box = new TextBox { Name = name, Text = value, MaxLength = max, Margin = new Thickness(0, 7, 0, 16), AcceptsReturn = multiline, TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap, VerticalScrollBarVisibility = multiline ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled };
        if (multiline) { box.Height = 130; box.VerticalContentAlignment = VerticalAlignment.Top; }
        System.Windows.Automation.AutomationProperties.SetName(box, name); return box;
    }
    private static void Field(Panel panel, string label, Control input) { panel.Children.Add(Text(label, 12, "#BCCBDE")); panel.Children.Add(input); }
    private static Grid Columns(UIElement left, UIElement right, double leftWeight = 1, double rightWeight = 1)
    {
        var grid = new Grid(); grid.ColumnDefinitions.Add(new() { Width = new GridLength(leftWeight, GridUnitType.Star) }); grid.ColumnDefinitions.Add(new() { Width = new GridLength(22) }); grid.ColumnDefinitions.Add(new() { Width = new GridLength(rightWeight, GridUnitType.Star) });
        grid.Children.Add(left); Grid.SetColumn(right, 2); grid.Children.Add(right); return grid;
    }
    private void Intro(string text) { var t = Text(text, 14, "#A4B3C9"); t.Margin = new Thickness(0, 0, 0, 22); Page.Children.Add(t); }
    private static BitmapImage Bitmap(string path, int width = 1200)
    { var b = new BitmapImage(); b.BeginInit(); b.CacheOption = BitmapCacheOption.OnLoad; b.DecodePixelWidth = width; b.UriSource = new Uri(path, UriKind.Absolute); b.EndInit(); b.Freeze(); return b; }
    private static decimal Number(TextBox box) => decimal.TryParse(box.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var value) ? value : throw new ArgumentException("Enter a valid number for " + box.Name + ".");
    private static DateOnly Date(TextBox box) => DateOnly.TryParseExact(box.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : throw new ArgumentException("Use a date in YYYY-MM-DD format.");
    private void Dashboard()
    {
        var hero = new Grid { Height = 320, ClipToBounds = true };
        hero.ColumnDefinitions.Add(new() { Width = new GridLength(0.9, GridUnitType.Star) }); hero.ColumnDefinitions.Add(new() { Width = new GridLength(1.1, GridUnitType.Star) });
        var art = new Image { Source = Bitmap("pack://application:,,,/RoninJournal;component/Assets/ronin.png"), Stretch = Stretch.UniformToFill }; Grid.SetColumn(art, 1); hero.Children.Add(art);
        var welcome = Stack(Text("CULTIVATE YOUR CONSISTENCY", 10, "#89E4E0", true), Text("A little focus.\nA lasting record.", 32, "#F3F0E7", true), Text("A calm place to document your business,\none day at a time.", 14, "#B8C7DC"));
        welcome.Margin = new Thickness(25, 28, 10, 22); ((TextBlock)welcome.Children[1]).Margin = new Thickness(0, 16, 0, 14);
        var begin = Button("Open today's journal  →", () => Navigate("Journal"), "BeginJournal", true); begin.Margin = new Thickness(0, 22, 0, 0); welcome.Children.Add(begin);
        var notebook = Button("Open notebook  →", () => Navigate("Notebook"), "NotebookTab"); notebook.Margin = new Thickness(0, 12, 0, 0); welcome.Children.Add(notebook); hero.Children.Add(welcome);
        Page.Children.Add(Card(hero, 0));
        var scores = store.Week(Today); int avg = scores.Count == 0 ? 0 : (int)Math.Round(scores.Average(x => x.Points));
        var active = store.Data.Inventory.Where(x => !x.Archived).ToList();
        Border Metric(string title, string value, string sub) { var s = Stack(Text(title, 11, "#9AADC6"), Text(value, 30, "#EDEFE9", true), Text(sub, 12, "#A6B9D1")); s.Children[1].SetValue(MarginProperty, new Thickness(0, 9, 0, 7)); return Card(s); }
        var metrics = new Grid(); for (int i = 0; i < 3; i++) metrics.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        var tiles = new[] { Metric("THIS WEEK", avg + "%", "Documentation completeness"), Metric("INVENTORY", active.Count.ToString(), active.Count(x => x.Quantity <= x.ReorderAt) + " at or below reorder level"), Metric("PHOTO JOURNAL", store.Data.Photos.Count.ToString(), "Locally stored photo records") };
        for (int i = 0; i < tiles.Length; i++) { Grid.SetColumn(tiles[i], i); tiles[i].Margin = new Thickness(i == 0 ? 0 : 8, 0, i == 2 ? 0 : 8, 16); metrics.Children.Add(tiles[i]); } Page.Children.Add(metrics);
        var entry = store.Data.Entries.Find(x => x.Date == Today);
        var ritual = Stack(Text("Your daily rhythm", 20, "#EDEFE9", true), Text("Three small steps. One clear record.", 13, "#A4B3C9"));
        string[] lines = [(entry?.Submitted == true ? "✓" : "○") + "   Write and complete today's journal", (entry?.InventoryReviewed == true ? "✓" : "○") + "   Review your inventory records", (store.Data.Photos.Any(x => x.Date == Today && x.QualityConfirmed) ? "✓" : "○") + "   Attach a captioned, checked photo"];
        foreach (var line in lines) { var t = Text(line, 14); t.Margin = new Thickness(0, 17, 0, 0); ritual.Children.Add(t); }
        var recent = Stack(Text("Keep the story intact", 20, "#EDEFE9", true), Text("Honest notes count. Recording a problem never reduces your score. Late entries remain marked as late.", 14, "#B0BFD2")); recent.Children[1].SetValue(MarginProperty, new Thickness(0, 14, 0, 20)); recent.Children.Add(Button("Review the week  →", () => Navigate("Weekly review")));
        Page.Children.Add(Columns(Card(ritual), Card(recent)));
    }
}
