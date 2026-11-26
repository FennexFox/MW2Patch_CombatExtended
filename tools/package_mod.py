"""Assemble the dual-version mod from the upstream assets, version-specific DLLs and this fork's patches."""

import argparse
import hashlib
from pathlib import Path
from tempfile import TemporaryDirectory
from zipfile import ZIP_DEFLATED, ZipFile


ROOT = Path(__file__).resolve().parents[1]
UPSTREAM_SHA256 = "21926b847cfa3929c843c538b846e504e2a82a7e38c1e3219ffba7de4c33001f"
DLL = "Common/Assemblies/MW2Patch_CombatExtended.dll"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("upstream", type=Path, help="Original MW2_CE.zip from Alpha0.3.3")
    parser.add_argument("assembly16", type=Path, help="Integration DLL built for net472")
    parser.add_argument("assembly17", type=Path, help="Integration DLL built for net10.0")
    parser.add_argument("output", type=Path, help="Destination mod ZIP")
    args = parser.parse_args()
    if hashlib.sha256(args.upstream.read_bytes()).hexdigest() != UPSTREAM_SHA256:
        parser.error("Expected the original Alpha0.3.3 release archive.")
    if args.output.resolve() in {p.resolve() for p in (args.upstream, args.assembly16, args.assembly17)}:
        parser.error("The output must be distinct from the input files.")
    with ZipFile(args.upstream) as upstream:
        files = {name: upstream.read(name) for name in upstream.namelist() if not name.endswith("/")}
    original = files.pop(DLL)
    for version, path, framework in (("1.6", args.assembly16, b".NETFramework,Version=v4.7.2"),
                                     ("1.7", args.assembly17, b".NETCoreApp,Version=v10.0")):
        assembly = path.read_bytes()
        if assembly == original or framework not in assembly or b"MW2Patch_CombatExtended" not in assembly:
            parser.error(f"Expected a rebuilt RimWorld {version} integration assembly at {path}.")
        files[f"{version}/Assemblies/MW2Patch_CombatExtended.dll"] = assembly
    for name in ("About/About.xml", "loadFolders.xml"):
        files[name] = (ROOT / "packaging" / name).read_bytes()
    for folder in ("Common", "CEAttachments"):
        for patch in (ROOT / folder).rglob("*.xml"):
            files[patch.relative_to(ROOT).as_posix()] = patch.read_bytes()
    files["LICENCE"] = (ROOT / "LICENCE").read_bytes()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with TemporaryDirectory(dir=args.output.parent) as directory:
        archive = Path(directory) / "mod.zip"
        with ZipFile(archive, "w", ZIP_DEFLATED) as output:
            for name, content in sorted(files.items()):
                output.writestr(name, content)
        archive.replace(args.output)


if __name__ == "__main__":
    main()
