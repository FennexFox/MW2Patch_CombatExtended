"""Draw a source-sprite contact sheet for every Phase 1 weapon. Requires Pillow."""

import argparse
import math
from pathlib import Path
from xml.etree import ElementTree as ET

from PIL import Image, ImageChops, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parents[1]
CELL = (450, 224)
QUAD = 250


def vector(node, tag, default):
    values = node.findtext(tag, default).strip("()").split(",")
    return float(values[0]), float(values[-1])


def rotate(x, y, clockwise):
    a = math.radians(clockwise)
    return x * math.cos(a) + y * math.sin(a), -x * math.sin(a) + y * math.cos(a)


def adapter_paths(node, mounts, ancestors=()):
    mount = node.findtext("mountDef")
    path = (*ancestors, node)
    yield mount, path
    if node.findtext("allowMoreAdapter", "true").lower() == "true":
        for child in mounts[mount].findall("canAdaptAs/li"):
            yield from adapter_paths(child, mounts, path)


def part_layers(path, part):
    # MW2 places the part using the total rotation. Each adapter graphic uses
    # the cumulative transform of its parents. Descendant adapters draw first.
    final_angle = sum(float(n.findtext("rotation", "0")) for n in path)
    ax = ay = px = py = angle = 0.0
    sx = sy = 1.0
    adapters = []
    order = int(path[0].findtext("layerOrder", "0"))
    for depth, node in enumerate(path):
        x, y = vector(node, "offset", "0,0")
        dx, dy = rotate(x, y, angle)
        ax, ay = ax + dx, ay + dy
        dx, dy = rotate(x, y, final_angle if depth else 0.0)
        px, py = px + dx, py + dy
        width, height = vector(node, "scale", "1,1")
        sx, sy = sx * width, sy * height
        angle += float(node.findtext("rotation", "0"))
        texture = node.findtext("adapterGraphic/texPath")
        if texture:
            adapters.append((order, texture, ax, ay, sx, sy, angle))
    return [*reversed(adapters), (order, part.findtext("graphicData/texPath"),
                                  px, py, sx, sy, final_angle)]


def variants(component, mounts, parts):
    yield "Original", []
    for mount in component.findall("partsMounts/li"):
        paths = dict(adapter_paths(mount, mounts))
        for part, attached_to in parts:
            path = paths.get(attached_to)
            if path:
                yield part.findtext("defName"), part_layers(path, part)


def all_layers_touch(masks):
    """Check for a path of opaque pixels from each layer to the weapon."""
    joined = masks[0]
    remaining = masks[1:]
    while remaining:
        nearby = joined.filter(ImageFilter.MaxFilter(3))
        touching = [m for m in remaining if ImageChops.multiply(m, nearby).getbbox()]
        if not touching:
            return False
        remaining = [m for m in remaining if all(m is not other for other in touching)]
        for mask in touching:
            joined = ImageChops.lighter(joined, mask)
    return True


def draw_sheet(component, options, texture_dirs, textures):
    width, height = CELL
    sheet = Image.new("RGB", (width * 3, height * math.ceil(len(options) / 3)), "#c5c9c1")
    labels = ImageDraw.Draw(sheet)
    base = (0, component.findtext("baseGraphicData/texPath"), 0, 0, 1, 1, 0)
    detached = []
    for index, (label, layers) in enumerate(options):
        cell = Image.new("RGBA", CELL)
        masks = []
        for _, name, x, y, sx, sy, angle in sorted([base, *layers], key=lambda layer: layer[0]):
            if name not in textures:
                filename = next((d / (name + ".png") for d in texture_dirs
                                 if (d / (name + ".png")).is_file()), None)
                if filename is None:
                    raise FileNotFoundError(f"Could not resolve texPath {name}")
                with Image.open(filename) as source:
                    textures[name] = source.convert("RGBA")
            size = round(QUAD * abs(sx)), round(QUAD * abs(sy))
            sprite = textures[name].resize(size, Image.Resampling.LANCZOS)
            if sx < 0:
                sprite = sprite.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
            if sy < 0:
                sprite = sprite.transpose(Image.Transpose.FLIP_TOP_BOTTOM)
            if angle:
                sprite = sprite.rotate(-angle, Image.Resampling.BICUBIC, expand=True)
            left = round(width / 2 + QUAD * x - sprite.width / 2)
            top = round(height / 2 - QUAD * y - sprite.height / 2)
            cell.alpha_composite(sprite, (left, top))
            mask = Image.new("L", CELL)
            mask.paste(sprite.getchannel("A").point(lambda a: 255 if a >= 128 else 0), (left, top))
            masks.append(mask)
        origin = (index % 3 * width, index // 3 * height)
        sheet.paste(cell, origin, cell)
        labels.text((origin[0] + 8, origin[1] + 5), label, fill="#20231e")
        if not all_layers_touch(masks):
            detached.append(label)
    return sheet, detached


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--mw2", type=Path, required=True, help="ModularWeapons 2 mod directory")
    parser.add_argument("--ce", type=Path, required=True, help="Combat Extended mod directory")
    parser.add_argument("--attachments", type=Path, required=True, help="CE Attachments mod directory")
    parser.add_argument("--patch-root", type=Path, default=ROOT)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    common = args.mw2 / "Common"
    mounts, definitions = {}, []
    for filename in sorted((common / "Defs" / "CommonParts").glob("*.xml")):
        for node in ET.parse(filename).getroot():
            if node.tag == "ModularWeapons2.ModularPartsMountDef":
                mounts[node.findtext("defName")] = node
            if node.tag == "ModularWeapons2.ModularPartsDef":
                definitions.append(node)
    named = {d.attrib["Name"]: d for d in definitions if "Name" in d.attrib}
    parts = []
    for node in definitions:
        if node.get("Abstract", "").lower() == "true" or not node.findtext("graphicData/texPath"):
            continue
        parent = node
        while parent is not None and not parent.findtext("attachedTo"):
            parent = named.get(parent.get("ParentName"))
        if parent is not None:
            parts.append((node, parent.findtext("attachedTo")))
    textures = {}
    texture_dirs = [p / "Textures" for p in (common, args.ce, args.attachments)]
    args.output.mkdir(parents=True, exist_ok=True)
    for folder in ("Common/Patches/Weapons/CECore", "CEAttachments/Patches/Weapons"):
        for filename in sorted((args.patch_root / folder).glob("*.xml")):
            component = ET.parse(filename).find(".//li[@Class='ModularWeapons2.CompProperties_ModularWeapon']")
            options = list(variants(component, mounts, parts))
            sheet, detached = draw_sheet(component, options, texture_dirs, textures)
            sheet.save(args.output / (filename.stem + ".png"))
            print(f"{filename.stem}: {len(options) - 1} mounted variants")
            if detached:
                print("  Inspect disconnected layers: " + ", ".join(detached))


if __name__ == "__main__":
    main()
