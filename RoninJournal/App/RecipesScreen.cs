using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RoninJournal.Desktop;

public partial class MainWindow
{
    private sealed record RecipeEntry(string Title, string Summary, string Ingredients, string Method, string Pros, string Cons, string BestUse);
    private sealed record RecipeCategory(string Id, string Title, string Subtitle, RecipeEntry[] Entries);
    private static readonly RecipeCategory[] RecipeCategories =
    [
        new("agar", "Agar", "Media reference collection",
        [
            new("Light Malt Extract Agar (LMEA)", "A transparent baseline medium for routine culture work.",
                "500 ml distilled or RO water; 10 g agar-agar; 10 g light malt extract; optional pH adjustment toward 6.0.",
                "Dissolve, mix thoroughly, portion, and sterilize using your validated media cycle. Allow to cool before pouring plates.",
                "Consistent growth; transparent for observing culture; useful baseline for maintenance and expansion.",
                "Nutrient-rich media can also support competitor molds and bacteria when technique is poor.",
                "Routine strain maintenance, culture comparison, and liquid-culture validation."),
            new("Potato Dextrose Agar (PDA)", "A carbohydrate-rich medium for vigorous growth.",
                "500 ml distilled water; 10 g agar-agar; 10 g dextrose; 4 g potato infusion powder or about 5 g instant potato flakes.",
                "Prepare and dissolve completely, portion, and use a validated sterilization cycle. Avoid excessive heating that darkens or scorches the sugars.",
                "Economical; supports dense, fast colonization; useful for vigorous strains and storage slants.",
                "Can obscure early bacterial contamination; over-processing can darken the medium and inhibit growth.",
                "Rapid expansion and long-term slant storage."),
            new("Water Agar (WA)", "A low-nutrient medium for cleaning or rescuing cultures.",
                "500 ml distilled water; 10 g agar-agar.",
                "Dissolve, portion, sterilize, and transfer a small tissue sample. Move a clean leading edge to a richer medium promptly.",
                "Low nutrient load can reduce bacterial competition; useful for isolating a clean edge from a difficult sample.",
                "Cultures can stall or senesce if left too long; it is not a strong expansion medium.",
                "Wild-tissue cleanup and rescue transfers."),
            new("Malt Yeast Peptone Agar (MYPA)", "A nutrient-diverse medium for slow or reluctant cultures.",
                "500 ml distilled water; 10 g agar-agar; 7.5 g light malt extract; 0.25 g soy peptone; 0.25 g nutritional yeast extract.",
                "Dissolve completely, portion, and sterilize using a validated cycle. Expect a less transparent medium than LMEA.",
                "Broad nutrient profile; supports branching and spore germination; helpful for senescent cultures.",
                "Cloudiness can make early bacterial detection harder; richer media demand careful aseptic handling.",
                "Spore germination and slow-growing or tired cultures."),
            new("Activated Charcoal Agar (Black Agar)", "A dark, high-contrast medium for observing white mycelium.",
                "500 ml distilled water; 10 g agar-agar; 7.5 g light malt extract; 1–2 g finely milled activated charcoal.",
                "Disperse charcoal thoroughly, portion, and sterilize using a validated media cycle. Check that the suspension is even before pouring.",
                "Strong contrast for sectoring and photography; charcoal can bind some exudates.",
                "Dark competitor molds may be harder to see; charcoal can obscure subtle color changes.",
                "Photographic documentation, sector selection, and exudative cultures.")
        ]),
        new("grain", "Grain/Spawn", "Grain reference collection",
        [
            new("Rye Berries", "A nutrient-dense traditional grain preparation.",
                "Rye berries; clean water; optional gypsum to reduce clumping.",
                "Rinse, soak 12–24 hours, simmer about 10–15 minutes, drain and surface-dry, then use a validated pressure-sterilization cycle.",
                "Strong nutrition; good moisture retention; reliable for vigorous cultures.",
                "Labor-intensive; simmering can burst kernels and create a sticky, compact mass.",
                "High-value cultures where nutritional density and vigor matter."),
            new("Whole Oats", "A low-cost grain with a resilient hull.",
                "Whole oats and clean water.",
                "Simmer dry oats about 30–45 minutes, drain and surface-dry thoroughly, then use a validated pressure-sterilization cycle.",
                "Affordable; hulls resist bursting; maintains useful air gaps.",
                "Larger kernels provide fewer inoculation points; moisture still needs careful control.",
                "Cost-conscious or larger-scale spawn preparation."),
            new("White Millet (NSNS)", "Small kernels provide many inoculation points.",
                "One part dry millet; 0.5–0.75 parts water by weight.",
                "Combine directly in a suitable filter container, then use a validated pressure-sterilization cycle long enough for the container size.",
                "Very high inoculation-point density; no soaking or simmering; convenient for small-kernel spawn.",
                "Moisture ratios are less forgiving; overhydration can make the kernels clump.",
                "Fast distribution through spawn and small-container work."),
            new("Wheat Berries", "A locally available alternative to rye.",
                "Wheat berries; clean water; gypsum may help reduce sticking.",
                "Use either a 12-hour soak followed by a short simmer or a measured no-soak hydration method, then surface-dry and sterilize using a validated cycle.",
                "Balanced nutrition; often inexpensive and easy to source.",
                "Gluten can make kernels sticky; excess moisture restricts gas exchange.",
                "A practical rye substitute where wheat is more accessible."),
            new("Wild Bird Seed / Sorghum", "A mixed-grain option with varied kernel sizes.",
                "Millet/sorghum-based seed mix; clean water; remove sunflower seeds and floating debris.",
                "Soak about 12 hours, skim debris, simmer about 10 minutes, drain and surface-dry, then use a validated pressure-sterilization cycle.",
                "Accessible; varied grain sizes combine many inoculation points with moisture retention.",
                "Washing is laborious; cracked corn can become mushy and create contamination-prone zones.",
                "Hobby cultivation and locations without dedicated grain suppliers.")
        ]),
        new("substrate", "Substrate", "Material reference collection",
        [
            new("CVG (Coir, Vermiculite, Gypsum)", "A low-nutrient hydrated matrix for secondary decomposers.",
                "650 g coco coir brick; 2 quarts coarse vermiculite; 1 cup gypsum; about 4–4.5 quarts boiling water.",
                "Mix in an insulated container, hydrate evenly, seal, and allow slow cooling for several hours. Confirm field capacity before use.",
                "Simple; hydrated coir resists contamination; does not require pressure sterilization in the basic formulation.",
                "Low nutritional ceiling; performs poorly for primary wood decomposers.",
                "Low-contamination-risk bulk applications for secondary decomposers."),
            new("The Master's Mix (50/50)", "A high-nitrogen commercial gourmet block mix.",
                "Equal dry weights hardwood sawdust or pellets and soybean hull pellets; about 60% moisture, such as 1 lb wood, 1 lb soy, and 1.4 L water.",
                "Mix dry ingredients, hydrate evenly, bag, and use a validated long pressure-sterilization cycle appropriate to the bag size.",
                "High biological efficiency; supports fast, productive Oyster and Lion's Mane blocks.",
                "High nutrient load makes contamination severe when sterile technique fails; requires careful sterilization.",
                "Controlled commercial production of gourmet species."),
            new("Cold-Water Lime-Pasteurized Straw", "A low-tech straw method for primary decomposers.",
                "Chopped wheat or oat straw; cold water; hydrated lime at roughly 2 g per liter.",
                "Chop straw into 2–4 inch pieces, soak 12–24 hours in the lime bath, drain completely, and inoculate using appropriate protective equipment.",
                "Low equipment and energy requirements; inexpensive; suitable for high-volume Oyster production.",
                "Lime is caustic; alkaline runoff needs responsible handling; unsuitable for heavily supplemented mixes.",
                "Outdoor or warehouse Oyster cultivation."),
            new("Supplemented Hardwood Sawdust (80/20)", "A moderate-nutrient block for specialty wood lovers.",
                "80% hardwood sawdust; 20% wheat or rice bran by dry weight; hydrate to about 60% moisture; gypsum may be useful for Shiitake.",
                "Mix, hydrate evenly, bag, and use a validated pressure-sterilization cycle appropriate to the container size.",
                "Stable and reliable; supports dense colonization; lower contamination risk than heavily supplemented mixes.",
                "Slower than high-nitrogen blends; requires pressure sterilization and careful moisture control.",
                "Shiitake, Maitake, and other slow-growing specialty species."),
            new("Composted Manure Bulk Substrate", "A nutrient-rich compost blend for dung-adapted species.",
                "50% properly composted and leached manure; 30% coco coir; 15% chopped straw; 5% gypsum; water to field capacity.",
                "Hydrate to field capacity and thermally pasteurize in a controlled 140–160°F range. Do not exceed the validated temperature window.",
                "High nutrient availability; supports Agaricus and other dung-adapted species; beneficial thermophiles can remain active.",
                "Biological and odor hazards; requires PPE and temperature control; over-processing increases contamination risk.",
                "Commercial Button mushroom work and dung-adapted species.")
        ])
    ];
    private string? recipeCategory;
    private int? recipeEntry;

