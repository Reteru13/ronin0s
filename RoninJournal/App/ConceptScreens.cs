using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RoninJournal.Desktop;

public partial class MainWindow
{
    private static readonly string[] PromptCards =
    [
        "Document culture ancestry with source, transfer, clone, isolate and cross records.",
        "Create a batch record that tracks substrate, spawn, sterilization, and harvest dates.",
        "Review a culture image for visible contamination indicators and list the next safe action."
    ];

    private void Operations()
    {
        Intro("The ronin0s cultivation concepts are now part of the desktop app. These local tools are decision aids; they do not replace validated sterilization, safety, or environmental procedures.");
        var cards = new[]
        {
            ("Formulas", "C:N and hydration engine", "Balance wood, supplements, water, and gypsum."),
            ("Family tree", "Culture ancestry", "Trace parents, transfers, clones, isolates and crosses."),
            ("Vision", "Inspection workspace", "Record a visual finding and recommended response."),
            ("Genetics", "Culture observations", "Enter observations and review your saved culture history."),
            ("Batch planner", "Yield planning", "Translate target yield into substrate and spawn needs.")
        };
        var grid = new UniformGrid { Columns = 2 };
        foreach (var card in cards)
        {
            var title = card.Item1;
            var panel = Stack(Text(card.Item1.ToUpperInvariant(), 11, "#89E4E0", true), Text(card.Item2, 19, "#F3F0E7", true), Text(card.Item3, 13, "#A4B3C9"));
            var button = Button("Open " + title + "  →", () => Navigate(title), primary: true);
            button.Margin = new Thickness(0, 18, 0, 0);
            panel.Children.Add(button);
            var border = Card(panel);
            border.Margin = new Thickness(0, 0, 14, 14);
            grid.Children.Add(border);
        }
        Page.Children.Add(grid);
        var prompts = Stack(Text("AI prompt hub", 18, "#F3F0E7", true), Text("Copy-ready prompts for building local cultivation workflows.", 13, "#A4B3C9"));
        for (var i = 0; i < PromptCards.Length; i++)
        {
            var prompt = PromptCards[i];
            prompts.Children.Add(Text("Prompt " + (i + 1), 12, "#89E4E0", true));
            prompts.Children.Add(Text(prompt, 13));
            prompts.Children.Add(Button("Copy prompt text", () => Clipboard.SetText(prompt)));
        }
        Page.Children.Add(Card(prompts));
    }

    private void Formulas()
    {
        Intro("Adjust dry ingredients to estimate elemental balance and field-capacity water. Values are planning estimates; validate the actual material moisture and recipe.");
        var wood = Input("HardwoodKg", "1.00"); var soy = Input("SoyHullsKg", "1.00"); var bran = Input("WheatBranKg", "0.00"); var coir = Input("CoirKg", "0.00"); var gypsum = Input("GypsumKg", "0.05");
        var output = Text("Enter quantities to calculate.", 16, "#F3F0E7", true);
        void Calculate()
        {
            decimal D(TextBox box) => decimal.TryParse(box.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var value) ? value : 0;
            var carbon = D(wood) * .50m + D(soy) * .45m + D(bran) * .46m + D(coir) * .48m;
            var nitrogen = D(wood) * .001m + D(soy) * .045m + D(bran) * .025m + D(coir) * .004m;
            var dry = D(wood) + D(soy) + D(bran) + D(coir) + D(gypsum);
            var ratio = nitrogen == 0 ? 0 : carbon / nitrogen;
            output.Text = $"Dry weight  {dry:0.00} kg\nField-capacity water  {dry * 1.5m:0.00} L\nEstimated C:N  {ratio:0.0}:1\n\n" +
                (ratio < 25 ? "HIGH NITROGEN: review contamination controls." : ratio <= 45 ? "BALANCED: inside the planning range." : "HIGH CARBON: expect slower nutrient release.");
            output.Foreground = Color(ratio < 25 ? "#FFBABE" : ratio <= 45 ? "#8AE7CD" : "#F4C97A");
        }
        foreach (var box in new[] { wood, soy, bran, coir, gypsum }) box.TextChanged += (_, _) => Calculate();
        var form = Stack(); Field(form, "Hardwood sawdust", wood); Field(form, "Soy hulls", soy); Field(form, "Wheat bran", bran); Field(form, "Coco coir", coir); Field(form, "Gypsum buffer", gypsum);
        var save = Button("Save formula to Library", () => { store.SaveCultivation(new() { Type = "Formula", Title = "C:N substrate formula", Details = output.Text }); Success("Formula saved to local cultivation records."); }, primary: true);
        form.Children.Add(save);
        Page.Children.Add(Columns(Card(form), Card(Stack(Text("Result", 18, "#F3F0E7", true), output))));
        Calculate();
    }

