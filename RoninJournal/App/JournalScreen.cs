using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using RoninJournal.Core;

namespace RoninJournal.Desktop;
public partial class MainWindow
{
    private void Journal(DateOnly date)
    {
        Page.Children.Clear(); Intro("Capture the day honestly. Drafts save automatically; choose Complete entry when you are ready.");
        var existing = store.Data.Entries.Find(x => x.Date == date);
        var entry = existing == null ? new DayEntry { Date = date } : Clone(existing);
        var form = new StackPanel(); var dateInput = Input("JournalDate", date.ToString("yyyy-MM-dd"));
        Field(form, "Record date · YYYY-MM-DD", dateInput);
        var load = Button("Open this date", () => { var next = Date(dateInput); if (next > Today) throw new ArgumentException("Choose today or an earlier date."); draftTimer.Stop(); flushDraft?.Invoke(); flushDraft = null; Journal(next); }, "LoadJournalDate"); load.Margin = new Thickness(0,0,0,22); form.Children.Add(load);
        var notes = Input("JournalNotes", entry.Notes, true, 20000); notes.Height = 200;
        Field(form, "What should you remember about today? *", notes);
        var check = new CheckBox { Name = "InventoryReviewed", Content = "I reviewed my inventory records for this date", IsChecked = entry.InventoryReviewed }; form.Children.Add(check);
        var state = Text(existing?.Submitted == true ? "Entry completed · editing creates a new draft" : "A quiet space for your daily notes", 12, "#A4B3C9"); state.Margin = new Thickness(0, 10, 0, 18); form.Children.Add(state);
        bool dirty = false;
        void SaveDraft()
        {
            if (!dirty) return;
            entry.Notes = notes.Text; entry.InventoryReviewed = check.IsChecked == true; entry.Submitted = false;
            store.SaveEntry(Clone(entry)); dirty = false; state.Text = "Draft saved · complete entry to include it in your score"; Success("Draft saved locally at " + DateTime.Now.ToString("t"));
        }
        flushDraft = SaveDraft;
        void Changed() { dirty = true; draftTimer.Stop(); draftTimer.Start(); }
        notes.TextChanged += (_, _) => Changed(); check.Checked += (_, _) => Changed(); check.Unchecked += (_, _) => Changed();
        form.Children.Add(Button("Complete entry  ✓", () => {
            if (Date(dateInput) != date) throw new ArgumentException("Click Open this date before completing a record for a different date. Your current notes still belong to " + date.ToString("yyyy-MM-dd") + ".");
            draftTimer.Stop(); entry.Notes = notes.Text.Trim(); entry.InventoryReviewed = check.IsChecked == true; entry.Submitted = true;
            store.SaveEntry(Clone(entry)); dirty = false; state.Text = "Entry completed · " + DateTime.Now.ToString("g"); Success("Daily entry completed and saved.");
        }, "CompleteEntry", true));
        var side = Stack(Text("A record worth keeping", 20, "#EDEFE9", true), Text("Write the facts, changes, questions and follow-ups you want to remember. Problems are part of an honest record.", 14, "#B0C1D5")); side.Children[1].SetValue(MarginProperty, new Thickness(0,14,0,20));
        side.Children.Add(Text("DOCUMENTATION POINTS", 11, "#83DED9", true));
        foreach (var line in new[] { "40  ·  Written and completed journal", "30  ·  Inventory review confirmed", "30  ·  Captioned, checked photo" }) { var t = Text(line, 13); t.Margin = new Thickness(0, 12, 0, 0); side.Children.Add(t); }
        var attach = Button("Add a photo  →", () => { draftTimer.Stop(); flushDraft?.Invoke(); flushDraft = null; Navigate("Photos"); Photos(date); }, "JournalAddPhoto"); attach.Margin = new Thickness(0,24,0,20); side.Children.Add(attach);
        int count = store.Data.Photos.Count(x => x.Date == date); side.Children.Add(Text(count + " photo record(s) for " + date.ToString("MMM d"), 13, "#A4B3C9"));
        var history = Stack(Text("Recent entries", 18, "#EDEFE9", true));
        foreach (var old in store.Data.Entries.OrderByDescending(x => x.Date).Take(10))
        {
            var b = Button(old.Date.ToString("MMM d, yyyy") + (old.Submitted ? "  ·  completed" : "  ·  draft"), () => { draftTimer.Stop(); flushDraft?.Invoke(); flushDraft = null; Journal(old.Date); }); b.Margin = new Thickness(0,10,0,0); history.Children.Add(b);
        }
        Page.Children.Add(Columns(Card(form), Stack(Card(side), Card(history)), 1.5, 1));
    }
    private void Photos(DateOnly? selected = null, PhotoRecord? editing = null)
    {
        Page.Children.Clear(); Intro("Original image copies stay in your local data folder. PNG or JPEG · up to 20 MB each.");
        var form = Stack(Text(editing == null ? "Add a photo record" : "Edit photo details", 21, "#EDEFE9", true)); form.Children[0].SetValue(MarginProperty, new Thickness(0,0,0,20));
        var date = Input("PhotoDate", (editing?.Date ?? selected ?? Today).ToString("yyyy-MM-dd")); date.IsReadOnly = editing != null; var caption = Input("PhotoCaption", editing?.Caption ?? "", true, 500); caption.Height = 85;
        Field(form, "Record date · YYYY-MM-DD", date); Field(form, "Caption / identifying label *", caption);
        var quality = new CheckBox { Name = "PhotoQuality", IsChecked = editing?.QualityConfirmed == true, Content = new TextBlock { Text = "I checked that the subject is identifiable, framing is clear and lighting is readable.", TextWrapping = TextWrapping.Wrap } }; form.Children.Add(quality);
        form.Children.Add(Text("This confirmation is yours; the app does not assess sharpness or image content. Unconfirmed photos are saved but do not earn photo points.", 12, "#A4B3C9"));
        var add = Button(editing == null ? "Choose photo & save" : "Save photo details", () => {
            var recordDate = Date(date); if (recordDate > Today) throw new ArgumentException("Choose today or an earlier date.");
            if (string.IsNullOrWhiteSpace(caption.Text)) throw new ArgumentException("Write a caption before choosing your photo.");
            if (editing != null) { var edit = Clone(editing); edit.Caption = caption.Text; edit.QualityConfirmed = quality.IsChecked == true; store.UpdatePhoto(edit); Photos(recordDate); Success("Photo details updated."); return; }
            var dialog = new OpenFileDialog { Filter = "Photos (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg", CheckFileExists = true };
            if (dialog.ShowDialog(this) != true) return;
            if (new FileInfo(dialog.FileName).Length > 20 * 1024 * 1024) throw new ArgumentException("Choose an image of 20 MB or less.");
            _ = Bitmap(dialog.FileName, 640);
            store.AddPhoto(dialog.FileName, recordDate, caption.Text, quality.IsChecked == true); Photos(recordDate); Success("Photo and caption saved locally.");
        }, "AddPhoto", true); add.Margin = new Thickness(0,20,0,0); form.Children.Add(add);
        if (editing != null) { var cancel = Button("Cancel editing", () => Photos(selected)); cancel.Margin = new Thickness(0,12,0,0); form.Children.Add(cancel); }
        var gallery = new StackPanel(); var search = Input("PhotoSearch", "", max:500); Field(gallery, "Search captions or dates", search); var results = new StackPanel(); gallery.Children.Add(results);
        void Populate()
        {
            results.Children.Clear(); var photos = store.Data.Photos.Where(x => (x.Caption + " " + x.Date.ToString("yyyy-MM-dd")).Contains(search.Text.Trim(), StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.Date).ThenByDescending(x => x.Added).ToList();
            if (photos.Count == 0) results.Children.Add(Card(Stack(Text("Your visual history starts here", 20, "#EDEFE9", true), Text("Attach a photo to preserve the details that words might miss.",14,"#A4B3C9"))));
            foreach (var photo in photos.Take(100))
            {
                var s = new StackPanel();
                try { s.Children.Add(new Image { Source = Bitmap(Path.Combine(store.Root, "Photos", photo.FileName), 600), Height = 185, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0,0,0,14) }); }
                catch { s.Children.Add(Text("Image unavailable · check your backup", 13, "#FFB8B8")); }
                s.Children.Add(Text(photo.Caption, 16, "#EDEFE9", true)); s.Children.Add(Text(photo.Date.ToString("MMM d, yyyy") + (photo.QualityConfirmed ? "  ·  quality confirmed by you" : "  ·  quality not confirmed"), 11, "#A4B3C9"));
                var editButton = Button("Edit caption / confirmation", () => Photos(photo.Date, Clone(photo))); editButton.Margin = new Thickness(0,14,0,0); s.Children.Add(editButton); results.Children.Add(Card(s));
            }
            if (photos.Count > 100) results.Children.Add(Text("Showing the latest 100 matches. Narrow your search to see older records.", 12, "#A4B3C9"));
        }
        search.TextChanged += (_,_) => Populate(); Populate(); Page.Children.Add(Columns(gallery, Card(form), 1.2, 1));
    }
}