    private void Recipes()
    {
        Page.Children.Clear();
        Eyebrow.Text = "RONIN.1.2 / REFERENCE COLLECTIONS";
        PageTitle.Text = "Recipes";
        var categories = new WrapPanel { Margin = new Thickness(0, 0, 0, 18) };
        AddCategoryTab("All categories", null);
        foreach (var category in RecipeCategories) AddCategoryTab(category.Title, category.Id);
        void AddCategoryTab(string title, string? id)
        {
            var tab = Button(title, () => { recipeCategory = id; recipeEntry = null; PageScroll.ScrollToTop(); Recipes(); }, "RecipeCategory" + (id ?? "all"));
            tab.Margin = new Thickness(0, 0, 8, 8);
            tab.Background = Color(recipeCategory == id ? "#41325F" : "#152136");
            tab.BorderBrush = Color(recipeCategory == id ? "#B99AED" : "#34415A");
            System.Windows.Automation.AutomationProperties.SetHelpText(tab, recipeCategory == id ? "Selected category" : "Open category");
            categories.Children.Add(tab);
        }
        Page.Children.Add(categories);

        if (recipeCategory == null)
        {
            Intro("Choose a collection, then open an individual reference entry. These protocols are reference material; validate equipment, sanitation, species, and local requirements before use.");
            foreach (var category in RecipeCategories)
            {
                var photo = CategoryPhoto(category.Id, 188);
                var copy = Stack(Text(category.Title, 27, "#EEE8FB", true), Text(category.Subtitle, 14, "#B7C4D8"));
                copy.Children[1].SetValue(MarginProperty, new Thickness(0, 8, 0, 16));
                copy.Children.Add(Text("5 reference entries", 12, "#C9B4F0"));
                var open = Button("Explore " + category.Title + "  →", () => { recipeCategory = category.Id; recipeEntry = null; PageScroll.ScrollToTop(); Recipes(); }, "OpenRecipe" + category.Id, true);
                open.Margin = new Thickness(0, 19, 0, 0); copy.Children.Add(open);
                Page.Children.Add(Card(Columns(photo, copy, 1.1, 1), 18));
            }
            return;
        }

        var selected = RecipeCategories.Single(x => x.Id == recipeCategory);
        var breadcrumb = Text("Recipes  /  " + selected.Title + (recipeEntry == null ? "" : "  /  Entry " + recipeEntry.Value.ToString("00")), 13, "#BDA3EF");
        breadcrumb.Margin = new Thickness(0, 0, 0, 16); Page.Children.Add(breadcrumb);
        Page.Children.Add(Card(CategoryPhoto(selected.Id, recipeEntry == null ? 205 : 245), 12));

        if (recipeEntry == null)
        {
            Intro(selected.Title + " entries · select an entry to open its reference page.");
            for (int n = 1; n <= selected.Entries.Length; n++)
            {
                int number = n;
                var content = Stack(Text(selected.Entries[number - 1].Title, 18, "#EEE8FB", true), Text(selected.Entries[number - 1].Summary, 12, "#CCB5F0"));
                content.Children[1].SetValue(MarginProperty, new Thickness(0, 5, 0, 0));
                var entry = Button("", () => { recipeEntry = number; PageScroll.ScrollToTop(); Recipes(); }, "RecipeEntry" + number);
                entry.Content = content; entry.HorizontalAlignment = HorizontalAlignment.Stretch; entry.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                entry.Margin = new Thickness(0, 0, 0, 10); entry.Background = Color("#181E34");
                System.Windows.Automation.AutomationProperties.SetName(entry, "Open " + selected.Entries[number - 1].Title + " reference entry");
                Page.Children.Add(entry);
            }
        }
        else
        {
            var entry = selected.Entries[recipeEntry.Value - 1];
            var details = Stack(Text(entry.Title, 26, "#EEE8FB", true), Text("REFERENCE PROTOCOL", 11, "#D2B5F8"));
            details.Children[1].SetValue(MarginProperty, new Thickness(0, 10, 0, 18));
            details.Children.Add(Text(entry.Summary, 15, "#BCCADD"));
            AddSection(details, "Ingredients / ratios", entry.Ingredients);
            AddSection(details, "Preparation / method", entry.Method);
            AddSection(details, "Pros", entry.Pros);
            AddSection(details, "Cons", entry.Cons);
            AddSection(details, "Best used for", entry.BestUse);
            var entryId = Text("Entry ID: " + selected.Id + "-" + recipeEntry.Value.ToString("00"), 12, "#A4B3C9");
            entryId.Margin = new Thickness(0, 16, 0, 0); details.Children.Add(entryId);
            Page.Children.Add(Card(details));
            Page.Children.Add(Button("← Back to " + selected.Title, () => { recipeEntry = null; PageScroll.ScrollToTop(); Recipes(); }, "RecipeBack"));
        }
    }

    private static void AddSection(StackPanel panel, string heading, string body)
    {
        var section = Stack(Text(heading, 14, "#CDB5F7", true), Text(body, 14, "#D5DFEC"));
        section.Children[1].SetValue(MarginProperty, new Thickness(0, 6, 0, 0));
        section.Margin = new Thickness(0, 16, 0, 0);
        panel.Children.Add(section);
    }

    private static StackPanel CategoryPhoto(string id, double height)
    {
        var photo = new Image
        {
            Name = "RecipePhoto" + id,
            Source = Bitmap("pack://application:,,,/RoninJournal;component/Assets/Recipes/" + id + "/cover.png"),
            Height = height,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        System.Windows.Automation.AutomationProperties.SetName(photo, id switch { "agar" => "Ronin holding agar plates", "grain" => "Ronin holding a jar of grain", _ => "Ronin holding substrate materials" });
        var caption = Text("Ronin character illustration", 11, "#9DAFC7");
        caption.Margin = new Thickness(0, 8, 0, 0);
        return Stack(photo, caption);
    }
}
