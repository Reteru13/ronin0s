using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RoninJournal.Core;

namespace RoninJournal.Desktop;

public partial class MainWindow
{
    private void DrawMycelium(Canvas canvas, List<CultureRecord> records, Guid selected, Action<CultureRecord> select, Action<Guid> toggle, HashSet<Guid> collapsed, HashSet<Guid> branches, Action<CultureRecord> add)
    {
        canvas.Children.Clear();
        var depth = new Dictionary<Guid, int>();
        for (int pass = 0; pass <= records.Count; pass++)
        {
            var ready = records.Where(x => !depth.ContainsKey(x.Id) && x.ParentIds.All(depth.ContainsKey)).ToList();
            if (ready.Count == 0) break;
            foreach (var row in ready) depth[row.Id] = row.ParentIds.Count == 0 ? 0 : row.ParentIds.Max(id => depth[id]) + 1;
        }
        var levels = records.GroupBy(row => depth[row.Id]).OrderBy(group => group.Key).ToList();
        var width = Math.Max(680, ((levels.Count == 0 ? 0 : levels.Max(group => group.Count())) + 1) * 220 + 120);
        canvas.Width = width; canvas.Height = Math.Max(530, 430 + (depth.Count == 0 ? 0 : depth.Values.Max()) * 165);
        var positions = new Dictionary<Guid, Point>();
        foreach (var level in levels)
        {
            var sorted = level.OrderBy(row => row.ParentIds.Count == 0 ? 0 : row.ParentIds.Average(id => positions[id].X)).ThenBy(row => row.Name).ThenBy(row => row.Id).ToList();
            for (int i = 0; i < sorted.Count; i++) positions[sorted[i].Id] = new Point((width - 120d) * (i + 1) / (sorted.Count + 1) + 60, 260 + level.Key * 165);
        }
        var lineage = new HashSet<Guid> { selected }; var queue = new Queue<Guid>(records.FirstOrDefault(x => x.Id == selected)?.ParentIds ?? []);
        while (queue.TryDequeue(out var id)) if (lineage.Add(id)) foreach (var parent in records.Single(x => x.Id == id).ParentIds) queue.Enqueue(parent);
        System.Windows.Shapes.Path Path(FormattableString data, string stroke, double thickness = 2, string? fill = null)
        {
            var path = new System.Windows.Shapes.Path { Data = Geometry.Parse(data.ToString(CultureInfo.InvariantCulture)), Stroke = Color(stroke), StrokeThickness = thickness, Fill = fill == null ? null : Color(fill), StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round, IsHitTestVisible = false };
            canvas.Children.Add(path); return path;
        }
        double cx = width / 2d;
        Path($"M {cx-21} 114 C {cx-15} 151 {cx-31} 185 {cx-24} 218 Q {cx} 233 {cx+25} 218 C {cx+16} 182 {cx+27} 145 {cx+20} 114 Z", "#B6D6C3", 2, "#DEE9D9");
        Path($"M {cx-145} 112 C {cx-113} 31 {cx-53} 16 {cx} 28 C {cx+62} 9 {cx+122} 55 {cx+145} 112 Q {cx} 166 {cx-145} 112 Z", "#C9E8D0", 2, "#78A893");
        Path($"M {cx-145} 112 Q {cx} 95 {cx+145} 112", "#CCE4CC");
        for (int i = -5; i <= 5; i++) Path($"M {cx+i*24} 119 Q {cx+i*12} 138 {cx+i*3} 143", "#E8EFCD", 1);
        Path($"M {cx-103} 84 Q {cx-67} 42 {cx-23} 46", "#D8EDCD", 3);
        Path($"M 35 231 Q {cx} 221 {width-35} 231", "#294738", 1);
        foreach (var row in records)
        {
            var p = positions[row.Id];
            if (row.ParentIds.Count == 0) Path($"M {cx} 219 C {cx} 243 {p.X} 227 {p.X} {p.Y}", "#749E7E", 3);
            foreach (var parentId in row.ParentIds)
            {
                var a = positions[parentId]; var start = a.Y + 66; var active = lineage.Contains(parentId) && lineage.Contains(row.Id);
                var stroke = active ? "#B4EFB6" : "#648A70";
                System.Windows.Shapes.Path link;
                if (depth[row.Id] - depth[parentId] > 1)
                {
                    var side = a.X < cx ? 24 : width - 24;
                    link = Path($"M {a.X} {start} C {a.X} {start+35} {side} {start+35} {side} {start+65} L {side} {p.Y-65} C {side} {p.Y-25} {p.X} {p.Y-40} {p.X} {p.Y}", stroke, active ? 4 : 2.5);
                }
                else link = Path($"M {a.X} {start} C {a.X} {start+52} {p.X} {p.Y-52} {p.X} {p.Y}", stroke, active ? 4 : 2.5);
                link.Tag = "RootLink:" + parentId + ":" + row.Id;
                Path($"M {p.X-4} {p.Y-9} L {p.X} {p.Y-2} L {p.X+4} {p.Y-9}", stroke);
            }
            if (!records.Any(child => child.ParentIds.Contains(row.Id)))
                foreach (var spread in new[] { -54, -20, 24, 58 }) Path($"M {p.X} {p.Y+66} C {p.X} {p.Y+98} {p.X+spread} {p.Y+105} {p.X+spread*1.25} {p.Y+133}", "#355D45", 1.4);
        }
        foreach (var row in records)
        {
            var p = positions[row.Id];
            var button = Button(row.Name + "\n" + row.Method + (row.ParentIds.Count == 2 ? " · 2 parents" : ""), () => select(row));
            button.Width = 180; button.Height = 66; button.Padding = new Thickness(8); button.FontSize = 16; button.ClipToBounds = true;
            button.Background = Color(row.Id == selected ? "#3C6549" : "#173B2C"); button.BorderBrush = Color(lineage.Contains(row.Id) ? "#D6ECC3" : "#719780"); button.BorderThickness = new Thickness(row.Id == selected ? 2 : 1);
            button.ToolTip = row.Name + "\n" + row.Species + "\n" + row.Date.ToString("yyyy-MM-dd");
            System.Windows.Automation.AutomationProperties.SetName(button, "Edit " + row.Name);
            Canvas.SetLeft(button, p.X - 90); Canvas.SetTop(button, p.Y); canvas.Children.Add(button);
            if (branches.Contains(row.Id))
            {
                var fold = Button(collapsed.Contains(row.Id) ? "+" : "−", () => toggle(row.Id)); fold.Width = 26; fold.Height = 26; fold.Padding = new Thickness(0);
                fold.ToolTip = "Show / hide descendants of " + row.Name;
                Canvas.SetLeft(fold, p.X + 72); Canvas.SetTop(fold, p.Y + 51); canvas.Children.Add(fold);
            }
            else
            {
                var child = Button("+ Add child", () => add(row)); child.Width = 104; child.Height = 30; child.Padding = new Thickness(3); child.FontSize = 13; child.Background = Color("#183E2B"); child.BorderBrush = Color("#719780");
                Canvas.SetLeft(child, p.X - 52); Canvas.SetTop(child, p.Y + 110); canvas.Children.Add(child);
            }
        }
    }
}
