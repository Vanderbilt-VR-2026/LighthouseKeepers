"""Regression checks for aapt2 parsing and final APK launch-activity validation."""
import importlib.util
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location("verifier", Path(__file__).parents[1] / "verify_quest_apk.py")
verifier = importlib.util.module_from_spec(spec)
spec.loader.exec_module(verifier)

DUMP = '''N: android=http://schemas.android.com/apk/res/android (line=2)
  E: manifest (line=2)
    A: package="com.lighthousekeepers.game" (Raw: "com.lighthousekeepers.game")
    A: http://schemas.android.com/apk/res/android:versionCode(0x0101021b)=3
      E: uses-permission (line=10)
        A: http://schemas.android.com/apk/res/android:name(0x01010003)="android.permission.INTERNET" (Raw: "android.permission.INTERNET")
      E: uses-feature (line=12)
        A: http://schemas.android.com/apk/res/android:name(0x01010003)="android.hardware.vr.headtracking" (Raw: "android.hardware.vr.headtracking")
        A: http://schemas.android.com/apk/res/android:required(0x0101028e)=true
        A: http://schemas.android.com/apk/res/android:version(0x01010219)=1
      E: application (line=16)
          E: meta-data (line=17)
            A: http://schemas.android.com/apk/res/android:name(0x01010003)="com.oculus.supportedDevices" (Raw: "com.oculus.supportedDevices")
            A: http://schemas.android.com/apk/res/android:value(0x01010024)="quest2|eureka" (Raw: "quest2|eureka")
          E: activity (line=20)
            A: http://schemas.android.com/apk/res/android:name(0x01010003)="com.unity3d.player.UnityPlayerGameActivity" (Raw: "com.unity3d.player.UnityPlayerGameActivity")
              E: meta-data (line=22)
                A: http://schemas.android.com/apk/res/android:name(0x01010003)="com.oculus.vr.focusaware" (Raw: "com.oculus.vr.focusaware")
                A: http://schemas.android.com/apk/res/android:value(0x01010024)=true
              E: intent-filter (line=25)
                  E: action (line=26)
                    A: http://schemas.android.com/apk/res/android:name(0x01010003)="android.intent.action.MAIN" (Raw: "android.intent.action.MAIN")
                  E: category (line=28)
                    A: http://schemas.android.com/apk/res/android:name(0x01010003)="android.intent.category.LAUNCHER" (Raw: "android.intent.category.LAUNCHER")
                  E: category (line=30)
                    A: http://schemas.android.com/apk/res/android:name(0x01010003)="com.oculus.intent.category.VR" (Raw: "com.oculus.intent.category.VR")
'''

class QuestApkVerifierTests(unittest.TestCase):
    def test_binary_tree_preserves_attribute_scope_and_valid_quest_passes(self):
        root = verifier.manifest_tree(DUMP)
        result = verifier.validate_manifest(root, "com.lighthousekeepers.game")
        self.assertEqual(result["supported_devices"], "quest2|eureka")
        self.assertEqual(result["version_code"], "3")
        self.assertEqual(result["launch_activities"], ["com.unity3d.player.UnityPlayerGameActivity"])

    def test_old_flat_apk_marker_omissions_are_rejected(self):
        root = verifier.manifest_tree(DUMP)
        root.remove(root.find("uses-feature"))
        with self.assertRaisesRegex(ValueError, "headtracking"):
            verifier.validate_manifest(root, "com.lighthousekeepers.game")

    def test_vr_category_on_a_different_filter_cannot_mask_flat_launcher(self):
        root = verifier.manifest_tree(DUMP)
        activity = root.find("application/activity")
        intent = activity.find("intent-filter")
        category = intent.find("category[@name='com.oculus.intent.category.VR']")
        intent.remove(category)
        import xml.etree.ElementTree as ET
        ET.SubElement(activity, "intent-filter").append(category)
        with self.assertRaisesRegex(ValueError, "VR category"):
            verifier.validate_manifest(root, "com.lighthousekeepers.game")

    def test_focusaware_must_be_on_launcher_and_correct_package_is_required(self):
        root = verifier.manifest_tree(DUMP)
        activity = root.find("application/activity")
        meta = activity.find("meta-data")
        activity.remove(meta)
        root.find("application").append(meta)
        with self.assertRaisesRegex(ValueError, "focusaware"):
            verifier.validate_manifest(root, "com.lighthousekeepers.game")
        with self.assertRaisesRegex(ValueError, "package id"):
            verifier.validate_manifest(verifier.manifest_tree(DUMP), "wrong.package")

if __name__ == "__main__":
    unittest.main()
