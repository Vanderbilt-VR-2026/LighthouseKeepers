using System.Xml;
using LighthouseKeepers.Editor.QuestBuild;
using NUnit.Framework;
using UnityEditor.Build;

namespace LighthouseKeepers.Tests
{
    public class QuestManifestTests
    {
        const string Android = "http://schemas.android.com/apk/res/android";
        static XmlDocument Manifest(string activity)
        {
            var document = new XmlDocument();
            document.LoadXml("<manifest xmlns:android='" + Android + "'><application><activity android:name='" + activity +
                "'><intent-filter><action android:name='android.intent.action.MAIN'/><category android:name='android.intent.category.LAUNCHER'/>" +
                "</intent-filter></activity><activity android:name='unrelated.ServiceActivity'/></application></manifest>");
            return document;
        }
        [TestCase("com.unity3d.player.UnityPlayerActivity")]
        [TestCase("com.unity3d.player.UnityPlayerGameActivity")]
        public void RepairsFlatManifestAndLeavesOtherActivityAlone(string activity)
        {
            var document = Manifest(activity);
            Assert.That(QuestManifest.Ensure(document, "quest2|eureka"), Is.True);
            var tracking = (XmlElement)document.SelectSingleNode("/manifest/uses-feature");
            Assert.That(tracking.GetAttribute("name", Android), Is.EqualTo("android.hardware.vr.headtracking"));
            Assert.That(tracking.GetAttribute("required", Android), Is.EqualTo("true"));
            Assert.That(tracking.GetAttribute("version", Android), Is.EqualTo("1"));
            var ns = new XmlNamespaceManager(document.NameTable); ns.AddNamespace("a", Android);
            Assert.That(document.SelectSingleNode("/manifest/application/activity[1]/intent-filter/category[@a:name='com.oculus.intent.category.VR']", ns), Is.Not.Null);
            Assert.That(((XmlElement)document.SelectSingleNode("/manifest/application/activity[1]/meta-data")).GetAttribute("value", Android), Is.EqualTo("true"));
            Assert.That(((XmlElement)document.SelectSingleNode("/manifest/application/meta-data")).GetAttribute("value", Android), Is.EqualTo("quest2|eureka"));
            Assert.That(document.SelectSingleNode("/manifest/application/activity[2]").ChildNodes.Count, Is.Zero);
            Assert.That(document.SelectSingleNode("/manifest/uses-permission[@a:name='android.permission.INTERNET']", ns), Is.Not.Null);
            var once = document.OuterXml;
            Assert.That(QuestManifest.Ensure(document, "quest2|eureka"), Is.False);
            Assert.That(document.OuterXml, Is.EqualTo(once));
        }
        [Test]
        public void RejectsUnsupportedOrMissingLaunchActivityWithoutMutating()
        {
            var document = Manifest("unexpected.FlatActivity");
            var original = document.OuterXml;
            Assert.Throws<BuildFailedException>(() => QuestManifest.Ensure(document, "eureka"));
            Assert.That(document.OuterXml, Is.EqualTo(original));
            document.SelectSingleNode("/manifest/application/activity[1]/intent-filter").RemoveAll();
            Assert.Throws<BuildFailedException>(() => QuestManifest.Ensure(document, "eureka"));
        }
        [Test]
        public void RepairsWrongValuesAndRefreshesConfiguredDevices()
        {
            var document = Manifest("com.unity3d.player.UnityPlayerGameActivity");
            QuestManifest.Ensure(document, "quest2");
            ((XmlElement)document.SelectSingleNode("/manifest/uses-feature")).SetAttribute("required", Android, "false");
            Assert.That(QuestManifest.Ensure(document, "eureka"), Is.True);
            Assert.That(((XmlElement)document.SelectSingleNode("/manifest/uses-feature")).GetAttribute("required", Android), Is.EqualTo("true"));
            Assert.That(((XmlElement)document.SelectSingleNode("/manifest/application/meta-data")).GetAttribute("value", Android), Is.EqualTo("eureka"));
        }
    }
}
