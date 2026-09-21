# Ronin Journal · Windows v0.1

A private desktop journal for Ronin Mushrooms Co. General business inventory, daily notes, photo documentation, reference notes and weekly documentation completeness. Uses your supplied Ronin artwork. There is no account, cloud service or shared database.

## Open the application

Open **RoninJournal.exe** inside the release folder. Keep the accompanying DLL and JSON application files beside it. The application needs the .NET 10 Desktop Runtime, already present on the computer where it was built. This release is a portable application folder, not an installer.

Your records appear in a **Data** folder beside the executable the first time you launch it. Keep the application somewhere your Windows account can write, rather than Program Files. Open only one instance for a given Data folder.

## Your first daily entry

1. Open **Inventory**, add your business supplies or assets, and choose **Save item**. Editing and archiving retain the item in the local database. Inventory forms require an explicit save.
2. Open **Journal**. The default record date is today according to your computer's local clock. To work on a different date, enter YYYY-MM-DD and choose **Open this date** before editing.
3. Write your notes and, when appropriate, confirm that you reviewed your inventory. Draft notes save automatically after a brief pause, when navigating away and when closing normally. Choose **Complete entry** to count the record toward your score. Editing a completed entry makes it a draft until completed again.
4. Open **Photos**, enter a date and identifying caption, then choose a PNG or JPEG up to 20 MB. Check the quality confirmation only after inspecting the subject, framing and lighting yourself. The app validates the file format but does not judge image content or sharpness. Photo copies remain available even if you move the original file.
5. Open **Weekly review** to see your documentation completeness. Missing records remain visible. Use **Backup** to export a ZIP to another drive or your own backup location.

The **Library** stores your own general business references with their categories and source details. It starts empty. Library and photo-edit forms require an explicit save. Search inventory, photo captions/dates or library notes from their respective screens.

## Culture family tree

Open **Family tree** (replacing Telemetry). Add a source with a culture name, species/strain and date, then choose **Save culture**. Select a culture in the tree and choose **Add child culture** to record a transfer, clone or isolate. Choose **Cross** and two different parents for a cross. Culture names sit on branching mycelium beneath a mushroom. Follow the roots downward from sources to descendants; crosses visibly join both parents. Click a name to edit it and highlight its ancestry, or use + Add child at a root tip to extend it. Search retains ancestor paths; expand/collapse branches and use zoom or Fit width to explore larger trees. The mushroom represents the collection, not a biological ancestor.

Sources have no parents; transfers, clones and isolates have one; crosses have two. Dates must be today or earlier and cannot predate parents. Circular ancestry is rejected. Up to 1,000 cultures and 64 ancestry levels are supported. Save forms explicitly before changing selection. Cultures are stored in the existing local journal database and included in complete backup/restore. Existing journal records and the separate Genetics observations are retained.

The browser demo provides the same tracker, stores its own records in browser local storage, and offers JSON export. Browser and desktop records do not synchronize. Keep the browser address/profile consistent and export before clearing browser data.

## Genetics observations

Open **Genetics** to enter a culture name, date and observation notes. Choose an existing family-tree name or type your own. Transfer number is optional (0 = original, 1 = first transfer); leave it blank if unknown. Condition is your own assessment and defaults to **Not rated**. The screen explains the choices and displays only your saved observations, with no illustrative vitality graph or automatic warning.

Choose **Save observation**, then search your observation history or choose **Edit observation** to correct an entry. Edits update the same record. Windows saves locally with journal backups; earlier Genetics notes remain visible and editable. The browser stores observations separately and offers JSON export. Save before navigating away; these forms do not autosave.

## Documentation score

A completed daily entry earns 40 points for written notes, 30 for inventory-review confirmation and 30 for a captioned photo whose quality you confirmed. Drafts earn zero. The weekly percentage averages elapsed days since this installation began; future days are excluded. Late records can improve completeness but remain marked late. This score measures documentation, not product quality, biological condition or readiness. Reporting a problem does not reduce it.

The initial version has a fixed daily checklist. Custom schedules, notifications, expenses, cultivation workflows, mobile access and synchronization are not included.

## Backups and recovery

**Export complete backup** creates a validated ZIP containing records and referenced photos. Keep a copy on another drive; the working folder and a copy on the same drive are not protection against drive failure. This version supports up to 20 MB of record text and 2 GB combined records/photos, with up to 10,000 photo records. Photo search shows up to 100 matching records at once.

**Restore** validates the archive before changing records and requires confirmation. It first writes a full rollback ZIP under `Data/Backups`. If the current photo files are missing or damaged, it instead preserves the surviving original files and records in a `recovery-*.zip` snapshot with a `RECOVERY-NOTES.txt` report, then recovers from the verified backup. Recovery snapshots are for manual inspection, not normal in-app restore.

Each successful records save also preserves one prior JSON version as `Data/journal.json.previous`. This is a short rollback aid; it does not contain photos or replace full ZIP backups.

If damaged JSON prevents startup, close the app and preserve the entire Data folder under another name before any recovery. Start with a new Data folder and restore a known-good exported backup. Keep the damaged original for manual recovery. Do not delete it or overwrite the only backup.

Data is local and unencrypted. Protect your Windows account and backup storage. The app makes no network requests.

## Sharing later

Send only the original clean application ZIP. **Do not send your Data folder** unless you intend to share all records and photographs. Your cousin can extract the application into his own writable folder and use a separate local database. He will need the .NET 10 Desktop Runtime. A self-contained installer can be built later.

## Development and verification

Open `App/App.csproj` in Visual Studio with desktop development tools, or open this directory in VS Code. No third-party package dependencies are required. The local NuGet.Config intentionally has no remote package sources.

```powershell
dotnet restore Checks --configfile NuGet.Config
dotnet run --project Checks --no-restore
dotnet restore UiChecks --configfile NuGet.Config
```

Run the UI harness from the parent project directory because it uses the source artwork as a test fixture:

```powershell
dotnet run --project RoninJournal/UiChecks --no-restore -- RoninJournal/qa
dotnet publish RoninJournal/App -c Release --no-restore -o RoninJournal/release
```

Core checks exercise real filesystem persistence, data validation, draft scoring, archive safety and photo recovery. The separate UI harness uses isolated fixtures, invokes real WPF controls and renders the actual screens. Test fixtures are never copied into the release.
