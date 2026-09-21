using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RoninJournal.Core;
using RoninJournal.Desktop;

class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            var output = Path.GetFullPath(args.Length > 0 ? args[0] : "qa/recipes");
            Directory.CreateDirectory(output);
            var app = new App(); app.InitializeComponent();
            using var store = new JournalStore(Path.Combine(output, "Fixture-" + Guid.NewGuid().ToString("N")));
            var window = new MainWindow(store);
            var content = (FrameworkElement)window.Content;
            double width = 1380, height = 860;
            void Layout() { content.Measure(new Size(width, height)); content.Arrange(new Rect(0, 0, width, height)); content.UpdateLayout(); }
            IEnumerable<DependencyObject> Walk(DependencyObject root)
            {
                yield return root;
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
                    foreach (var child in Walk(VisualTreeHelper.GetChild(root, i))) yield return child;
            }
            T Find<T>(string name) where T : FrameworkElement => Walk(content).OfType<T>().Single(x => x.Name == name);
            void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
            void Click(string name)
            {
                Layout(); Find<Button>(name).RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Layout();
                Assert(!Find<TextBlock>("Status").Text.StartsWith("Could not complete:"), Find<TextBlock>("Status").Text);
            }
            void Capture(string name)
            {
                Layout(); var bitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(content); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var f = File.Create(Path.Combine(output, name + ".png")); encoder.Save(f);
            }

            Layout();
            Assert(Find<Image>("BrandLogo").Source is BitmapSource { PixelWidth: > 0 }, "Brand logo failed to decode");
            Capture("01-home-logo");
            Click("NavRecipes");
            Assert(Walk(content).OfType<Image>().Count(i => i.Name.StartsWith("RecipePhoto") && i.Source is BitmapSource { PixelWidth: > 0 }) == 3, "Category images missing");
            Capture("02-recipe-categories");
            foreach (var id in new[] { "agar", "grain", "substrate" })
            {
                Click("RecipeCategory" + id);
                Assert(Walk(content).OfType<Button>().Count(b => b.Name.StartsWith("RecipeEntry")) == 5, "Expected five draft entries in " + id);
                if (id == "agar") Capture("03-agar-entries");
                for (int number = 1; number <= 5; number++)
                {
                    Click("RecipeEntry" + number);
                    var text = string.Join("\n", Walk(content).OfType<TextBlock>().Select(t => t.Text));
                    Assert(text.Contains("Entry ID: " + id + "-" + number.ToString("00")), "Wrong entry route");
                    Assert(text.Contains("REFERENCE PROTOCOL") && text.Contains("Ingredients / ratios") && text.Contains("Preparation / method") && text.Contains("Pros") && text.Contains("Cons") && text.Contains("Best used for"), "Recipe detail sections missing");
                    if (id == "agar" && number == 1) Capture("04-entry-detail");
                    Click("RecipeBack");
                }
            }
            Click("RecipeCategoryall");
            width = 1100; height = 720; Capture("05-compact-categories");
            // A larger logo must not leave the final navigation item permanently clipped.
            var backupButton = Find<Button>("NavBackup");
            DependencyObject ancestor = backupButton;
            while (ancestor is not ScrollViewer) ancestor = VisualTreeHelper.GetParent(ancestor) ?? throw new Exception("Navigation scroll container missing");
            var navigationScroll = (ScrollViewer)ancestor;
            navigationScroll.ScrollToEnd(); Layout();
            var backupBounds = backupButton.TransformToAncestor(navigationScroll).TransformBounds(new Rect(backupButton.RenderSize));
            Assert(backupBounds.Top >= -1 && backupBounds.Bottom <= navigationScroll.ActualHeight + 1, "Backup cannot be scrolled into the compact navigation viewport");
            Click("NavBackup"); Assert(Find<TextBlock>("PageTitle").Text == "Backup", "Compact Backup navigation failed");
            Capture("06-compact-backup");
            Click("BrandHome"); Assert(Find<TextBlock>("PageTitle").Text == "Today", "Logo must return home");
            var allText = string.Join("\n", Walk(content).OfType<TextBlock>().Select(t => t.Text));
            Assert(!allText.Contains("Jamaica"), "Private location exposed in shell");
            Click("NavJournal"); Find<TextBox>("JournalNotes").Text = "Draft before opening Recipes";
            Click("NavRecipes"); Assert(store.Data.Entries.Single().Notes == "Draft before opening Recipes", "Recipe navigation lost journal draft");
            Console.WriteLine("PASS Recipes UI: 3 category images, all 15 entry routes, back links, home logo, compact layout, draft persistence. Renders: " + output);
            window.Close(); return 0;
        }
        catch (Exception error) { Console.WriteLine(error); return 1; }
    }
}
