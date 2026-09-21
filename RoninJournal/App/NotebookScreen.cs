using System.Windows;
using System.Windows.Controls;

namespace RoninJournal.Desktop;

public partial class MainWindow
{
    private void Notebook()
    {
        Page.Children.Clear();
        Intro("Keep the working record together. Inventory, daily journal entries and reference notes remain separate records, but can be opened from one notebook.");

        var title = Input("NotebookTitle", store.Data.NotebookTitle, max: 160);
        var header = Stack(
            Text("Notebook", 24, "#EDEFE9", true),
            Text("A single place to open the records that support your daily work.", 14, "#B0C1D5"));
        header.Children[1].SetValue(FrameworkElement.MarginProperty, new Thickness(0, 10, 0, 18));
        Field(header, "Notebook title", title);
        header.Children.Add(Button("Save notebook title", () =>
        {
            store.SaveNotebookTitle(title.Text);
            Notebook();
            Success("Notebook title saved locally.");
        }, "SaveNotebookTitle", true));
        Page.Children.Add(Card(header));

        var sections = Stack(
            Text("Notebook sections", 21, "#EDEFE9", true),
            Text("Choose a section below. Your existing records and save behavior are unchanged.", 14, "#B0C1D5"));
        sections.Children[1].SetValue(FrameworkElement.MarginProperty, new Thickness(0, 10, 0, 18));

        var inventory = Button("Inventory  →", () => Navigate("Inventory"), "NotebookInventory", true);
        var journal = Button("Today's journal  →", () => Navigate("Journal"), "NotebookJournal", true);
        var library = Button("Reference library  →", () => Navigate("Library"), "NotebookLibrary", true);
        foreach (var button in new[] { inventory, journal, library })
        {
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.Margin = new Thickness(0, 0, 0, 10);
            sections.Children.Add(button);
        }
        Page.Children.Add(Card(sections));
    }
}
