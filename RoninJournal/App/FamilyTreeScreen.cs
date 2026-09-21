using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RoninJournal.Core;

namespace RoninJournal.Desktop;

public partial class MainWindow
{
    private void FamilyTree()
    {
        Intro("Follow your culture names down the mycelium roots. Click a name to edit or add a child; its ancestry lights up. Crosses connect to both parents.");
        var search = Input("CultureSearch");
        var tree = new Canvas { Name = "CultureTree", Background = Color("#10291F") };
        var viewport = new ScrollViewer { Content = tree, Height = 580, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        viewport.Resources[typeof(System.Windows.Controls.Primitives.ScrollBar)] = new Style(typeof(System.Windows.Controls.Primitives.ScrollBar));
        Guid selectedId = Guid.Empty; var collapsed = new HashSet<Guid>(); double zoom = .72;
        var form = Stack();
        void Edit(CultureRecord record)
        {
            selectedId = record.Id;
            form.Children.Clear();
            form.Children.Add(Text(store.Data.Cultures.Any(x => x.Id == record.Id) ? "Culture details" : "New culture", 20, "#89E4E0", true));
            var name = Input("CultureName", record.Name);
            var species = Input("CultureSpecies", record.Species);
            var date = Input("CultureDate", record.Date.ToString("yyyy-MM-dd"));
            var notes = Input("CultureNotes", record.Notes, true, 20000);
            var method = new ComboBox { Name = "CultureMethod", ItemsSource = new[] { "Source", "Transfer", "Clone", "Isolate", "Cross" }, SelectedItem = record.Method, Foreground = Color("#17212E"), MinHeight = 34, Margin = new Thickness(0, 7, 0, 16) };
            void Readable(ComboBox box, string binding = ".")
            {
                var text = new FrameworkElementFactory(typeof(TextBlock));
                text.SetValue(TextBlock.ForegroundProperty, Color("#17212E"));
                text.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(binding));
                box.DisplayMemberPath = "";
                box.ItemTemplate = new DataTemplate { VisualTree = text };
            }
            Readable(method);
            ComboBox Parent(int index)
            {
                var box = new ComboBox { Name = "CultureParent" + index, DisplayMemberPath = "Name", SelectedValuePath = "Id", Foreground = Color("#17212E"), MinHeight = 34, Margin = new Thickness(0, 7, 0, 16) };
                Readable(box, "Name");
                box.Items.Add(new CultureRecord { Id = Guid.Empty, Name = "No parent" });
                foreach (var x in store.Data.Cultures.Where(x => x.Id != record.Id).OrderBy(x => x.Name)) box.Items.Add(x);
                box.SelectedValue = record.ParentIds.Count > index ? record.ParentIds[index] : Guid.Empty;
                return box;
            }
            var first = Parent(0); var second = Parent(1);
            Field(form, "Name", name); Field(form, "Species / strain", species); Field(form, "Date (YYYY-MM-DD)", date); Field(form, "Relationship", method);
            Field(form, "Parent", first); Field(form, "Second parent (cross only)", second); Field(form, "Notes", notes);
            var ancestors = new HashSet<Guid>(); var queue = new Queue<Guid>(record.ParentIds);
            while (queue.TryDequeue(out var id)) if (ancestors.Add(id)) foreach (var parent in store.Data.Cultures.Single(x => x.Id == id).ParentIds) queue.Enqueue(parent);
            if (ancestors.Count > 0) form.Children.Add(Text("Ancestry: " + string.Join(" • ", store.Data.Cultures.Where(x => ancestors.Contains(x.Id)).Select(x => x.Name)), 13, "#89E4E0"));
            form.Children.Add(Button("Save culture", () =>
            {
                var parents = new[] { (Guid)first.SelectedValue, (Guid)second.SelectedValue }.Where(x => x != Guid.Empty).ToList();
                var saved = new CultureRecord { Id = record.Id, Name = name.Text.Trim(), Species = species.Text.Trim(), Date = Date(date), Method = (string)method.SelectedItem, Notes = notes.Text, ParentIds = parents };
                store.SaveCulture(saved); search.Text = ""; Render(); Edit(saved); Success("Culture saved locally.");
            }, "SaveCulture", true));
            if (store.Data.Cultures.Any(x => x.Id == record.Id))
                form.Children.Add(Button("Add child culture", () => Edit(new CultureRecord { Species = record.Species, Method = "Transfer", ParentIds = [record.Id] }), "AddChildCulture"));
            Render();
        }
        void Render()
        {
            HashSet<Guid> Ancestors(Guid id)
            {
                var ids = new HashSet<Guid>(); var pending = new Queue<Guid>(store.Data.Cultures.Single(x => x.Id == id).ParentIds);
                while (pending.TryDequeue(out var parent)) if (ids.Add(parent)) foreach (var next in store.Data.Cultures.Single(x => x.Id == parent).ParentIds) pending.Enqueue(next);
                return ids;
            }
            var query = search.Text.Trim();
            var matches = store.Data.Cultures.Where(x => (x.Name + " " + x.Species + " " + x.Notes).Contains(query, StringComparison.OrdinalIgnoreCase)).Select(x => x.Id).ToHashSet();
            var visible = new HashSet<Guid>(matches);
            foreach (var id in matches) visible.UnionWith(Ancestors(id));
            var shown = store.Data.Cultures.Where(x => visible.Contains(x.Id) && (query.Length > 0 || !Ancestors(x.Id).Any(collapsed.Contains))).ToList();
            DrawMycelium(tree, shown, selectedId, Edit, id => { if (!collapsed.Remove(id)) collapsed.Add(id); Render(); }, collapsed, store.Data.Cultures.SelectMany(x => x.ParentIds).ToHashSet(), parent => Edit(new() { Species = parent.Species, Method = "Transfer", ParentIds = [parent.Id] }));
            tree.LayoutTransform = new ScaleTransform(zoom, zoom);
            if (shown.Count == 0)
            {
                var empty = Text(store.Data.Cultures.Count == 0 ? "Add your first source culture\nto start the roots." : "No matching cultures.", 17, "#CFDEC6");
                empty.Width = 280; empty.TextAlignment = TextAlignment.Center; Canvas.SetLeft(empty, tree.Width / 2 - 140); Canvas.SetTop(empty, 285); tree.Children.Add(empty);
            }
        }
        var actions = new WrapPanel();
        actions.Children.Add(Button("+ New source", () => Edit(new()), "NewCulture", true));
        actions.Children.Add(Button("Expand all", () => { collapsed.Clear(); Render(); }, "ExpandCultures"));
        actions.Children.Add(Button("Collapse all", () => { collapsed.UnionWith(store.Data.Cultures.SelectMany(x => x.ParentIds)); Render(); }, "CollapseCultures"));
        var zoomActions = new WrapPanel { Margin = new Thickness(0, 8, 0, 8) };
        zoomActions.Children.Add(Button("Zoom −", () => { zoom = Math.Max(.4, zoom - .1); Render(); }));
        zoomActions.Children.Add(Button("Zoom +", () => { zoom = Math.Min(1.5, zoom + .1); Render(); }));
        zoomActions.Children.Add(Button("Fit width", () => { zoom = Math.Clamp((viewport.ViewportWidth - 20) / tree.Width, .4, 1); Render(); viewport.ScrollToHorizontalOffset(0); viewport.ScrollToVerticalOffset(0); }));
        var left = Stack(Text("Mycelium family tree", 20, "#89E4E0", true), Text("Search name, species or notes", 12), search, actions, zoomActions, viewport, Text("Sources are closest to the stem; descendants grow downward. Named connections show ancestry. The mushroom represents your collection. Scroll or zoom to explore.", 12, "#A4B3C9"));
        search.TextChanged += (_, _) => Render();
        Page.Children.Add(Columns(Card(left), Card(form), 1.7, 1));
        Render(); Edit(store.Data.Cultures.FirstOrDefault() ?? new());
    }
}
