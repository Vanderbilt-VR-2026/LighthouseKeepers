#!/usr/bin/env python3
"""Verify the built APK, rather than trusting source XR settings. No device claims."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
import zipfile


def manifest_tree(dump):
    """Parse aapt2 xmltree indentation so VR tags on a different activity cannot pass."""
    stack = []
    root = None
    for line in dump.splitlines():
        stripped = line.lstrip()
        depth = len(line) - len(stripped)
        if stripped.startswith("E: "):
            element = ET.Element(stripped[3:].split()[0])
            while stack and stack[-1][0] >= depth:
                stack.pop()
            if stack:
                stack[-1][1].append(element)
            else:
                root = element
            stack.append((depth, element))
        elif stripped.startswith("A: ") and stack:
            name, value = stripped[3:].split("=", 1)
            name = name.split("(")[0].rsplit(":", 1)[-1]
            raw = re.search(r'\(Raw: (".*")\)$', value)
            if raw:
                value = json.loads(raw.group(1))
            elif value.startswith('"'):
                value = json.loads(value)
            stack[-1][1].set(name, value)
    if root is None or root.tag != "manifest":
        raise ValueError("aapt2 did not return an Android manifest")
    return root


def validate_manifest(root, package):
    def require(condition, reason):
        if not condition:
            raise ValueError(reason)
    require(root.get("package") == package, "APK package id differs from expected application id")
    tracking = root.find("uses-feature[@name='android.hardware.vr.headtracking']")
    require(tracking is not None and tracking.get("required") == "true" and
            tracking.get("version") == "1", "Missing required Quest headtracking version 1")
    require(root.find("uses-permission[@name='android.permission.INTERNET']") is not None,
            "Missing multiplayer Internet permission")
    application = root.find("application")
    require(application is not None, "Missing application")
    devices = application.find("meta-data[@name='com.oculus.supportedDevices']")
    require(devices is not None and bool(devices.get("value", "").strip()), "Missing Quest supportedDevices")
    launchers = []
    for activity in application.findall("activity"):
        for intent in activity.findall("intent-filter"):
            if (intent.find("action[@name='android.intent.action.MAIN']") is not None and
                    intent.find("category[@name='android.intent.category.LAUNCHER']") is not None):
                require(activity.get("name") in {"com.unity3d.player.UnityPlayerActivity",
                        "com.unity3d.player.UnityPlayerGameActivity"}, "Unexpected Android launcher activity")
                require(intent.find("category[@name='com.oculus.intent.category.VR']") is not None,
                        "Unity launcher is missing the VR category (flat Android launch risk)")
                focus = activity.find("meta-data[@name='com.oculus.vr.focusaware']")
                require(focus is not None and focus.get("value") == "true", "Unity launcher is missing focusaware=true")
                launchers.append(activity.get("name"))
    require(bool(launchers), "No Unity MAIN/LAUNCHER activity")
    return {"package": package, "version_name": root.get("versionName"),
            "version_code": root.get("versionCode"), "supported_devices": devices.get("value"),
            "launch_activities": launchers}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("apk", type=Path)
    parser.add_argument("--build-tools", type=Path, required=True, help="Android SDK build-tools/version directory")
    parser.add_argument("--java-home", type=Path, help="JDK root for apksigner; Unity AndroidPlayer/OpenJDK works")
    parser.add_argument("--package", default="com.lighthousekeepers.game")
    parser.add_argument("--source-record", type=Path, help="Successful Unity build JSON to include in the verification evidence")
    args = parser.parse_args()
    apk = args.apk.resolve(strict=True)
    env = os.environ.copy()
    if args.java_home:
        env["JAVA_HOME"] = str(args.java_home.resolve(strict=True))
    def run(tool, *arguments):
        suffix = ".bat" if os.name == "nt" and tool == "apksigner" else ".exe" if os.name == "nt" else ""
        return subprocess.run([str(args.build_tools / (tool + suffix)), *map(str, arguments)],
                              env=env, check=True, capture_output=True, text=True).stdout
    # Do not leave a previous passing sidecar beside an APK that now fails verification.
    output = apk.with_suffix(".verification.json")
    output.unlink(missing_ok=True)
    with zipfile.ZipFile(apk) as archive:
        bad = archive.testzip()
        if bad:
            raise ValueError("ZIP CRC failure: " + bad)
        names = set(archive.namelist())
        required = {"lib/arm64-v8a/libopenxr_loader.so", "lib/arm64-v8a/libUnityOpenXR.so",
                    "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so", "assets/bin/Data/boot.config"}
        if not required.issubset(names):
            raise ValueError("APK is missing ARM64 OpenXR/IL2CPP player files: " + str(sorted(required - names)))
    result = validate_manifest(manifest_tree(run("aapt2", "dump", "xmltree", "--file", "AndroidManifest.xml", apk)), args.package)
    signer = run("apksigner", "verify", "--verbose", "--print-certs", apk)
    result.update({"apk": str(apk), "bytes": apk.stat().st_size,
                   "sha256": sha256_file(apk),
                   "zip_crc": "passed", "signature": signer.strip(), "headset_execution": "not performed"})
    if args.source_record:
        source = json.loads(args.source_record.read_text())
        if source.get("result") != "Succeeded" or source.get("errors") != 0:
            raise ValueError("Source record is not a successful zero-error Unity build")
        if Path(source.get("apk", "")).resolve() != apk:
            raise ValueError("Source record refers to another APK")
        result["unity_build"] = source
    output.write_text(json.dumps(result, indent=2) + "\n")
    print(json.dumps(result, indent=2))
    print("Quest packaging checks passed; headset launch and two-device acceptance remain separate.")


def sha256_file(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


if __name__ == "__main__":
    try:
        main()
    except (ValueError, OSError, subprocess.CalledProcessError, zipfile.BadZipFile) as error:
        print("Quest APK verification FAILED: " + str(error), file=sys.stderr)
        if isinstance(error, subprocess.CalledProcessError):
            print(error.stdout + error.stderr, file=sys.stderr)
        sys.exit(1)
