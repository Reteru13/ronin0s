# Contributing to Ronin0s

## Prerequisites

- Windows 10 or 11 for the WPF desktop project.
- .NET 10 SDK.
- Node.js 20 or newer for the browser tests.

## Build and test

From the repository root:

```powershell
dotnet restore RoninJournal/Checks/Checks.csproj --configfile RoninJournal/NuGet.Config
dotnet run --project RoninJournal/Checks/Checks.csproj --no-restore
dotnet restore RoninJournal/UiChecks/UiChecks.csproj --configfile RoninJournal/NuGet.Config
dotnet run --project RoninJournal/UiChecks/UiChecks.csproj --no-restore -- RoninJournal/qa
node --test family-tree.test.cjs family-tree-ui.test.cjs
```

The UI harness writes screenshots and isolated data under `RoninJournal/qa/`.
Those files are generated and must not be committed.

## Desktop build

```powershell
dotnet publish RoninJournal/App/App.csproj -c Release --no-restore -o release/RoninJournal-windows
```

The desktop package currently requires the .NET 10 Desktop Runtime. Do not
include a user's `Data` folder or exported backups in a release artifact.

## Pull requests

- Explain the user-visible behavior being changed.
- Add or update focused tests for behavior changes.
- Run the relevant checks before opening a pull request.
- Keep browser and desktop data formats backward-compatible unless the change
  includes a migration plan.
- Do not commit credentials, private records, photographs, backups, generated
  output, or local shortcuts.

## Security reports

Do not open a public issue for a suspected vulnerability. Follow
[`SECURITY.md`](./SECURITY.md) instead.
