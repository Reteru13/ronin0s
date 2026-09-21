using System.Windows;
using System.Windows.Controls;
using RoninJournal.Core;

namespace RoninJournal.Desktop;
public partial class MainWindow
{
    private void Inventory(StockItem? editing = null, bool archived = false)
    {
        Page.Children.Clear(); Intro("Supplies, packaging and business assets. Keep quantities and locations in one place.");
        var list = new StackPanel(); var search = Input("InventorySearch", "", false, 200); Field(list, "Search inventory", search);
        var archiveToggle = new CheckBox { Content = "Include archived items", IsChecked = archived }; list.Children.Add(archiveToggle);
        var results = new StackPanel(); list.Children.Add(results);
        void Populate()
        {
            results.Children.Clear(); var query = search.Text.Trim();
            var items = store.Data.Inventory.Where(x => (archiveToggle.IsChecked == true || !x.Archived) && (x.Name + " " + x.Category + " " + x.Location).Contains(query, StringComparison.OrdinalIgnoreCase)).OrderBy(x => x.Archived).ThenBy(x => x.Name).ToList();
            if (items.Count == 0) results.Children.Add(Card(Stack(Text("Room for your first record", 19, "#EDEFE9", true), Text("Add an item using the form. Your saved records will appear here.", 14, "#A4B3C9"))));
            foreach (var item in items)
            {
                var content = Stack(Text(item.Name, 19, "#EDEFE9", true), Text(item.Quantity.ToString("0.##") + " " + item.Unit + "  ·  " + item.Category, 14, "#85DED9"), Text(string.IsNullOrWhiteSpace(item.Location) ? "No location recorded" : item.Location, 12, "#A4B3C9"));
                content.Children[1].SetValue(MarginProperty, new Thickness(0, 9, 0, 8));
                var badge = Text(item.Archived ? "ARCHIVED" : item.Quantity <= item.ReorderAt ? "REORDER LEVEL REACHED" : "IN STOCK", 10, item.Quantity <= item.ReorderAt ? "#EBC69A" : "#A5B9D2"); badge.Margin = new Thickness(0, 12, 0, 12); content.Children.Add(badge);
                content.Children.Add(Button("Edit record  →", () => Inventory(Clone(item), archiveToggle.IsChecked == true))); results.Children.Add(Card(content));
            }
        }
        search.TextChanged += (_, _) => Populate(); archiveToggle.Checked += (_, _) => Populate(); archiveToggle.Unchecked += (_, _) => Populate(); Populate();
        var itemEdit = editing ?? new StockItem(); var form = Stack(Text(editing == null ? "Add an inventory item" : "Edit inventory item", 21, "#EDEFE9", true));
        form.Children[0].SetValue(MarginProperty, new Thickness(0, 0, 0, 20));
        var name = Input("StockName", itemEdit.Name); var category = Input("StockCategory", itemEdit.Category, max:80); var quantity = Input("StockQuantity", itemEdit.Quantity.ToString()); var unit = Input("StockUnit", itemEdit.Unit, max:40); var reorder = Input("StockReorder", itemEdit.ReorderAt.ToString()); var location = Input("StockLocation", itemEdit.Location, max:160); var notes = Input("StockNotes", itemEdit.Notes, true, 20000);
        Field(form, "Item name *", name); Field(form, "Category *", category);
        var q = new StackPanel(); Field(q, "Quantity *", quantity); var u = new StackPanel(); Field(u, "Unit *", unit); form.Children.Add(Columns(q, u));
        Field(form, "Reorder at or below", reorder); Field(form, "Location", location); Field(form, "Notes", notes);
        form.Children.Add(Button("Save item", () => {
            itemEdit.Name = name.Text.Trim(); itemEdit.Category = category.Text.Trim(); itemEdit.Quantity = Number(quantity); itemEdit.Unit = unit.Text.Trim(); itemEdit.ReorderAt = Number(reorder); itemEdit.Location = location.Text.Trim(); itemEdit.Notes = notes.Text;
            store.UpsertStock(itemEdit); Inventory(); Success("Inventory item saved on this computer.");
        }, "SaveInventory", true));
        if (editing != null)
        {
            var archive = Button(itemEdit.Archived ? "Restore to active inventory" : "Archive this item", () => { itemEdit.Archived = !itemEdit.Archived; store.UpsertStock(itemEdit); Inventory(); Success("Inventory updated. Archived records are retained."); }); archive.Margin = new Thickness(0, 12, 0, 0); form.Children.Add(archive);
            var cancel = Button("Cancel editing", () => Inventory()); cancel.Margin = new Thickness(0,12,0,0); form.Children.Add(cancel);
        }
        Page.Children.Add(Columns(list, Card(form)));
    }
}
