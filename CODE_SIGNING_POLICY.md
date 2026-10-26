# Code signing policy

Free code signing provided by [SignPath.io](https://signpath.io), certificate by [SignPath Foundation](https://signpath.org).
*(Applies once the SignPath application is approved. Until then, releases are unsigned.)*

## What is signed
- `Delft3D4Launcher.exe`: built by GitHub Actions from this repository.
- `Delft3D4-Community-Setup-*.exe`: the Inno Setup installer built by GitHub Actions from this repository.

Not signed by this project (upstream / third-party, bundled unmodified):
- Delft3D engines (`bin\*.exe`, `lib\*.dll`) compiled from Deltares' public source, https://github.com/Deltares/Delft3D (tag noted in each release).
- Intel oneAPI runtime redistributables and the Microsoft Visual C++ runtime.

## Team roles
| Role | Member |
|---|---|
| Authors (commit) | [DevAnthonyM](https://github.com/DevAnthonyM) |
| Reviewers | [DevAnthonyM](https://github.com/DevAnthonyM) |
| Approvers (approve each signing request) | [DevAnthonyM](https://github.com/DevAnthonyM) |

All team members use multi-factor authentication on GitHub and SignPath.

## Privacy
This program will not transfer any information to other networked systems unless specifically requested by the user. The only network actions are opening the project website, Deltares' download page or the source code page in your browser when you click those Help menu items.

## Releases
Each release lists the SHA-256 of every file and links to its VirusTotal report. Only files built by this repository's GitHub Actions and approved by the approver are signed.
