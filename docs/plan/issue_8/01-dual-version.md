# Dual-version package and verification

[Issue #8](https://github.com/FennexFox/MW2Patch_CombatExtended/issues/8) extends Phase 1 to stable RimWorld 1.6. Build the shared integration, package both targets, and exercise the same gameplay checks on each version with CE alone and with CE Attachments.

## Build and package

The .NET 10 SDK built both Release targets from a clean output with zero warnings and errors. The net472 and net10.0 assemblies use the same C# source. Both clean-build DLLs matched the files in the tested ZIP byte for byte. Package SHA-256: `0effd4cb6f22faba825366f8672d427f60663b318efa75f859aeb3e25c9d4dcb`.

The ZIP has exactly two DLLs. For each game version, resolving loadFolders.xml selected only its own DLL and the shared assets. The base configuration resolved three Phase 1 weapons. The optional configuration resolved all ten. All ten native texture paths matched the patches on both versions. The 1.6 XML preview generated 104 single-part combinations.

## Test matrix

| Game | Dependencies | CE alone | CE with Attachments |
| --- | --- | --- | --- |
| 1.6.4871 | CE 16.7.4.0, Harmony DLL 2.4.3.0, MW2 1.6 DLL | Build and static checks passed. Runtime pending. | Build and static checks passed. Runtime pending. |
| 1.7.5014 rev1195 | CE 17.0 at ecf1f16, Harmony DLL 3.0.0.0, MW2 1.7 DLL | 37/37 gameplay checks | 147/147 gameplay checks |

The tested CE Attachments revision was 8bcabf0. MW2 DLL SHA-256 prefixes were 7a0bb962704c (1.6) and ccc43b127ee4 (1.7). Both game configurations use the add-on's 1.6 weapon definitions.

The 1.7 tests included attaching and removing parts, saving and reloading customized weapons, firing the disposable launcher and checking the flare effect. The optional profile exercised the KSG at the workbench and verified both magazine contents and the selected tube after reloading the save. A first core run failed the random-map firing checks. A fresh-map repeat passed. The CE SentienceCatalyst startup error was previously reproduced without this patch.

The local 1.6 test image could supply compile-time references but lacked core content and a runtime dependency. Its startup failed before the mod could be tested. Run both profiles against a complete 1.6 installation to check startup, workbench actions, secondary functions and save/load before release.
