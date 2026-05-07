#!/usr/bin/env python3
# /// script
# requires-python = ">=3.9"
# ///
"""Prepend a new version entry to manifest.json and emby-packages.xml.

Run from the repo root. Stdlib-only (json + xml.etree.ElementTree).

Usage: update-manifests.py <release-version> <jellyfin-md5> <emby-md5> [<iso-timestamp>]
"""

import json
import sys
from datetime import datetime, timezone
from pathlib import Path
from xml.etree import ElementTree as ET


JF_URL_TEMPLATE = (
    "https://github.com/Tracearr/mediaserver-sse/releases/download/"
    "v{v}/Tracearr.Sse.Jellyfin_{v}.zip"
)
EMBY_URL_TEMPLATE = (
    "https://github.com/Tracearr/mediaserver-sse/releases/download/"
    "v{v}/Tracearr.Sse.Emby_{v}.zip"
)
TARGET_ABI = "10.11.0.0"


def main(argv):
    if len(argv) < 4 or len(argv) > 5:
        sys.exit(
            f"usage: {argv[0]} <release-version> <jellyfin-md5> <emby-md5> "
            "[<iso-timestamp>]"
        )

    release_version = argv[1]
    jf_md5 = argv[2]
    emby_md5 = argv[3]
    timestamp = (
        argv[4]
        if len(argv) == 5
        else datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    )

    assembly_version = f"{release_version}.0"
    jf_url = JF_URL_TEMPLATE.format(v=release_version)
    emby_url = EMBY_URL_TEMPLATE.format(v=release_version)

    update_manifest_json(assembly_version, jf_md5, jf_url, timestamp)
    update_emby_xml(assembly_version, emby_md5, emby_url, timestamp)

    print(
        f"updated manifest.json and emby-packages.xml for version {release_version}"
    )


def update_manifest_json(version, checksum, source_url, timestamp):
    path = Path("manifest.json")
    data = json.loads(path.read_text())
    new_entry = {
        "version": version,
        "changelog": "See GitHub release notes.",
        "targetAbi": TARGET_ABI,
        "sourceUrl": source_url,
        "checksum": checksum,
        "timestamp": timestamp,
    }
    data[0]["versions"] = [new_entry] + data[0]["versions"]
    path.write_text(json.dumps(data, indent=2) + "\n")


def update_emby_xml(version, checksum, source_url, timestamp):
    path = Path("emby-packages.xml")
    tree = ET.parse(path)
    root = tree.getroot()
    versions = root.find(".//PackageInfo/versions")
    if versions is None:
        sys.exit("emby-packages.xml: <PackageInfo>/<versions> not found")

    new_version = ET.Element("version")
    fields = [
        ("versionStr", version),
        ("classification", "Release"),
        ("description", "See GitHub release notes."),
        ("sourceUrl", source_url),
        ("checksum", checksum),
        ("runtimes", "netstandard"),
        ("timestamp", timestamp),
    ]
    for tag, text in fields:
        sub = ET.SubElement(new_version, tag)
        sub.text = text

    versions.insert(0, new_version)

    ET.indent(tree, space="  ")
    tree.write(path, encoding="utf-8", xml_declaration=True)
    with path.open("ab") as f:
        f.write(b"\n")


if __name__ == "__main__":
    main(sys.argv)
