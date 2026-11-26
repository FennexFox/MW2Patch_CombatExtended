# MW2Patch_CombatExtended
This mod makes `ModularWeapons 2` compatible with `Combat Extended`.

Load `ModularWeapons 2` BEFORE CE, and load _this mod_ AFTER CE.

## RimWorld 1.6 and 1.7

Phase 1 adds workbench mounts to the CE grenade launcher, disposable rocket launcher, and flare gun. Enabling Combat Extended Attachments adds the AKM-GL, M16A4-GL, KSG, LeMat, Double Badger, Savage 2400, and TP-82. Each weapon keeps its original sprite and CE weapon definition.

Use the CE, ModularWeapons 2 and Harmony assemblies for your game version. The stable 1.6 CE build and the [1.7 CE development build](https://github.com/CombatExtended-Continued/CombatExtended/tree/Development) target different runtimes. Both versions use the same installation ZIP. The game selects its own integration assembly and loads the seven optional weapon patches when CE Attachments is enabled. The tested add-on supplies 1.6 content that RimWorld 1.7 also selects.

Extract the packaged ZIP into a single mod directory so that `About/About.xml` sits directly inside it. Enable Harmony, ModularWeapons 2, CE, optionally CE Attachments, and finally this patch. This fork uses the upstream patch's package ID. Enable one copy of the patch in a loadout.

### Build and package

Use the .NET 10 SDK and Python 3. The Windows 1.6 build targets .NET Framework 4.7.2 and reads game assemblies from `RimWorldWin64_Data/Managed`. The 1.7 build targets .NET 10 and reads game assemblies from the game root. Set each game directory separately. The default mod reference paths assume `CombatExtended`, `ModularWeapons2` and `Harmony` under that game's `Mods` folder. For Workshop installs, pass `-p:CombatExtendedReference=...`, `-p:ModularWeapons2Reference=...` and `-p:HarmonyReference=...` with the matching DLL paths on each build.

Build both targets from the repository root. To build one target with its own reference overrides, pass `-f net472` or `-f net10.0` to `dotnet build`. Download the original [Alpha0.3.3 release archive](https://github.com/kaitorisenkou/MW2Patch_CombatExtended/releases/tag/Alpha0.3.3) as `MW2_CE.zip`. The packager checks its SHA-256, replaces the upstream assembly with the two builds, and includes the shared assets once.

```powershell
$env:MW2CE_RIMWORLD16_DIR = 'C:\Games\RimWorld-1.6'
$env:MW2CE_RIMWORLD17_DIR = 'C:\Games\RimWorld-1.7'
dotnet build MW2Patch_CombatExtended.sln -c Release
python tools/package_mod.py MW2_CE.zip MW2Patch_CombatExtended/bin/Release/net472/MW2Patch_CombatExtended.dll MW2Patch_CombatExtended/bin/Release/net10.0/MW2Patch_CombatExtended.dll artifacts/MW2_CE-phase1.zip
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
