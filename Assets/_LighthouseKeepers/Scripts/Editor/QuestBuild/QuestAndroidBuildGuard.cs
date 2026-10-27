using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

namespace LighthouseKeepers.Editor.QuestBuild
{
    // Android builds in this project are Quest players. Fail before building a flat Android APK.
    public sealed class QuestAndroidBuildGuard : IPreprocessBuildWithReport, IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 10000;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android) ValidateSettings();
        }

        public static string ValidateSettings()
        {
            var general = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (general == null || !general.InitManagerOnStart || general.Manager == null ||
                !general.Manager.activeLoaders.Any(loader => loader is OpenXRLoader))
                throw new BuildFailedException("Quest requires Android OpenXR and Initialize XR on Startup in XR Plug-in Management.");
            // Do not change automaticLoading/automaticRunning: XRGeneralSettings manages that lifecycle.
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var quest = settings != null ? settings.GetFeature<MetaQuestFeature>() : null;
            if (quest == null || !quest.enabled)
                throw new BuildFailedException("Enable Meta Quest Support in the Android OpenXR settings.");
            var touch = settings.GetFeature<OculusTouchControllerProfile>();
            if (touch == null || !touch.enabled)
                throw new BuildFailedException("Enable the Oculus Touch Controller interaction profile for the lobby and gameplay controls.");
            if (quest.ForceRemoveInternetPermission)
                throw new BuildFailedException("Quest Support must retain Internet permission for the two-player lobby.");
            if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0 ||
                PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP)
                throw new BuildFailedException("Quest requires ARM64 and the IL2CPP scripting backend.");
            // targetDevices is package-internal. Read its serialized settings instead of mutating
            // package source or hard-coding a different device list from the Inspector.
            var devices = new SerializedObject(quest).FindProperty("targetDevices");
            if (devices == null || !devices.isArray)
                throw new BuildFailedException("Cannot read Meta Quest target devices; review the installed OpenXR package API.");
            var supported = new List<string>();
            for (var i = 0; i < devices.arraySize; i++)
            {
                var device = devices.GetArrayElementAtIndex(i);
                if (device.FindPropertyRelative("enabled").boolValue)
                    supported.Add(device.FindPropertyRelative("manifestName").stringValue);
            }
            if (supported.Count == 0 || supported.Any(string.IsNullOrWhiteSpace))
                throw new BuildFailedException("Select at least one Meta Quest target device in Android OpenXR settings.");
            return string.Join("|", supported.Distinct());
        }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            var manifest = new XmlDocument { PreserveWhitespace = true };
            manifest.Load(manifestPath);
            // Run after the package hooks. Existing valid markers are left alone. This protects
            // both Activity entry types against the previously published APK's missing VR tags.
            if (QuestManifest.Ensure(manifest, ValidateSettings())) manifest.Save(manifestPath);
        }
    }

    public static class QuestManifest
    {
        const string Android = "http://schemas.android.com/apk/res/android";
        public static bool Ensure(XmlDocument document, string devices)
        {
            if (string.IsNullOrWhiteSpace(devices)) throw new BuildFailedException("No Quest target devices.");
            var root = document.DocumentElement;
            var application = root?.SelectSingleNode("application") as XmlElement;
            if (root == null || root.Name != "manifest" || application == null)
                throw new BuildFailedException("Invalid generated Android manifest.");
            var launchers = application.SelectNodes("activity").Cast<XmlElement>()
                .SelectMany(activity => activity.SelectNodes("intent-filter").Cast<XmlElement>()
                    .Where(filter => Has(filter, "action", "android.intent.action.MAIN") &&
                                     Has(filter, "category", "android.intent.category.LAUNCHER"))
                    .Select(filter => (activity, filter))).ToArray();
            if (launchers.Length == 0)
                throw new BuildFailedException("Generated manifest has no MAIN/LAUNCHER Unity activity; cannot guarantee a Quest VR launch.");
            foreach (var launcher in launchers)
            {
                var name = launcher.activity.GetAttribute("name", Android);
                if (name != "com.unity3d.player.UnityPlayerActivity" && name != "com.unity3d.player.UnityPlayerGameActivity")
                    throw new BuildFailedException("Unsupported launch activity: " + name + ". Review Quest manifest handling before shipping.");
            }
            var changed = false;
            var tracking = Named(root, "uses-feature", "android.hardware.vr.headtracking", ref changed);
            changed |= Set(tracking, "required", "true");
            changed |= Set(tracking, "version", "1");
            changed |= Set(Named(application, "meta-data", "com.oculus.supportedDevices", ref changed), "value", devices);
            foreach (var launcher in launchers)
            {
                changed |= Set(Named(launcher.activity, "meta-data", "com.oculus.vr.focusaware", ref changed), "value", "true");
                Named(launcher.filter, "category", "com.oculus.intent.category.VR", ref changed);
            }
            Named(root, "uses-permission", "android.permission.INTERNET", ref changed);
            return changed;
        }
        static bool Has(XmlElement parent, string tag, string name) =>
            parent.SelectNodes(tag).Cast<XmlElement>().Any(e => e.GetAttribute("name", Android) == name);
        static XmlElement Named(XmlElement parent, string tag, string name, ref bool changed)
        {
            var matches = parent.SelectNodes(tag).Cast<XmlElement>().Where(e => e.GetAttribute("name", Android) == name).ToArray();
            if (matches.Length > 1) throw new BuildFailedException("Duplicate Quest manifest element: " + name);
            if (matches.Length == 1) return matches[0];
            var element = parent.OwnerDocument.CreateElement(tag);
            element.SetAttribute("name", Android, name);
            parent.AppendChild(element);
            changed = true;
            return element;
        }
        static bool Set(XmlElement element, string name, string value)
        {
            if (element.GetAttribute(name, Android) == value) return false;
            element.SetAttribute(name, Android, value);
            return true;
        }
    }
}