    private void Vision()
    {
        Intro("Inspection workspace for documenting what you see. This is a human-review tool, not a diagnostic model.");
        var finding = Input("Finding", "Trichoderma-like patch"); var confidence = Input("ConfidencePercent", "86"); var response = Input("RecommendedResponse", "Quarantine block; inspect airflow, humidity, and handling records.", true, 500);
        var save = Button("Save inspection note", () => { store.SaveCultivation(new() { Type = "Vision inspection", Title = finding.Text, Details = $"Confidence: {confidence.Text}%\n\n{response.Text}" }); Success("Inspection note saved to local cultivation records."); }, primary: true);
        Page.Children.Add(Card(Stack(Text("Visual inspection", 18, "#F3F0E7", true), Text("Use the photo record to attach the original image separately in Photos.", 13, "#A4B3C9"), Text("Finding", 12, "#BCCBDE"), finding, Text("Confidence", 12, "#BCCBDE"), confidence, Text("Next safe action", 12, "#BCCBDE"), response, save)));
    }

    private void BatchPlanner()
    {
        Intro("Reverse-plan a batch from a target yield. This is a starting estimate; adjust for your measured biological efficiency and container capacity.");
        var yield = Input("TargetYieldKg", "10"); var be = Input("ExpectedBEPercent", "100"); var moisture = Input("WetSubstrateMultiplier", "1.60"); var spawn = Input("SpawnRatio", "0.10");
        var output = Text("", 16, "#F3F0E7", true);
        void Calculate()
        {
            decimal D(TextBox box) => decimal.TryParse(box.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var value) ? value : 0;
            var target = D(yield); var efficiency = D(be) / 100m; var multiplier = D(moisture); var ratio = D(spawn);
            if (efficiency <= 0 || multiplier <= 0) { output.Text = "Enter positive efficiency and moisture values."; return; }
            var dry = target / efficiency / multiplier; var spawnKg = dry * ratio;
            output.Text = $"Estimated dry substrate  {dry:0.0} kg\nWet substrate  {(dry * multiplier):0.0} kg\nSpawn  {spawnKg:0.0} kg\nLiquid culture  {Math.Max(.5m, dry * .02m):0} mL\nSterilization runs  {Math.Max(1, Math.Ceiling(dry / 120)):0}";
        }
        foreach (var box in new[] { yield, be, moisture, spawn }) box.TextChanged += (_, _) => Calculate();
        var form = Stack(); Field(form, "Target yield", yield); Field(form, "Expected BE", be); Field(form, "Wet substrate multiplier", moisture); Field(form, "Spawn ratio", spawn);
        form.Children.Add(Button("Save batch plan", () => { store.SaveCultivation(new() { Type = "Batch plan", Title = $"Batch target {yield.Text} kg", Details = output.Text }); Success("Batch plan saved to local cultivation records."); }, primary: true));
        Page.Children.Add(Columns(Card(form), Card(Stack(Text("Plan", 18, "#F3F0E7", true), output))));
        Calculate();
    }
}
