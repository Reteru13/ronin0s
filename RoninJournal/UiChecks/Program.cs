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
            var output=Path.GetFullPath(args.Length>0?args[0]:"ui-checks");Directory.CreateDirectory(output);
            var app=new App();app.InitializeComponent();
            using var store=new JournalStore(Path.Combine(output,"Fixture-"+Guid.NewGuid().ToString("N")));
            var window=new MainWindow(store);var content=(FrameworkElement)window.Content;
            void Layout(){content.Measure(new Size(1380,860));content.Arrange(new Rect(0,0,1380,860));content.UpdateLayout();}
            IEnumerable<DependencyObject> Walk(DependencyObject root){yield return root;for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)foreach(var child in Walk(VisualTreeHelper.GetChild(root,i)))yield return child;}
            T Find<T>(string name)where T:FrameworkElement=>Walk(content).OfType<T>().Single(x=>x.Name==name);
            void Click(string name){Layout();Find<Button>(name).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Layout();}
            void Assert(bool value,string message){if(!value)throw new Exception(message);}
            void Capture(string name){Layout();var render=new RenderTargetBitmap(1380,860,96,96,PixelFormats.Pbgra32);render.Render(content);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(render));using var f=File.Create(Path.Combine(output,name+".png"));png.Save(f);}
            Capture("01-today");
            Click("NotebookTab"); Find<TextBox>("NotebookTitle").Text="Daily operations notebook"; Click("SaveNotebookTitle"); Click("NavInventory"); Click("NavNotebook"); Assert(Find<TextBox>("NotebookTitle").Text=="Daily operations notebook","Notebook title did not persist"); 
            Click("NavInventory");Find<TextBox>("StockName").Text="Shipping boxes";Find<TextBox>("StockQuantity").Text="24";Click("SaveInventory");Assert(store.Data.Inventory.Single().Quantity==24,"Inventory button did not persist quantity");Capture("02-inventory");
            Click("NavJournal");Find<TextBox>("JournalNotes").Text="Reviewed packaging inventory and filed today's supplier receipt.";Find<CheckBox>("InventoryReviewed").IsChecked=true;Click("CompleteEntry");Assert(store.Data.Entries.Single().Submitted,"Journal completion did not persist");Capture("03-journal");
            Click("NavLibrary");Find<TextBox>("ReferenceTitle").Text="Supplier documentation";Find<TextBox>("ReferenceNotes").Text="Keep dated receipts and record reference numbers.";Click("SaveReference");Assert(store.Data.Library.Count==1,"Library save failed");Capture("04-library");
            var assetCandidates = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "App", "Assets", "ronin.png"),
                Path.Combine(Directory.GetCurrentDirectory(), "RoninJournal", "App", "Assets", "ronin.png"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "RoninJournal", "App", "Assets", "ronin.png")
            };
            var assetPath = assetCandidates.Select(Path.GetFullPath).FirstOrDefault(File.Exists)
                ?? throw new FileNotFoundException("Could not locate App/Assets/ronin.png from the current working directory. Run the UI harness from the repo root or parent folder.");
            store.AddPhoto(assetPath,DateOnly.FromDateTime(DateTime.Today),"Brand artwork reference",false);
            Click("NavPhotos");Capture("05-photos");Click("NavWeeklyreview");Assert(store.Week(DateOnly.FromDateTime(DateTime.Today)).Last().Points==70,"UI completion should score 70 without a confirmed photo");Capture("06-review");Click("NavBackup");Capture("07-backup");
            Click("NavOperations"); Assert(Find<Button>("NavOperations").Content?.ToString()?.Contains("Operations") == true, "Operations navigation failed");
            Click("NavFormulas"); Assert(Walk(content).OfType<TextBlock>().Any(x => x.Text.Contains("Estimated C:N")), "Formula screen did not render");
            Click("NavFamilytree");
            Find<TextBox>("CultureName").Text="Oyster master"; Find<TextBox>("CultureSpecies").Text="Pleurotus ostreatus"; Click("SaveCulture");
            Assert(Walk(content).OfType<Button>().Any(x => x.Content?.ToString()?.Contains("Oyster master") == true), "Saved root missing from tree");
            Click("AddChildCulture"); Find<TextBox>("CultureName").Text="Oyster clone"; Find<ComboBox>("CultureMethod").SelectedItem="Clone"; Click("SaveCulture");
            var cloneId=store.Data.Cultures.Single(x=>x.Name=="Oyster clone").Id;
            Click("NewCulture"); Find<TextBox>("CultureName").Text="Second source"; Find<TextBox>("CultureSpecies").Text="Pleurotus ostreatus"; Click("SaveCulture");
            Click("AddChildCulture"); Find<TextBox>("CultureName").Text="Oyster cross"; Find<ComboBox>("CultureMethod").SelectedItem="Cross"; Find<ComboBox>("CultureParent1").SelectedValue=cloneId; Click("SaveCulture");
            Find<TextBox>("CultureNotes").Text="Observe this lineage"; Click("SaveCulture");
            Assert(store.Data.Cultures.Single(x=>x.Name=="Oyster cross").Notes=="Observe this lineage", "Culture edit did not persist");
            Find<TextBox>("CultureSearch").Text="Oyster clone"; Layout();
            Assert(!Walk(content).OfType<Button>().Any(x=>x.Content?.ToString()?.Contains("Second source") == true), "Search did not filter tree");
            Find<TextBox>("CultureSearch").Text=""; Layout();
            Assert(Walk(Find<ComboBox>("CultureMethod")).OfType<TextBlock>().Any(x=>x.Text=="Cross" && x.Foreground is SolidColorBrush brush && brush.Color.R < 100), "Culture dropdown selected text has unreadable contrast");
            Assert(Walk(content).OfType<System.Windows.Shapes.Path>().Count(x => x.Tag?.ToString()?.StartsWith("RootLink:") == true) == 3, "Mycelium view must show every parent link, including both cross parents");
            Walk(content).OfType<Button>().Single(x=>x.Content?.ToString()?.StartsWith("Oyster clone\n")==true).RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Layout();
            Assert(Walk(content).OfType<System.Windows.Shapes.Path>().Count(x=>x.Tag?.ToString()?.StartsWith("RootLink:")==true && x.StrokeThickness==4)==1,"Selecting a culture did not highlight its ancestry");
            Walk(content).OfType<Button>().Single(x=>x.Content?.ToString()?.StartsWith("Oyster cross\n")==true).RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Layout();
            Click("CollapseCultures"); Assert(!Walk(content).OfType<Button>().Any(x=>x.Content?.ToString()?.StartsWith("Oyster clone\n")==true),"Collapse did not hide descendants"); Click("ExpandCultures");
            var rootCanvas=Find<Canvas>("CultureTree"); var oldTransform=rootCanvas.LayoutTransform; rootCanvas.LayoutTransform=Transform.Identity; Layout();
            var rootRender=new RenderTargetBitmap((int)rootCanvas.Width,(int)rootCanvas.Height,96,96,PixelFormats.Pbgra32); rootRender.Render(rootCanvas); var rootPng=new PngBitmapEncoder(); rootPng.Frames.Add(BitmapFrame.Create(rootRender)); using(var rootFile=File.Create(Path.Combine(output,"09-mycelium-detail.png"))) rootPng.Save(rootFile); rootCanvas.LayoutTransform=oldTransform; Layout();
            Capture("09-family-tree");
            Walk(content).OfType<Button>().Single(x=>x.Content?.ToString()=="+ Add child").RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Layout();
            Assert(Find<TextBox>("CultureName").Text=="" && (Guid)Find<ComboBox>("CultureParent0").SelectedValue==store.Data.Cultures.Single(x=>x.Name=="Oyster cross").Id,"Root tip did not prepare a child culture");
            Click("NavToday"); Click("NavFamilytree");
            Assert(Walk(content).OfType<Button>().Any(x => x.Content?.ToString()?.Contains("Oyster clone") == true), "Child lost after navigation");
            Click("NavVision"); Assert(Walk(content).OfType<TextBlock>().Any(x => x.Text.Contains("inspection", StringComparison.OrdinalIgnoreCase)), "Vision screen did not render");
            Click("NavGenetics"); Find<TextBox>("GeneticsCulture").Text="Oyster master"; Find<TextBox>("GeneticsNotes").Text="Even growth observed"; Find<TextBox>("GeneticsPassage").Text="2"; Click("SaveGenetics");
            Click("NavToday"); Click("NavGenetics");
            Assert(Walk(content).OfType<TextBlock>().Any(x=>x.Text.Contains("Even growth observed")), "Genetics history did not retain observation");
            Walk(content).OfType<Button>().Single(x=>x.Content?.ToString()=="Edit observation").RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Layout();
            Find<TextBox>("GeneticsNotes").Text="Updated growth observation"; Find<ComboBox>("GeneticsCondition").SelectedItem="Strong"; Click("SaveGenetics");
            Assert(store.Data.Cultivation.Count(x=>x.Type=="Genetics")==1 && store.Data.Cultivation.Single(x=>x.Type=="Genetics").Condition=="Strong", "Genetics edit duplicated or lost entry");
            Find<TextBox>("GeneticsSearch").Text="no match"; Layout(); Assert(!Walk(content).OfType<TextBlock>().Any(x=>x.Text=="Updated growth observation"), "Genetics search failed"); Find<TextBox>("GeneticsSearch").Text=""; Layout();
            Capture("10-genetics");
            Click("NavBatchplanner"); Assert(Walk(content).OfType<TextBlock>().Any(x => x.Text.Contains("batch", StringComparison.OrdinalIgnoreCase)), "Batch planner screen did not render");
            Click("NavPhotos");Walk(content).OfType<Button>().Single(x=>x.Content?.ToString()=="Edit caption / confirmation").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Layout();Find<TextBox>("PhotoCaption").Text="Confirmed brand reference";Find<CheckBox>("PhotoQuality").IsChecked=true;Click("AddPhoto");Assert(store.Data.Photos.Single().Caption=="Confirmed brand reference" && store.Data.Photos.Single().QualityConfirmed,"Photo metadata editing failed");
            Click("NavJournal");Find<TextBox>("JournalNotes").Text="Unsaved editing should persist as a draft when navigating.";Click("NavToday");Assert(!store.Data.Entries.Single().Submitted && store.Data.Entries.Single().Notes.StartsWith("Unsaved"),"Navigation lost draft");
            content.Measure(new Size(1100,720));content.Arrange(new Rect(0,0,1100,720));content.UpdateLayout();Assert(content.ActualWidth==1100,"Minimum layout failed");
            var compact=new RenderTargetBitmap(1100,720,96,96,PixelFormats.Pbgra32);compact.Render(content);var compactPng=new PngBitmapEncoder();compactPng.Frames.Add(BitmapFrame.Create(compact));using(var f=File.Create(Path.Combine(output,"08-compact.png")))compactPng.Save(f);
            Console.WriteLine("PASS UI: inventory, journal, library, scoring, navigation drafts; family tree source, clone, cross, edit and search; actual WPF screen renders. Output: "+output);window.Close();return 0;
        }
        catch(Exception e){Console.WriteLine(e);return 1;}
    }
}
