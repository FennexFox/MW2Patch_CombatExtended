# MW2Patch_CombatExtended
This mod makes `ModularWeapons 2` compatible with `Combat Extended`.

Load `ModularWeapons 2` BEFORE CE, and load _this mod_ AFTER CE.

## RimWorld 1.7

Phase 1 adds workbench mounts to the CE grenade launcher, disposable rocket launcher, and flare gun. Enabling Combat Extended Attachments adds the AKM-GL, M16A4-GL, KSG, LeMat, Double Badger, Savage 2400, and TP-82. Each weapon keeps its original sprite and CE weapon definition.

Use a 1.7 build of [Combat Extended](https://github.com/CombatExtended-Continued/CombatExtended/tree/Development). The 1.6 Steam build uses different game assemblies. The CE Attachments version used for development declares support through 1.6. RimWorld 1.7 selects its 1.6 content folder automatically.

Extract the packaged ZIP into a single mod directory so that `About/About.xml` sits directly inside it. Enable Harmony, ModularWeapons 2, CE, optionally CE Attachments, and finally this patch. This fork uses the upstream patch's package ID. Enable one copy of the patch in a loadout.

### Build and package

Use the .NET 10 SDK and Python 3. Set `MW2CE_RIMWORLD17_DIR` to the game directory containing `Assembly-CSharp.dll`. By default the project looks under that directory's `Mods` folder for `CombatExtended`, `Harmony`, and `ModularWeapons2`. For Workshop installations, pass the full paths to the relevant 1.7 DLLs with `-p:CombatExtendedReference=...`, `-p:HarmonyReference=...`, and `-p:ModularWeapons2Reference=...`.

From the repository root, build the solution and combine the resulting DLL and XML with the original [Alpha0.3.3 release archive](https://github.com/kaitorisenkou/MW2Patch_CombatExtended/releases/tag/Alpha0.3.3). The packaging script checks the upstream archive's SHA-256 before using its assets.

```powershell
$env:MW2CE_RIMWORLD17_DIR = 'C:\Games\RimWorld'
dotnet build MW2Patch_CombatExtended.sln -c Release
python tools/package_mod.py MW2_CE.zip MW2Patch_CombatExtended/bin/Release/MW2Patch_CombatExtended.dll artifacts/MW2_CE-1.7-phase1.zip
```

### Check attachment placement

With Python and Pillow installed, run the following command from the repository root. Point the three mod arguments to the directories containing each mod's `About` folder.

```powershell
python tools/preview_mounts.py --mw2 'path/to/ModularWeapons2' --ce 'path/to/CombatExtended' --attachments 'path/to/CombatExtendedAttachments' --output artifacts/mount-previews
```

Each sheet shows the original sprite and one XML composite per compatible part, including sights reached through adapters. Inspect the sight base against the rail, the muzzle part against the barrel end, and the side device against the body. The script flags disconnected layers. A sight buried in the receiver can still pass that check.

## If you have suggestions or found a bug/issue
Please report it using one of the following methods:

* Report it on [Issues](https://github.com/kaitorisenkou/ModularWeapons2/issues)
* Report it on [My Discord server](https://discord.gg/XK3Em4eEd3)
* Send pull request to the `master` branch

## Link
[Licence of this mod](https://github.com/kaitorisenkou/MW2Patch_CombatExtended/blob/master/LICENCE)

[ModularWeapons 2](https://github.com/kaitorisenkou/ModularWeapons2)

[Combat Extended](https://github.com/CombatExtended-Continued/CombatExtended)
