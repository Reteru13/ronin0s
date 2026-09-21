namespace RoninJournal.Core;

public sealed class JournalData
{
    public int Version { get; set; } = 1;
    public DateOnly Started { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string NotebookTitle { get; set; } = "Ronin operations notebook";
    public List<StockItem> Inventory { get; set; } = [];
    public List<DayEntry> Entries { get; set; } = [];
    public List<PhotoRecord> Photos { get; set; } = [];
    public List<ReferenceNote> Library { get; set; } = [];
    public List<CultivationRecord> Cultivation { get; set; } = [];
    public List<CultureRecord> Cultures { get; set; } = [];
}
public sealed class StockItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Category { get; set; } = "Supplies";
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "each";
    public decimal ReorderAt { get; set; }
    public string Location { get; set; } = "";
    public string Notes { get; set; } = "";
    public bool Archived { get; set; }
}
public sealed class DayEntry
{
    public DateOnly Date { get; set; }
    public string Notes { get; set; } = "";
    public bool InventoryReviewed { get; set; }
    public bool Submitted { get; set; }
    public DateTimeOffset FirstSaved { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset Updated { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? SubmittedAt { get; set; }
}
public sealed class PhotoRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly Date { get; set; }
    public string FileName { get; set; } = "";
    public string Caption { get; set; } = "";
    public bool QualityConfirmed { get; set; }
    public DateTimeOffset Added { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? ReviewedAt { get; set; }
    public string Sha256 { get; set; } = "";
    public long ByteLength { get; set; }
}
public sealed class ReferenceNote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Category { get; set; } = "Business";
    public string Source { get; set; } = "";
    public string Notes { get; set; } = "";
}
public sealed class CultivationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Details { get; set; } = "";
    public DateOnly? ObservationDate { get; set; }
    public int? Passage { get; set; }
    public string Condition { get; set; } = "Not rated";
}
public record DayScore(DateOnly Date, int Points, bool Submitted, bool Late, bool HasPhoto);
public sealed class CultureRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Species { get; set; } = "";
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string Method { get; set; } = "Source";
    public List<Guid> ParentIds { get; set; } = [];
    public string Notes { get; set; } = "";
}
