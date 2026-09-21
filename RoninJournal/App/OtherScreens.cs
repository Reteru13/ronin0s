using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using RoninJournal.Core;

namespace RoninJournal.Desktop;
public partial class MainWindow
{
    private void Library(ReferenceNote? editing = null)
    {
        Page.Children.Clear(); Intro("A personal shelf for business references, packaging notes, supplier details and documentation standards.");
        var list = new StackPanel(); var search = Input("LibrarySearch", "", max:200); Field(list, "Search your reference library", search); var results = new StackPanel(); list.Children.Add(results);
        void Populate()
        {
            results.Children.Clear(); var matches = store.Data.Library.Where(x => (x.Title + " " + x.Category + " " + x.Notes).Contains(search.Text.Trim(), StringComparison.OrdinalIgnoreCase)).OrderBy(x => x.Title).ToList();
            if (matches.Count == 0) results.Children.Add(Card(Stack(Text("Make room for what you learn", 20, "#EDEFE9", true), Text("Save a reference with its source so you can find it again.", 14, "#A4B3C9"))));
            foreach (var note in matches)
            {
                var s = Stack(Text(note.Category.ToUpperInvariant(),10,"#80DDD7"), Text(note.Title,20,"#EDEFE9",true), Text(note.Notes.Length > 220 ? note.Notes[..220] + "…" : note.Notes,14,"#A4B3C9")); s.Children[1].SetValue(MarginProperty,new Thickness(0,10,0,12));
                var b = Button("Read / edit reference  →",()=>Library(Clone(note))); b.Margin = new Thickness(0,16,0,0); s.Children.Add(b); results.Children.Add(Card(s));
            }
        }
        search.TextChanged += (_,_)=>Populate(); Populate();
        var noteEdit = editing ?? new ReferenceNote(); var form = Stack(Text(editing == null ? "New reference" : "Edit reference",21,"#EDEFE9",true)); form.Children[0].SetValue(MarginProperty,new Thickness(0,0,0,20));
        var title = Input("ReferenceTitle",noteEdit.Title); var category=Input("ReferenceCategory",noteEdit.Category,max:80); var source=Input("ReferenceSource",noteEdit.Source,max:2000); var notes=Input("ReferenceNotes",noteEdit.Notes,true,20000); notes.Height=220;
        Field(form,"Title *",title); Field(form,"Category *",category); Field(form,"Source / URL",source); Field(form,"Reference notes",notes);
        form.Children.Add(Button("Save reference",()=>{ noteEdit.Title=title.Text.Trim(); noteEdit.Category=category.Text.Trim(); noteEdit.Source=source.Text.Trim(); noteEdit.Notes=notes.Text; store.SaveReference(noteEdit); Library(); Success("Reference saved locally."); },"SaveReference",true));
        if(editing != null) { var cancel=Button("Cancel editing",()=>Library()); cancel.Margin=new Thickness(0,12,0,0);form.Children.Add(cancel); }
        Page.Children.Add(Columns(list,Card(form)));
    }
    private void Review()
    {
        Page.Children.Clear(); Intro("A measure of your documentation habits. It does not assess product condition, safety or readiness.");
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Margin=new Thickness(0,0,0,18) };
        var prev=Button("← Previous week",()=>{reviewWeek=reviewWeek.AddDays(-7);Review();}); prev.IsEnabled=reviewWeek>Monday(store.Data.Started); actions.Children.Add(prev);
        var range=Text(reviewWeek.ToString("MMM d")+" – "+reviewWeek.AddDays(6).ToString("MMM d, yyyy"),16,"#EDEFE9",true);range.Margin=new Thickness(22,10,22,0);actions.Children.Add(range);
        var next=Button("Next week →",()=>{reviewWeek=reviewWeek.AddDays(7);Review();});next.IsEnabled=reviewWeek<Monday(Today);actions.Children.Add(next); Page.Children.Add(actions);
        var scores=store.Week(Today,reviewWeek); var average=scores.Count==0?0:(int)Math.Round(scores.Average(x=>x.Points));
        var intro=Stack(Text(average+"%",48,"#83E9DC",true),Text("DOCUMENTATION COMPLETENESS",11,"#A5B9D2"),Text(scores.Count+" elapsed day(s) counted. Future days and dates before this installation began are excluded.",14,"#BCCADD"));intro.Children[2].SetValue(MarginProperty,new Thickness(0,14,0,0));Page.Children.Add(Card(intro));
        foreach(var score in scores)
        {
            var row = new Grid(); row.ColumnDefinitions.Add(new(){Width=new GridLength(150)}); row.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)}); row.ColumnDefinitions.Add(new(){Width=new GridLength(90)});
            var label=Stack(Text(score.Date.ToString("dddd"),16,"#EDEFE9",true),Text(score.Date.ToString("MMM d"),12,"#A4B3C9"));row.Children.Add(label);
            var progress = new ProgressBar { Minimum=0,Maximum=100,Value=score.Points,Height=6,Foreground=Color("#72DCD5"),Background=Color("#26364E"),BorderThickness=new Thickness(0),Margin=new Thickness(0,5,0,10) };
            var detail=Stack(progress,Text(!score.Submitted?"Missing or draft · complete the entry":score.Late?"Completed · late documentation recorded":"Completed",12,"#A4B3C9"));Grid.SetColumn(detail,1);row.Children.Add(detail);
            var points=Text(score.Points+" / 100",15,"#B5ECE7",true);points.HorizontalAlignment=HorizontalAlignment.Right;Grid.SetColumn(points,2);row.Children.Add(points);Page.Children.Add(Card(row,18));
        }
        Page.Children.Add(Card(Text("Per completed entry: notes 40 points · inventory review 30 points · captioned, quality-confirmed photo 30 points. Drafts earn zero. Late entries can improve completeness but remain labeled. Honest reports of problems never lose points.",13,"#B4C4D8")));
    }
    private void Backup()
    {
        Page.Children.Clear(); Intro("Your business records belong to you. Keep an exported copy on a separate drive or in your own backup service.");
        var export=Stack(Text("Take your records with you",24,"#EDEFE9",true),Text("One ZIP file contains your inventory, journal, reference library and all attached photos. Images are checked before the export is completed.",14,"#B0C1D5")); export.Children[1].SetValue(MarginProperty,new Thickness(0,16,0,22));
        export.Children.Add(Button("Export complete backup",()=>{ var d=new SaveFileDialog {Filter="Ronin backup (*.zip)|*.zip",FileName="Ronin-backup-"+DateTime.Now.ToString("yyyy-MM-dd-HHmm")+".zip",DefaultExt=".zip",AddExtension=true};if(d.ShowDialog(this)==true){store.Export(d.FileName);Success("Complete backup saved: "+d.FileName);}},"ExportBackup",true));
        var restore=Stack(Text("Restore a backup",24,"#EDEFE9",true),Text("Restoring replaces the active records. The app validates the archive first and saves a full rollback backup before applying it.",14,"#B0C1D5"));restore.Children[1].SetValue(MarginProperty,new Thickness(0,16,0,22));
        restore.Children.Add(Button("Choose backup to restore",()=>{var d=new OpenFileDialog{Filter="Ronin backup (*.zip)|*.zip"};if(d.ShowDialog(this)!=true)return;if(MessageBox.Show(this,"Replace the current records with this backup? A rollback ZIP will be created first.\n\n"+d.FileName,"Restore records",MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;store.Restore(d.FileName);reviewWeek=Monday(Today);Backup();Success("Backup restored. Previous records are preserved in Data/Backups.");},"RestoreBackup"));
        Page.Children.Add(Columns(Card(export),Card(restore)));
        var details=Stack(Text("Local by design",20,"#EDEFE9",true),Text("Data folder",12,"#8BDED9"),Text(store.Root,14,"#BBCBDE"),Text("No accounts or cloud synchronization. Local files are not encrypted. Closing the app before copying its full folder avoids incomplete copies.",14,"#A4B3C9"),Text("Sharing the application",18,"#EDEFE9",true),Text("Give another person only a clean application package. Never send your Data folder unless you intend to share every record and photo.",14,"#A4B3C9"));
        for(int i=1;i<details.Children.Count;i++)details.Children[i].SetValue(MarginProperty,new Thickness(0,14,0,0));Page.Children.Add(Card(details));
    }
}
