using RoninJournal.Core;
using System.IO.Compression;

var root = Path.Combine(Path.GetTempPath(), "RoninChecks-" + Guid.NewGuid());
Directory.CreateDirectory(root);
int failures = 0, passed = 0;
void Check(string title, Action action) { try { action(); Console.WriteLine("PASS " + title); passed++; } catch (Exception e) { Console.WriteLine("FAIL " + title + ": " + e.Message); failures++; } }
void Assert(bool value, string message) { if (!value) throw new Exception(message); }
void Reject(Action action) { try { action(); } catch (Exception e) when (e is InvalidDataException or IOException or ArgumentException) { return; } throw new Exception("Expected rejection"); }
string Place() => Path.Combine(root, Guid.NewGuid().ToString());
var day = new DateOnly(2026, 9, 10);
var png = Path.Combine(root, "photo.png");
File.WriteAllBytes(png, Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aWQAAAABJRU5ErkJggg=="));

Check("stock survives reopen and keeps one record after update", () => {
    var p = Place(); var item = new StockItem { Name = "Shipping boxes", Quantity = 12 };
    using (var s = new JournalStore(p)) { s.UpsertStock(item); item.Quantity = 8; s.UpsertStock(item); }
    using var read = new JournalStore(p); Assert(read.Data.Inventory.Count == 1 && read.Data.Inventory[0].Quantity == 8, "Persistence/update failed");
});
Check("negative stock and blank names are rejected", () => {
    using var s = new JournalStore(Place()); Reject(() => s.UpsertStock(new() { Name = "Boxes", Quantity = -1 })); Reject(() => s.UpsertStock(new()));
});
Check("archiving retains record", () => { using var s = new JournalStore(Place()); var item = new StockItem { Name = "Box", Archived = true }; s.UpsertStock(item); Assert(s.Data.Inventory.Single().Archived, "Archive lost"); });
Check("one writer only", () => { var p = Place(); using var s = new JournalStore(p); Reject(() => { using var other = new JournalStore(p); }); });
Check("draft earns zero; submitted note earns 40; future days excluded", () => {
    using var s = new JournalStore(Place()); s.Data.Started = day.AddDays(-1);
    var entry = new DayEntry { Date = day, Notes = "Business records reviewed today." }; s.SaveEntry(entry);
    Assert(s.Week(day).Count == 2 && s.Week(day)[1].Points == 0, "Draft or denominator wrong");
    entry.Submitted = true; s.SaveEntry(entry); Assert(s.Week(day)[1].Points == 40 && s.Week(day)[0].Points == 0, "Missing day or notes wrong");
});
Check("completed record scores 100 with late flag and managed photo", () => {
    using var s = new JournalStore(Place()); s.Data.Started = day;
    var photo = s.AddPhoto(png, day, "Delivery receipt", true);
    s.SaveEntry(new() { Date = day, Notes = "Recorded the delivery.", Submitted = true, InventoryReviewed = true, SubmittedAt = new DateTimeOffset(2026, 9, 11, 10, 0, 0, TimeSpan.Zero) });
    Assert(s.Week(day)[0].Points == 100 && s.Week(day)[0].Late, "Completion score or late flag wrong");
    Assert(File.Exists(Path.Combine(s.Root, "Photos", photo.FileName)), "Photo not copied");
});
Check("unconfirmed photo cannot earn photo points", () => {
    using var s = new JournalStore(Place()); s.Data.Started = day; s.AddPhoto(png, day, "Receipt", false);
    s.SaveEntry(new() { Date = day, Notes = "Checked the records", Submitted = true, InventoryReviewed = true });
    Assert(s.Week(day)[0].Points == 70, "Unconfirmed photo counted");
});
Check("invalid photo or caption rejected", () => {
    using var s = new JournalStore(Place()); Reject(() => s.AddPhoto(png, day, " ", true));
    var fake = Path.Combine(root, "fake.png"); File.WriteAllText(fake, "not a picture"); Reject(() => s.AddPhoto(fake, day, "Fake", true));
});
Check("backup round trip includes records, photos, references and rollback checkpoint", () => {
    using var s = new JournalStore(Place()); s.Data.Started = day;
    s.UpsertStock(new() { Name = "Boxes", Quantity = 4 }); var photo = s.AddPhoto(png, day, "Receipt", true);
    s.SaveReference(new() { Title = "Packing reference", Notes = "Our business reference" });
    var zip = Path.Combine(root, "export.zip"); s.Export(zip);
    s.UpsertStock(new() { Name = "Labels", Quantity = 5 }); s.Restore(zip);
    Assert(s.Data.Inventory.Count == 1 && s.Data.Library.Count == 1 && s.Data.Photos.Count == 1, "Records not restored");
    Assert(File.Exists(Path.Combine(s.Root, "Photos", photo.FileName)), "Missing restored image");
    Assert(Directory.GetFiles(Path.Combine(s.Root, "Backups"), "*.zip").Length > 0, "No rollback archive");
});
Check("unsafe or corrupt backup leaves current data intact", () => {
    using var s = new JournalStore(Place()); s.UpsertStock(new() { Name = "Keep me" });
    var bad = Path.Combine(root, "unsafe.zip"); using (var z = ZipFile.Open(bad, ZipArchiveMode.Create)) { using var w = new StreamWriter(z.CreateEntry("../outside.txt").Open()); w.Write("bad"); }
    Reject(() => s.Restore(bad)); Assert(s.Data.Inventory.Single().Name == "Keep me", "Unsafe restore changed data");
    var valid = Path.Combine(root, "tamper.zip"); s.AddPhoto(png, day, "Receipt", true); s.Export(valid);
    using (var z = ZipFile.Open(valid, ZipArchiveMode.Update)) { var e = z.Entries.First(x => x.FullName.StartsWith("Photos/")); var name = e.FullName; e.Delete(); using var w = new StreamWriter(z.CreateEntry(name).Open()); w.Write("tampered"); }
    Reject(() => s.Restore(valid)); Assert(s.Data.Inventory.Single().Name == "Keep me", "Corrupt restore changed data");
});
Check("corrupt local database is not silently replaced", () => { var p = Place(); Directory.CreateDirectory(p); File.WriteAllText(Path.Combine(p, "journal.json"), "broken"); Reject(() => { using var s = new JournalStore(p); }); Assert(File.ReadAllText(Path.Combine(p,"journal.json")) == "broken", "Corrupt file overwritten"); });
Check("incomplete backup schema cannot erase existing records", () => {
    using var s = new JournalStore(Place()); s.UpsertStock(new() { Name = "Preserve" });
    var bad = Path.Combine(root,"empty-schema.zip"); using(var z=ZipFile.Open(bad,ZipArchiveMode.Create)){using var w=new StreamWriter(z.CreateEntry("journal.json").Open());w.Write("{}");}
    Reject(()=>s.Restore(bad)); Assert(s.Data.Inventory.Single().Name=="Preserve","Incomplete schema erased data");
});
Check("photos can be corrected without losing their original reference", () => {
    using var s=new JournalStore(Place()); var photo=s.AddPhoto(png,day,"Receipt",false);
    photo.Caption="Corrected receipt label";photo.QualityConfirmed=true;s.UpdatePhoto(photo);
    Assert(s.Data.Photos.Count==1 && s.Data.Photos[0].QualityConfirmed && s.Data.Photos[0].FileName==photo.FileName,"Photo edit lost metadata");
});
Check("export rejects records above restore capacity", () => {
    using var s=new JournalStore(Place());s.Data.Library=Enumerable.Range(0,1100).Select(i=>new ReferenceNote{Title="Reference "+i,Notes=new string('x',20000)}).ToList();
    Reject(()=>s.Export(Path.Combine(root,"oversized.zip")));
});
Check("valid backup recovers missing and corrupted local photos", () => {
    using var s=new JournalStore(Place());var photo=s.AddPhoto(png,day,"Keep receipt",true);var backup=Path.Combine(root,"recover.zip");s.Export(backup);
    var local=Path.Combine(s.Root,"Photos",photo.FileName);File.Delete(local);s.Restore(backup);Assert(File.Exists(local),"Missing photo was not recovered");
    File.WriteAllText(local,"damaged original");s.Restore(backup);Assert(File.ReadAllBytes(local).SequenceEqual(File.ReadAllBytes(png)),"Corrupt photo not recovered");
    var recovery=Directory.GetFiles(Path.Combine(s.Root,"Backups"),"recovery-*.zip");Assert(recovery.Length==2,"Damaged originals were not preserved separately");
});
Check("unchanged photo confirmation preserves review time", () => {
    using var s=new JournalStore(Place());var photo=s.AddPhoto(png,day,"Receipt",true);var before=photo.ReviewedAt;Thread.Sleep(20);s.UpdatePhoto(photo);Assert(s.Data.Photos.Single().ReviewedAt==before,"Unchanged confirmation changed its timestamp");
});
Check("missing photo does not prevent unrelated journal saves", () => {
    using var s=new JournalStore(Place());var photo=s.AddPhoto(png,day,"Receipt",true);File.Delete(Path.Combine(s.Root,"Photos",photo.FileName));
    s.SaveEntry(new(){Date=day,Notes="Keep these new notes"});Assert(s.Data.Entries.Single().Notes=="Keep these new notes","Missing photo blocked the draft");
});
Check("culture cross ancestry survives edits, reopen and backup restore", () => {
    var p = Place(); var first = new CultureRecord { Name = "Oyster master", Species = "Oyster", Date = day };
    var second = new CultureRecord { Name = "Second source", Species = "Oyster", Date = day };
    var child = new CultureRecord { Name = "Cross", Species = "Oyster", Date = day, Method = "Cross", ParentIds = [first.Id, second.Id] };
    var backup = Path.Combine(root, "cultures.zip");
    using (var s = new JournalStore(p)) { s.SaveCulture(first); s.SaveCulture(second); s.SaveCulture(child); first.Name = "Renamed source"; s.SaveCulture(first); s.Export(backup); }
    using var read = new JournalStore(p);
    Assert(read.Data.Cultures.Count == 3 && read.Data.Cultures.Single(x => x.Id == first.Id).Name == "Renamed source", "Culture edit lost on reopen");
    Assert(read.Data.Cultures.Single(x => x.Id == child.Id).ParentIds.SequenceEqual(new[] { first.Id, second.Id }), "Cross parents lost");
    using var restored = new JournalStore(Place()); restored.Restore(backup);
    Assert(restored.Data.Cultures.Single(x => x.Id == child.Id).ParentIds.Count == 2, "Backup lost ancestry");
});
Check("culture cycles, missing parents, duplicate parents and invalid dates leave saved data intact", () => {
    using var s = new JournalStore(Place());
    var first = new CultureRecord { Name = "Source", Species = "Oyster", Date = day }; s.SaveCulture(first);
    var child = new CultureRecord { Name = "Child", Species = "Oyster", Date = day, Method = "Transfer", ParentIds = [first.Id] }; s.SaveCulture(child);
    first.Method = "Transfer"; first.ParentIds = [child.Id]; Reject(() => s.SaveCulture(first));
    child.ParentIds = [Guid.NewGuid()]; Reject(() => s.SaveCulture(child));
    child.ParentIds = [child.Id]; Reject(() => s.SaveCulture(child));
    child.Method = "Cross"; child.ParentIds = [first.Id, first.Id]; Reject(() => s.SaveCulture(child));
    child.Method = "Transfer"; child.ParentIds = [first.Id]; child.Date = day.AddDays(-1); Reject(() => s.SaveCulture(child));
    Reject(() => s.SaveCulture(new()));
    Assert(s.Data.Cultures.Count == 2 && s.Data.Cultures.Single(x => x.Id == first.Id).ParentIds.Count == 0, "Rejected save mutated records");
});
Check("genetics observations preserve metadata, edits and earlier notes across reopen and backup", () => {
    var p = Place(); var id = Guid.NewGuid(); var backup = Path.Combine(root, "observations.zip");
    using (var s = new JournalStore(p)) {
        s.Data.Cultivation.Add(new() { Type = "Genetics", Title = "Earlier culture", Details = "Existing note" }); s.Save();
        s.SaveCultivation(new() { Id = id, Type = "Genetics", Title = "Oyster", Details = "Even growth", ObservationDate = day, Passage = 2, Condition = "Steady" });
        s.SaveCultivation(new() { Id = id, Type = "Genetics", Title = "Oyster", Details = "Updated observation", ObservationDate = day, Passage = 3, Condition = "Strong" }); s.Export(backup);
    }
    using var read = new JournalStore(p); var record = read.Data.Cultivation.Single(x => x.Id == id);
    Assert(read.Data.Cultivation.Count == 2 && record.Passage == 3 && record.Condition == "Strong" && record.Details == "Updated observation", "Edit or legacy record lost");
    using var restored = new JournalStore(Place()); restored.Restore(backup);
    Assert(restored.Data.Cultivation.Single(x=>x.Id==id).ObservationDate==day, "Observation date lost in backup");
});
Check("invalid genetics input does not create an observation", () => {
    using var s = new JournalStore(Place());
    var record = new CultivationRecord { Type = "Genetics", Title = "Oyster", Details = "Note", ObservationDate = day, Passage = -1 };
    Reject(()=>s.SaveCultivation(record)); record.Passage = 10000; Reject(()=>s.SaveCultivation(record));
    record.Passage = null; record.ObservationDate = DateOnly.FromDateTime(DateTime.Today).AddDays(1); Reject(()=>s.SaveCultivation(record));
    record.ObservationDate = day; record.Details = " "; Reject(()=>s.SaveCultivation(record));
    Assert(s.Data.Cultivation.Count==0,"Rejected entry was saved");
});
Console.WriteLine($"{passed} passed; {failures} failed. Fixtures: {root}");
return failures == 0 ? 0 : 1;
