using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug=UnityEngine.Debug;
namespace LighthouseKeepers.WatchDemo.Editor
{
    public static class ImportRadioRecordings
    {
        [Serializable] sealed class Mapping { public HarborVoiceEntry[] entries; }
        static HarborVoiceEntry[] Entries=>JsonUtility.FromJson<Mapping>(File.ReadAllText("Assets/WatchDemo/Resources/HarborVoiceTiming.json")).entries;
        // Same gentle voice chain as Tools/VoicePass/import_clips.py. Trim ends only, retaining room tone.
        public const string Filter="highpass=f=80,afftdn=nr=6:nf=-40,deesser=i=0.15:m=0.35:f=0.5,equalizer=f=2800:t=q:w=0.8:g=2.5,silenceremove=start_periods=1:start_duration=0.02:start_threshold=-45dB:start_silence=0.08,areverse,silenceremove=start_periods=1:start_duration=0.02:start_threshold=-45dB:start_silence=0.08,areverse,acompressor=threshold=0.125:ratio=2:attack=10:release=80,loudnorm=I=-18:TP=-1.5:LRA=7";
        public static string FindFfmpeg()
        {
            var configured=System.Environment.GetEnvironmentVariable("LK_FFMPEG");if(!string.IsNullOrEmpty(configured)&&File.Exists(configured))return configured;
            foreach(var directory in (System.Environment.GetEnvironmentVariable("PATH")??"").Split(Path.PathSeparator).Concat(new[]{"/opt/homebrew/bin","/usr/local/bin",Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),".local/bin")}))
            {var p=Path.Combine(directory,Application.platform==RuntimePlatform.WindowsEditor?"ffmpeg.exe":"ffmpeg");if(File.Exists(p))return p;}
            throw new FileNotFoundException("FFmpeg is required. Install it (macOS: brew install ffmpeg), then restart Unity, or set LK_FFMPEG to its executable path.");
        }
        // Arguments are passed directly to a process, never evaluated by a shell.
        static string Quote(string value)=>"\""+value.Replace("\\","\\\\").Replace("\"","\\\"")+"\"";
        public static void Convert(string executable,string source,string destination,bool reportedLongTake=false)
        {
            string temporary=destination+".importing.wav";
            try
            {
                var start=new ProcessStartInfo(executable,"-hide_banner -loglevel error -nostdin -y -i "+Quote(Path.GetFullPath(source))+" -vn -af "+Quote(Filter)+" -ac 1 -ar 44100 -c:a pcm_s16le "+Quote(Path.GetFullPath(temporary)))
                {UseShellExecute=false,CreateNoWindow=true,RedirectStandardError=true};
                var errors=new StringBuilder();using var process=new Process{StartInfo=start};
                process.ErrorDataReceived+=(s,e)=>{if(e.Data!=null)lock(errors)errors.AppendLine(e.Data);};
                process.Start();process.BeginErrorReadLine();
                if(!process.WaitForExit(120000)){process.Kill();throw new TimeoutException("Audio conversion exceeded two minutes: "+Path.GetFileName(source));}
                process.WaitForExit();if(process.ExitCode!=0)throw new InvalidOperationException(errors.ToString());
                if(!File.Exists(temporary)||new FileInfo(temporary).Length<9000)throw new InvalidDataException("No usable speech remains in "+Path.GetFileName(source));
                using(var reader=new BinaryReader(File.OpenRead(temporary)))
                {
                    reader.BaseStream.Position=12;int bytesPerSecond=0;
                    while(reader.BaseStream.Position+8<=reader.BaseStream.Length)
                    {
                        var tag=new string(reader.ReadChars(4));int size=reader.ReadInt32();long end=reader.BaseStream.Position+size;
                        if(tag=="fmt "){reader.ReadInt16();reader.ReadInt16();reader.ReadInt32();bytesPerSecond=reader.ReadInt32();}
                        if(tag=="data"&&bytesPerSecond>0&&(double)size/bytesPerSecond>=10&&!reportedLongTake)
                            throw new InvalidDataException("Select one take first (under 10 seconds): "+Path.GetFileName(source)+". Use Tools/S4/process_radio_takes.py for three-take recordings.");
                        reader.BaseStream.Position=end+size%2;
                    }
                }
                // Stage completely before replacing; leave .meta files untouched.
                File.Copy(temporary,destination,true);
            }
            finally{if(File.Exists(temporary))File.Delete(temporary);}
        }
        [MenuItem("Lighthouse Keepers/Import Radio Recordings")]
        public static void Import()
        {
            try
            {
                var executable=FindFfmpeg();string source=Path.GetFullPath("Recordings/Radio");Directory.CreateDirectory(source);
                var files=Directory.GetFiles(source).Where(p=>new[]{".wav",".m4a",".mp3"}.Contains(Path.GetExtension(p).ToLowerInvariant())).ToArray();
                var groups=files.GroupBy(Path.GetFileNameWithoutExtension,StringComparer.OrdinalIgnoreCase).ToArray();
                var duplicates=groups.Where(g=>g.Count()>1).Select(g=>g.Key).ToArray();
                if(duplicates.Length>0)throw new InvalidOperationException("Keep only one take per name before import: "+string.Join(", ",duplicates));
                var mapped=Entries;int count=0;var ignored=new System.Collections.Generic.List<string>();
                foreach(var file in files)
                {
                    var name=Path.GetFileNameWithoutExtension(file).ToLowerInvariant();var entry=mapped.FirstOrDefault(e=>Path.GetFileNameWithoutExtension(e.file)==name);if(entry==null){ignored.Add(Path.GetFileName(file));continue;}
                    if(EditorUtility.DisplayCancelableProgressBar("Import radio recordings",name,(float)count/Mathf.Max(1,files.Length)))break;
                    string destination=entry.file;Convert(executable,file,destination,entry.longSingleTake);
                    if(entry.longSingleTake)Debug.LogWarning("Reported long single take: "+name+". Review duration; do not speed speech up.");
                    // A replacement performance has new timing. Use the even-reveal fallback until aligned.
                    entry.wordStarts=null;entry.characterEnds=null;
                    AssetDatabase.ImportAsset(destination,ImportAssetOptions.ForceUpdate);
                    entry.duration=AssetDatabase.LoadAssetAtPath<AudioClip>(destination).length;
                    AssetDatabase.ImportAsset(destination,ImportAssetOptions.ForceUpdate);count++;
                }
                File.WriteAllText("Assets/WatchDemo/Resources/HarborVoiceTiming.json",JsonUtility.ToJson(new Mapping{entries=mapped},true)+"\n");
                AssetDatabase.ImportAsset("Assets/WatchDemo/Resources/HarborVoiceTiming.json");HarborVoiceTiming.Invalidate();
                string message=$"Imported {count} radio recordings. Mono 44.1 kHz; trimmed, gently denoised and normalized. Unity references preserved.";
                if(ignored.Count>0)message+="\nUnrecognized filenames (unchanged): "+string.Join(", ",ignored);
                Debug.Log(message);EditorUtility.DisplayDialog("Radio recordings",message,"OK");
            }
            catch(Exception e){Debug.LogError(e.Message);EditorUtility.DisplayDialog("Radio import needs attention",e.Message,"OK");}
            finally{EditorUtility.ClearProgressBar();}
        }
    }
}
