using System.IO.Compression;
using System.Text.Json;
using System.Security.Cryptography;

namespace RoninJournal.Core;

public sealed partial class JournalStore
{
    public void Export(string path)
    {
        path = Path.GetFullPath(path);
        if (Path.GetExtension(path).ToLowerInvariant() != ".zip") throw new ArgumentException("Choose a .zip backup filename.");
        Validate(Data);
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(Data, Json);
        CheckCapacity(Data, jsonBytes.Length);
        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var zip = ZipFile.Open(temp, ZipArchiveMode.Create))
            {
                using (var stream = zip.CreateEntry("journal.json").Open()) stream.Write(jsonBytes);
                foreach (var photo in Data.Photos.DistinctBy(x => x.FileName))
                {
                    var source = Path.Combine(Root, "Photos", photo.FileName);
                    if (!File.Exists(source) || new FileInfo(source).Length != photo.ByteLength || HashFile(source) != photo.Sha256) throw new InvalidDataException("A photo is missing or damaged. Backup was not completed.");
                    zip.CreateEntryFromFile(source, "Photos/" + photo.FileName, CompressionLevel.NoCompression);
                }
            }
            File.Move(temp, path, true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
    public void Restore(string path)
    {
        using var zip = ZipFile.OpenRead(path);
        if (zip.Entries.Count > 10001 || zip.Entries.Sum(x => x.Length) > 2L * 1024 * 1024 * 1024) throw new InvalidDataException("Backup exceeds the supported size (2 GB / 10,000 photos).");
        var names = zip.Entries.Select(x => x.FullName).ToList();
        if (names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Count) throw new InvalidDataException("Duplicate backup files.");
        var dataFile = zip.GetEntry("journal.json") ?? throw new InvalidDataException("Backup has no records file.");
        if (dataFile.Length > 20 * 1024 * 1024) throw new InvalidDataException("Records file exceeds 20 MB.");
        byte[] ReadEntry(ZipArchiveEntry e) { using var input = e.Open(); using var output = new MemoryStream(); input.CopyTo(output); return output.ToArray(); }
        var restored = Read(ReadEntry(dataFile));
        var allowed = restored.Photos.Select(x => "Photos/" + x.FileName).Append("journal.json").ToHashSet(StringComparer.Ordinal);
        if (names.Any(x => !allowed.Contains(x)) || allowed.Any(x => !names.Contains(x))) throw new InvalidDataException("Backup contains unexpected or missing files.");
        foreach (var photo in restored.Photos.DistinctBy(x => x.FileName))
        {
            var entry = zip.GetEntry("Photos/" + photo.FileName)!;
            if (entry.Length > 20 * 1024 * 1024 || entry.Length != photo.ByteLength) throw new InvalidDataException("Photo size does not match the records.");
            var bytes = ReadEntry(entry);
            if (Convert.ToHexStringLower(SHA256.HashData(bytes)) != photo.Sha256 || ImageExtension(bytes) != Path.GetExtension(photo.FileName)) throw new InvalidDataException("Photo integrity check failed.");
        }
        var checkpoint = DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")[..6] + ".zip";
        try { Export(Path.Combine(Root, "Backups", "before-restore-" + checkpoint)); }
        catch (Exception e) when (e is InvalidDataException or FileNotFoundException)
        {
            // Preserve damaged originals for inspection without preventing recovery from a verified backup.
            using var recovery = ZipFile.Open(Path.Combine(Root, "Backups", "recovery-" + checkpoint), ZipArchiveMode.Create);
            using (var stream = recovery.CreateEntry("journal.json").Open()) JsonSerializer.Serialize(stream, Data, Json);
            List<string> missing = [];
            foreach (var old in Data.Photos.DistinctBy(x => x.FileName))
            {
                var file = Path.Combine(Root, "Photos", old.FileName);
                if (File.Exists(file)) recovery.CreateEntryFromFile(file, "Photos/" + old.FileName, CompressionLevel.NoCompression);
                else missing.Add(old.FileName);
            }
            using var info = new StreamWriter(recovery.CreateEntry("RECOVERY-NOTES.txt").Open());
            info.WriteLine("This preserves the damaged pre-restore state for manual recovery. It is not a validated Ronin backup.\nReason: " + e.Message + "\nMissing photos:\n" + string.Join("\n", missing));
        }
        foreach (var photo in restored.Photos.DistinctBy(x => x.FileName))
        {
            var destination = Path.Combine(Root, "Photos", photo.FileName);
            if (!File.Exists(destination) || HashFile(destination) != photo.Sha256)
            {
                var temp = destination + ".restore-tmp";
                using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                { stream.Write(ReadEntry(zip.GetEntry("Photos/" + photo.FileName)!)); stream.Flush(true); }
                File.Move(temp, destination, true);
            }
        }
        Commit(restored);
    }
}
