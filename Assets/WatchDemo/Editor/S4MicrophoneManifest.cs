using System.IO;
using System.Xml;
using LighthouseKeepers.Core;
using UnityEditor.Android;

namespace LighthouseKeepers.WatchDemo.Editor
{
    // Permission is requested by CrewVoiceSession after joining a crew, never on solo launch.
    public sealed class S4MicrophoneManifest : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder=>10001;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var tuning=S4Tuning.Current;if(!tuning||!tuning.crewVoice)return;
            var file=Path.Combine(path,"src/main/AndroidManifest.xml");
            var doc=new XmlDocument{PreserveWhitespace=true};doc.Load(file);
            const string ns="http://schemas.android.com/apk/res/android";
            var app=doc.DocumentElement.SelectSingleNode("application");XmlElement entry=null;
            foreach(XmlElement node in app.SelectNodes("meta-data"))
                if(node.GetAttribute("name",ns)=="unityplayer.SkipPermissionsDialog")entry=node;
            if(entry==null){entry=doc.CreateElement("meta-data");entry.SetAttribute("name",ns,"unityplayer.SkipPermissionsDialog");app.AppendChild(entry);}
            entry.SetAttribute("value",ns,"true");doc.Save(file);
        }
    }
}
