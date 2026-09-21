using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using RoninJournal.Core;

namespace RoninJournal.Desktop;

public partial class MainWindow
{
    private void Genetics()
    {
        Intro("Enter a culture name, date and notes, then save. Transfer number and condition are optional. Reopen saved entries from your history.");
        var editor = Stack(); var history = Stack(); var search = Input("GeneticsSearch");
        void Edit(CultivationRecord? existing = null)
        {
            var draft = existing == null ? new CultivationRecord { Type = "Genetics", ObservationDate = Today } : Clone(existing);
            editor.Children.Clear();
            editor.Children.Add(Text(existing == null ? "New observation" : "Edit observation", 21, "#89E4E0", true));
            var culture = Input("GeneticsCulture", draft.Title);
            var date = Input("GeneticsDate", (draft.ObservationDate ?? DateOnly.FromDateTime(draft.Created.LocalDateTime)).ToString("yyyy-MM-dd"));
            var passage = Input("GeneticsPassage", draft.Passage?.ToString() ?? "");
            var notes = Input("GeneticsNotes", draft.Details, true, 20000);
            notes.Height = 85;
            Field(editor, "Culture / strain name *", culture);
            if (store.Data.Cultures.Count > 0)
            {
                var names = new List<CultureRecord> { new() { Id = Guid.Empty, Name = "Choose a saved culture…" } }; names.AddRange(store.Data.Cultures.OrderBy(x => x.Name));
                var choices = new ComboBox { Name = "GeneticsKnownCulture", ItemsSource = names, SelectedIndex = 0, SelectedValuePath = "Id", MinHeight = 32, Margin = new Thickness(0, 0, 0, 12) };
                var factory = new FrameworkElementFactory(typeof(TextBlock)); factory.SetValue(TextBlock.ForegroundProperty, Color("#17212E")); factory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Name")); choices.ItemTemplate = new DataTemplate { VisualTree = factory };
                choices.SelectedIndex = 0;
                choices.SelectionChanged += (_, _) => { if (choices.SelectedItem is CultureRecord record && record.Id != Guid.Empty) culture.Text = record.Name; };
                Field(editor, "Or choose a name from your family tree", choices);
            }
            var dateField = Stack(); Field(dateField, "Date * (YYYY-MM-DD)", date);
            var passageField = Stack(); Field(passageField, "Transfer number (optional)", passage);
            editor.Children.Add(Columns(dateField, passageField));
            editor.Children.Add(Text("How many transfers from the original culture? Use 0 for the original, 1 for its first transfer. Leave blank if unknown.", 12, "#A4B3C9"));
            var condition = new ComboBox { Name = "GeneticsCondition", ItemsSource = new[] { "Not rated", "Strong", "Steady", "Weak", "Needs attention" }, SelectedItem = draft.Condition, MinHeight = 34, Margin = new Thickness(0, 7, 0, 12) };
            var text = new FrameworkElementFactory(typeof(TextBlock)); text.SetValue(TextBlock.ForegroundProperty, Color("#17212E")); text.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(".")); condition.ItemTemplate = new DataTemplate { VisualTree = text };
            Field(editor, "Condition (your assessment)", condition);
            editor.Children.Add(Text("Strong = vigorous growth · Steady = no noticeable change · Weak = reduced growth · Needs attention = something to review. Choose Not rated if unsure.", 12, "#A4B3C9"));
            Field(editor, "Observation notes *", notes);
            editor.Children.Add(Text("Describe appearance, growth or changes you noticed. These are your observations, not an automatic diagnosis.", 12, "#A4B3C9"));
            var feedback = Text("", 13, "#FFBABE"); editor.Children.Add(feedback);
            editor.Children.Add(Button("Save observation", () =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(culture.Text)) throw new ArgumentException("Enter a culture / strain name, or choose one from your family tree.");
                    if (string.IsNullOrWhiteSpace(notes.Text)) throw new ArgumentException("Add a note describing what you observed.");
                    int? number = null;
                    if (!string.IsNullOrWhiteSpace(passage.Text))
                    {
                        if (!int.TryParse(passage.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)) throw new ArgumentException("Transfer number must be a whole number, or left blank.");
                        number = parsed;
                    }
                    draft.Title = culture.Text.Trim(); draft.Details = notes.Text.Trim(); draft.ObservationDate = Date(date); draft.Passage = number; draft.Condition = (string)condition.SelectedItem;
                    store.SaveCultivation(draft); search.Text = ""; Render(); Edit(); Success("Observation saved. Select Edit in your history to make changes.");
                }
                catch (Exception ex) { feedback.Text = "Not saved: " + ex.Message; }
            }, "SaveGenetics", true));
            if (existing != null) editor.Children.Add(Button("Cancel editing / new observation", () => Edit(), "NewGenetics"));
        }
        void Render()
        {
            history.Children.Clear();
            var records = store.Data.Cultivation.Where(x => x.Type == "Genetics" && (x.Title + " " + x.Details + " " + x.Condition).Contains(search.Text.Trim(), StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.ObservationDate ?? DateOnly.FromDateTime(x.Created.LocalDateTime)).ThenByDescending(x => x.Created).ToList();
            history.Children.Add(Text(records.Count + " saved observations", 14, "#89E4E0"));
            if (records.Count == 0) history.Children.Add(Text(search.Text.Length > 0 ? "No matching observations." : "No observations yet. Save your first entry using the form.", 14));
            foreach (var record in records.Take(100))
            {
                var when = record.ObservationDate?.ToString("yyyy-MM-dd") ?? "Earlier note · " + record.Created.ToString("yyyy-MM-dd");
                var card = Stack(Text(record.Title, 18, "#F3F0E7", true), Text(when + " · " + record.Condition + " · " + (record.Passage.HasValue ? "Transfer " + record.Passage : "Transfer not recorded"), 12, "#89E4E0"), Text(record.Details, 14));
                card.Children.Add(Button("Edit observation", () => Edit(record), "EditGenetics" + record.Id.ToString("N")));
                history.Children.Add(Card(card, 14));
            }
            if (records.Count > 100) history.Children.Add(Text("Showing the newest 100 matches. Narrow your search to find older entries.", 12));
        }
        search.TextChanged += (_, _) => Render();
        Page.Children.Add(Columns(Card(editor), Card(Stack(Text("Observation history", 21, "#89E4E0", true), Text("Search culture, condition or notes", 12), search, history))));
        Edit(); Render();
    }
}
