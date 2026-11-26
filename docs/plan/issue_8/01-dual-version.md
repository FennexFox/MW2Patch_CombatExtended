# Dual-version package and verification

[Issue #8](https://github.com/FennexFox/MW2Patch_CombatExtended/issues/8) extends Phase 1 to stable RimWorld 1.6. Build the shared integration, package both targets, and exercise the same gameplay checks on each version with CE alone and with CE Attachments.

## Build and package

The .NET 10 SDK built both Release targets from a clean output with zero warnings and errors. The net472 and net10.0 assemblies use the same C# source. Both clean-build DLLs matched the files in the tested ZIP byte for byte. Package SHA-256: `0effd4cb6f22faba825366f8672d427f60663b318efa75f859aeb3e25c9d4dcb`.

The ZIP has exactly two DLLs. For each game version, resolving loadFolders.xml selected only its own DLL and the shared assets. The base configuration resolved three Phase 1 weapons. The optional configuration resolved all ten. All ten native texture paths matched the patches on both versions. The 1.6 XML preview generated 104 single-part combinations.

## Test matrix

| Game | Dependencies | CE alone | CE with Attachments |
| --- | --- | --- | --- |
| 1.6.4871 rev591 | CE 16.7.4.0 (979aed6f0d-rel), Harmony DLL 2.4.3.0, MW2 1.6 DLL | 37/37 gameplay checks | 147/147 gameplay checks |
| 1.7.5014 rev1195 | CE 17.0 at ecf1f16, Harmony DLL 3.0.0.0, MW2 1.7 DLL | 37/37 gameplay checks | 147/147 gameplay checks |

The tested CE Attachments revision was 8bcabf0. MW2 DLL SHA-256 prefixes were 7a0bb962704c (1.6) and ccc43b127ee4 (1.7). Both game configurations use the add-on's 1.6 weapon definitions.

The 1.7 tests included attaching and removing parts, saving and reloading customized weapons, firing the disposable launcher and checking the flare effect. The optional profile exercised the KSG at the workbench and verified both magazine contents and the selected tube after reloading the save. A first core run failed the random-map firing checks. A fresh-map repeat passed. The CE SentienceCatalyst startup error was previously reproduced without this patch.

The 1.6 tests used an isolated copy of the full Windows installation and the same ZIP as the 1.7 runs. Both profiles loaded the patch successfully and verified attachment fitting/removal, save/load, grenade and flare behavior, and consumption of the M72 on firing. The optional profile also verified all ten weapons and the KSG workbench job, both ammo counts and the selected tube after reload. The first optional run timed out on the M72 firing job (146/147). The complete repeat on a fresh map passed all 147 checks with the same package and test code.

## M72 stacks and CE component refresh

Stock MW2 guns use RimWorld's stack limit of one. Neither supported MW2 assembly compares installed parts when merging stacks. CE sets the M72 limit to 25. The patch gives each launcher its own item and rejects merges even in containers that ignore the stack limit. When loading a pile from CE, the original item keeps its identity and any saved customization. Surplus launchers are separate items.

The targeted runs used the game and dependency versions in the matrix above. Both Release builds finished with zero warnings and errors. The tested ZIP had SHA-256 `906feeac2e757d0975fb06aa4f8997d75cfb3edd8de6a7cf62c3ac2f15655736`.

| In-game check | Configurations | Result on both versions |
| --- | --- | --- |
| M72 stacking and workbench | CE alone and with Attachments | Stock pairs, stock plus fitted, matching optics and different optics stayed separate. Removing and refitting the red dot worked. The fitted optic survived serialization. |
| Load existing M72 piles | CE alone and with Attachments | A saved map pile of 25 and a pawn inventory pile of five loaded as 25 and five individual items. The original identifiers, fitted optic, custom name, hit points and map forbiddance passed their checks. |
| Fire modes | CE alone and with Attachments | A test part changed the primary burst count from six to one and back with the same CE fire-mode properties. AutoFire disappeared and returned, and aim modes remained unique. |
| KSG | CE with Attachments | Fitting a part with the secondary tube active kept that tube selected and preserved both loads. Opening the workbench unloaded both. Each tube reloaded to seven rounds and retained its count through switching. |
| Ammo disabled during a caliber change | CE alone | After fitting and removing a test caliber override, re-enabling ammunition exposed a compatible ammo type and an empty magazine on each change. |

The old-stack save fixtures covered ground items and pawn inventories. Other container types were not exercised. The RimWorld 1.6 save verification used a fresh game process. The 1.7 run again logged the upstream CE SentienceCatalyst patch error.
