Free, unofficial community build of **Delft3D 4 (Delft3D-FLOW suite)** for Windows 64-bit.
**Not affiliated with or endorsed by Deltares.** Delft3D and Deltares are trademarks of Stichting Deltares.

Built unmodified from the official Deltares open-source code, tag `DIMRset_2026.02`, commit [`5a4649830b1e5072caf019fb4850bbdefd9ad431`](https://github.com/Deltares/Delft3D/commit/5a4649830b1e5072caf019fb4850bbdefd9ad431).

Download page with instructions: https://delft3d-community.vercel.app

| File | What it is | SHA-256 |
|---|---|---|
| `Delft3D4-Community-Setup-DIMRset_2026.02.exe` | Ready-to-run installer (engines + launcher, per-user, no admin) | `aba9ec6d9c76db368150df55c765b4516b841dfb379756ffba8d8978af701881` |
| `Delft3D4-Community-Portable-DIMRset_2026.02-win64.zip` | Compiled engine files (bin, lib, share), example model, licences | `6a672c56916a038803e8f2feb93c263337effc674dc7e0eb096e18f43bc90b09` |

Check a file in PowerShell: `Get-FileHash .\<file> -Algorithm SHA256`

**Not code-signed.** Windows SmartScreen and some antivirus tools may warn about new unsigned programs. Compare the SHA-256 above before running.

Licences: launcher GPL-3.0; Delft3D GPL-3.0 / LGPL-2.1 / AGPL-3.0; bundled Intel oneAPI and Microsoft Visual C++ runtime redistributables under their own licences (texts included in the `licenses` folder).
