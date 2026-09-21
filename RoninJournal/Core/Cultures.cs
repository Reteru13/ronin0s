namespace RoninJournal.Core;

public sealed partial class JournalStore
{
    private static void ValidateCultures(List<CultureRecord> records)
    {
        if (records == null || records.Count > 1000) throw new InvalidDataException("Family tree supports up to 1,000 cultures.");
        if (records.Any(x => x == null || x.Id == Guid.Empty) || records.Select(x => x.Id).Distinct().Count() != records.Count)
            throw new InvalidDataException("Invalid or duplicate culture ID.");
        var map = records.ToDictionary(x => x.Id);
        foreach (var x in records)
        {
            Text(x.Name, 120, true); Text(x.Species, 120, true); Text(x.Notes, 20000);
            if (x.Date == default || x.Date > DateOnly.FromDateTime(DateTime.Today)) throw new InvalidDataException("Culture date must be today or earlier.");
            if (!new[] { "Source", "Transfer", "Clone", "Isolate", "Cross" }.Contains(x.Method)) throw new InvalidDataException("Choose a culture relationship type.");
            if (x.ParentIds == null || x.ParentIds.Count > 2 || x.ParentIds.Distinct().Count() != x.ParentIds.Count || x.ParentIds.Any(id => id == x.Id || !map.ContainsKey(id)))
                throw new InvalidDataException("Choose up to two different existing parents; a culture cannot be its own parent.");
            if ((x.Method == "Source" && x.ParentIds.Count != 0) || (x.Method == "Cross" && x.ParentIds.Count != 2) || (x.Method != "Source" && x.Method != "Cross" && x.ParentIds.Count != 1))
                throw new InvalidDataException("Source needs no parents; transfer, clone and isolate need one; cross needs two.");
            if (x.ParentIds.Any(id => map[id].Date > x.Date)) throw new InvalidDataException("A culture cannot predate its parent.");
        }
        var depths = new Dictionary<Guid, int>();
        for (int pass = 0; pass <= records.Count; pass++)
        {
            var pending = records.Where(x => !depths.ContainsKey(x.Id) && x.ParentIds.All(depths.ContainsKey)).ToList();
            if (pending.Count == 0) break;
            foreach (var x in pending)
            {
                var depth = x.ParentIds.Count == 0 ? 0 : x.ParentIds.Max(id => depths[id]) + 1;
                if (depth > 64) throw new InvalidDataException("Family tree supports up to 64 ancestry levels.");
                depths[x.Id] = depth;
            }
        }
        if (depths.Count != records.Count) throw new InvalidDataException("Parent links would create circular ancestry.");
    }
}
