using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RoninJournal.Core;

public sealed partial class JournalStore : IDisposable
{
    internal static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private readonly FileStream writerLock;
    public JournalData Data { get; private set; } = new();
    public string Root { get; }
    private string Database => Path.Combine(Root, "journal.json");
    public JournalStore(string root)
    {
        Root = Path.GetFullPath(root);
        Directory.CreateDirectory(Root);
        writerLock = new FileStream(Path.Combine(Root, ".writer.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        try
        {
            Directory.CreateDirectory(Path.Combine(Root, "Photos"));
            Directory.CreateDirectory(Path.Combine(Root, "Backups"));
            if (File.Exists(Database)) Data = Read(File.ReadAllBytes(Database));
            else Save();
        }
        catch { writerLock.Dispose(); throw; }
    }
    internal static JournalData Read(byte[] bytes)
    {
        if (bytes.Length > 20 * 1024 * 1024) throw new InvalidDataException("Records exceed the supported 20 MB limit.");
        try
        {
            using var document = JsonDocument.Parse(bytes);
            string[] required = ["Version", "Started", "Inventory", "Entries", "Photos", "Library"];
            if (document.RootElement.ValueKind != JsonValueKind.Object || required.Any(key => !document.RootElement.TryGetProperty(key, out _))) throw new InvalidDataException("Records file is incomplete; no changes were made.");
            var data = JsonSerializer.Deserialize<JournalData>(bytes, Json) ?? throw new InvalidDataException("Empty data file.");
            Validate(data); return data;
        }
        catch (JsonException e) { throw new InvalidDataException("The records file is damaged. Keep it and restore a verified backup.", e); }
    }
    private JournalData Copy() => Read(JsonSerializer.SerializeToUtf8Bytes(Data, Json));
    public void Save() => Commit(Data);
    private void Commit(JournalData candidate)
    {
        Validate(candidate);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(candidate, Json);
        CheckCapacity(candidate, bytes.Length);
        var temp = Database + ".tmp";
        using (var f = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
        { f.Write(bytes); f.Flush(true); }
        if (File.Exists(Database)) File.Replace(temp, Database, Database + ".previous", true);
        else File.Move(temp, Database);
        // Detach caller-owned editor objects so later edits cannot mutate committed state.
        Data = Read(bytes);
    }
    private void CheckCapacity(JournalData candidate, int jsonLength)
    {
        if (jsonLength > 20 * 1024 * 1024) throw new InvalidDataException("The records limit is 20 MB. This change was not saved.");
        long size = jsonLength;
        foreach (var photo in candidate.Photos.DistinctBy(x => x.FileName)) size += photo.ByteLength;
        if (size > 2L * 1024 * 1024 * 1024) throw new InvalidDataException("This version supports 2 GB of records and photos. The change was not saved; your existing backup remains exportable.");
    }
    public void UpsertStock(StockItem item)
    {
        var copy = Copy(); copy.Inventory.RemoveAll(x => x.Id == item.Id); copy.Inventory.Add(item); Commit(copy);
    }
    public void SaveEntry(DayEntry entry)
    {
        if (entry.Date > DateOnly.FromDateTime(DateTime.Today)) throw new ArgumentException("Choose today or an earlier date.");
        var copy = Copy(); var previous = copy.Entries.Find(x => x.Date == entry.Date);
        entry.FirstSaved = previous?.FirstSaved ?? DateTimeOffset.Now;
        entry.Updated = DateTimeOffset.Now;
        entry.SubmittedAt = entry.Submitted ? previous?.SubmittedAt ?? entry.SubmittedAt ?? DateTimeOffset.Now : null;
        copy.Entries.RemoveAll(x => x.Date == entry.Date); copy.Entries.Add(entry); Commit(copy);
    }
    public PhotoRecord AddPhoto(string file, DateOnly date, string caption, bool confirmed)
    {
        Text(caption, 500, true);
        if (date > DateOnly.FromDateTime(DateTime.Today)) throw new ArgumentException("Photos cannot be dated in the future.");
        var info = new FileInfo(file);
        if (info.Length > 20 * 1024 * 1024) throw new ArgumentException("Choose a PNG or JPEG of 20 MB or less.");
        var bytes = File.ReadAllBytes(file);
        var ext = ImageExtension(bytes);
        var hash = Convert.ToHexStringLower(SHA256.HashData(bytes));
        var photo = new PhotoRecord { Date = date, Caption = caption.Trim(), QualityConfirmed = confirmed, ReviewedAt = confirmed ? DateTimeOffset.Now : null, Sha256 = hash, FileName = hash + ext, ByteLength = bytes.LongLength };
        var dest = Path.Combine(Root, "Photos", photo.FileName);
        if (!File.Exists(dest)) File.WriteAllBytes(dest, bytes);
        else if (HashFile(dest) != hash) throw new InvalidDataException("An existing photo is damaged; restore a backup first.");
        var copy = Copy(); copy.Photos.Add(photo); Commit(copy); return photo;
    }
    internal static string ImageExtension(byte[] bytes)
    {
        if (bytes.Length >= 24 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10})) return ".png";
        if (bytes.Length >= 4 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255) return ".jpg";
        throw new InvalidDataException("This file is not a recognized PNG or JPEG image.");
    }
    internal static string HashFile(string path) { using var stream = File.OpenRead(path); return Convert.ToHexStringLower(SHA256.HashData(stream)); }
    public void SaveReference(ReferenceNote note)
    {
        var copy = Copy(); copy.Library.RemoveAll(x => x.Id == note.Id); copy.Library.Add(note); Commit(copy);
    }
    public void SaveNotebookTitle(string title)
    {
        Text(title, 160, true);
        var copy = Copy(); copy.NotebookTitle = title.Trim(); Commit(copy);
    }
    public void SaveCultivation(CultivationRecord record)
    {
        if (record.Type == "Genetics")
        {
            Text(record.Title, 120, true); Text(record.Details, 20000, true);
            if (record.ObservationDate == null || record.ObservationDate == default(DateOnly) || record.ObservationDate > DateOnly.FromDateTime(DateTime.Today)) throw new ArgumentException("Choose an observation date today or earlier.");
            if (record.Passage < 0 || record.Passage > 9999) throw new ArgumentException("Transfer number must be a whole number from 0 to 9999, or left blank.");
            if (!new[] { "Not rated", "Strong", "Steady", "Weak", "Needs attention" }.Contains(record.Condition)) throw new ArgumentException("Choose a condition.");
        }
        if (string.IsNullOrWhiteSpace(record.Type) || string.IsNullOrWhiteSpace(record.Title) || string.IsNullOrWhiteSpace(record.Details))
            throw new ArgumentException("A cultivation record needs a type, title, and details.");
        var copy = Copy(); copy.Cultivation.RemoveAll(x => x.Id == record.Id); copy.Cultivation.Add(record); Commit(copy);
    }
    public void SaveCulture(CultureRecord record)
    {
        var copy = Copy(); copy.Cultures.RemoveAll(x => x.Id == record.Id); copy.Cultures.Add(record); Commit(copy);
    }
    public void UpdatePhoto(PhotoRecord photo)
    {
        var copy = Copy(); var old = copy.Photos.Find(x => x.Id == photo.Id) ?? throw new ArgumentException("Photo record not found.");
        old.Caption = photo.Caption.Trim();
        old.ReviewedAt = photo.QualityConfirmed ? (old.QualityConfirmed ? old.ReviewedAt ?? old.Added : DateTimeOffset.Now) : null;
        old.QualityConfirmed = photo.QualityConfirmed;
        Commit(copy);
    }
    public IReadOnlyList<DayScore> Week(DateOnly today, DateOnly? weekStart = null)
    {
        var start = weekStart ?? today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        List<DayScore> results = [];
        for (int i = 0; i < 7; i++)
        {
            var date = start.AddDays(i); if (date > today || date < Data.Started) continue;
            var entry = Data.Entries.Find(x => x.Date == date);
            var photo = Data.Photos.Where(x => x.Date == date && x.QualityConfirmed && !string.IsNullOrWhiteSpace(x.Caption)).OrderBy(x => x.Added).FirstOrDefault();
            bool submitted = entry?.Submitted == true;
            int points = submitted ? (!string.IsNullOrWhiteSpace(entry!.Notes) ? 40 : 0) + (entry.InventoryReviewed ? 30 : 0) + (photo != null ? 30 : 0) : 0;
            bool late = submitted && (DateOnly.FromDateTime(entry!.SubmittedAt!.Value.Date) > date || (photo != null && DateOnly.FromDateTime((photo.ReviewedAt ?? photo.Added).Date) > date));
            results.Add(new(date, points, submitted, late, photo != null));
        }
        return results;
    }
    private static void Text(string? text, int max, bool required = false)
    { if (text == null || text.Length > max || (required && string.IsNullOrWhiteSpace(text))) throw new InvalidDataException(required ? "Required text is missing or too long." : "Text is missing or too long."); }
    internal static void Validate(JournalData d)
    {
        d.Cultivation ??= [];
        ValidateCultures(d.Cultures);
        if (d.Version != 1 || d.Inventory == null || d.Entries == null || d.Photos == null || d.Library == null || d.Started == default) throw new InvalidDataException("Unsupported or incomplete records file.");
        Text(d.NotebookTitle ?? "Ronin operations notebook", 160, true);
        if (d.Inventory.Count > 10000 || d.Entries.Count > 30000 || d.Photos.Count > 10000 || d.Library.Count > 10000) throw new InvalidDataException("Records limit exceeded.");
        foreach (var x in d.Inventory)
        {
            if (x == null) throw new InvalidDataException("Invalid inventory record.");
            Text(x.Name, 120, true); Text(x.Category, 80, true); Text(x.Unit, 40, true); Text(x.Location, 160); Text(x.Notes, 20000);
            if (x.Quantity < 0 || x.ReorderAt < 0) throw new InvalidDataException("Quantities must be zero or greater.");
        }
        foreach (var x in d.Entries)
        {
            if (x == null || x.Date == default || (x.Submitted && x.SubmittedAt == null)) throw new InvalidDataException("Invalid journal entry.");
            Text(x.Notes, 20000, x.Submitted);
        }
        foreach (var x in d.Photos)
        {
            if (x == null || x.Date == default || x.ByteLength <= 0 || x.ByteLength > 20 * 1024 * 1024 || x.FileName == null || !Regex.IsMatch(x.FileName, "^[a-f0-9]{64}\\.(png|jpg)$") || x.Sha256 != Path.GetFileNameWithoutExtension(x.FileName)) throw new InvalidDataException("Invalid photo reference.");
            Text(x.Caption, 500, true);
        }
        foreach (var x in d.Library) { if (x == null) throw new InvalidDataException("Invalid reference."); Text(x.Title, 120, true); Text(x.Category, 80, true); Text(x.Source, 2000); Text(x.Notes, 20000); }
        if (d.Photos.GroupBy(x => x.FileName).Any(g => g.Select(x => x.ByteLength).Distinct().Count() != 1)) throw new InvalidDataException("Conflicting photo sizes.");
        if (d.Inventory.Select(x => x.Id).Distinct().Count() != d.Inventory.Count || d.Entries.Select(x => x.Date).Distinct().Count() != d.Entries.Count || d.Photos.Select(x => x.Id).Distinct().Count() != d.Photos.Count || d.Library.Select(x => x.Id).Distinct().Count() != d.Library.Count) throw new InvalidDataException("Duplicate records in data file.");
    }
    public void Dispose() => writerLock.Dispose();
}
