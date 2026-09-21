# Security Policy

## Scope

Ronin0s is a local-first browser application. Records are stored in the browser's local storage and are not synchronized with a server by this project. The desktop companion stores records and photos on the local filesystem.

Security reports are welcome for issues that could:

- Expose, corrupt, or delete Ronin0s records outside the user's intended actions.
- Bypass validation or safety checks in the desktop application.
- Cause unintended network requests, code execution, or access to files outside the application's intended data area.
- Introduce a dependency, build, or release-process vulnerability.

## Supported versions

Only the latest version on the `main` branch is actively supported. Older commits, forks, and unmodified archived copies may not receive security fixes.

## Reporting a vulnerability

Please do not disclose a suspected vulnerability in a public GitHub issue, pull request, discussion, or social-media post.

Use one of these private channels:

1. If GitHub Private Vulnerability Reporting is enabled for this repository, use the **Security** tab and choose **Report a vulnerability**.
2. Otherwise, contact the repository owner privately through the GitHub profile for [@Reteru1313](https://github.com/Reteru1313).

Please include:

- A clear description of the issue and its potential impact.
- The affected file, version, commit, or browser/desktop environment.
- Reproduction steps or a minimal proof of concept.
- Whether the issue requires an existing local file, imported backup, browser data, or user interaction.
- Any suggested mitigation, if known.

Please redact personal records, photographs, credentials, tokens, and other private data from reports. If a sample file is necessary, use synthetic data.

## Response process

We will acknowledge a report when practicable, investigate the report, and communicate the disposition when a review is complete. Confirmed issues will be prioritized according to impact and exploitability. Fixes may be released with a security note, a changelog entry, or an advisory when appropriate.

Please allow reasonable time for investigation and remediation before public disclosure. Coordinated disclosure is preferred.

## User security responsibilities

Ronin0s is not a hosted service and does not provide account security, encryption, access control, or remote backup. Users are responsible for:

- Protecting the Windows account, browser profile, and computer where records are stored.
- Keeping the application and browser up to date.
- Treating exported backups as sensitive files; they may contain business records and photographs.
- Storing backups in locations with appropriate permissions and avoiding untrusted restore archives.
- Sending only a clean application package when sharing Ronin0s. The local `Data` folder and browser storage may contain private records.
- Reviewing third-party changes before running scripts, importing data, or installing dependencies.

The browser demo stores data locally and does not synchronize with the desktop application. Clearing browser storage, changing browser profiles, or using private browsing can make browser records unavailable. Export important records before doing so.

## Known limitations

- Browser local storage is not a secure vault and may be readable by software or users with access to the browser profile.
- Desktop records are stored locally and unencrypted.
- Backups are not encrypted by Ronin0s.
- The application cannot guarantee that a user's operating system, browser extensions, filesystem, or backup provider is trustworthy.

These limitations are documented behavior rather than vulnerabilities by themselves. Reports that demonstrate an additional unintended access path are still welcome.

## Dependency and release hygiene

Changes should avoid unnecessary dependencies, keep dependencies current, and include tests for security-relevant behavior such as validation, import/export, restore safety, and filesystem boundaries. Do not commit secrets, private records, credentials, production exports, or recovery archives.

