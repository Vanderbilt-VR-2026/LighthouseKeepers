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
        static readonly string[] Names={"keeper_arrival","keeper_wrench","keeper_first_bolt","keeper_generator","keeper_all_done","keeper_hint_pipe","keeper_hint_generator","keeper_hint_power","keeper_hint_beacon","keeper_hint_radio","keeper_hint_bed","keeper_night_two","keeper_night_three","keeper_next_night","keeper_early_bed","keeper_win","keeper_lose"};
        // Mild denoising; trim only the two ends (reverse twice); EBU loudness normalization.
        public const string Filter="afftdn=nr=6:nf=-40,silenceremove=start_periods=1:start_duration=0.02:start_threshold=-45dB:start_silence=0.04,areverse,silenceremove=start_periods=1:start_duration=0.02:start_threshold=-45dB:start_silence=0.08,areverse,loudnorm=I=-18:TP=-1.5:LRA=7";
        public static string FindFfmpeg()
        {
            var configured=System.Environment.GetEnvironmentVariable("LK_FFMPEG");if(!string.IsNullOrEmpty(configured)&&File.Exists(configured))return configured;
            foreach(var directory in (System.Environment.GetEnvironmentVariable("PATH")??"").Split(Path.PathSeparator).Concat(new[]{"/opt/homebrew/bin","/usr/local/bin"}))
            {var p=Path.Combine(directory,Application.platform==RuntimePlatform.WindowsEditor?"ffmpeg.exe":"ffmpeg");if(File.Exists(p))return p;}
            throw new FileNotFoundException("FFmpeg is required. Install it (macOS: brew install ffmpeg), then restart Unity, or set LK_FFMPEG to its executable path.");
        }
        // Arguments are passed directly to a process, never evaluated by a shell.
        static string Quote(string value)=>"\""+value.Replace("\\","\\\\").Replace("\"","\\\"")+"\"";
        public static void Convert(string executable,string source,string destination)
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
                int count=0;var ignored=new System.Collections.Generic.List<string>();
                foreach(var file in files)
                {
                    var name=Path.GetFileNameWithoutExtension(file).ToLowerInvariant();if(!Names.Contains(name)){ignored.Add(Path.GetFileName(file));continue;}
                    if(EditorUtility.DisplayCancelableProgressBar("Import radio recordings",name,(float)count/Mathf.Max(1,files.Length)))break;
                    string destination="Assets/Audio/Radio/"+name+".wav";Convert(executable,file,destination);
                    AssetDatabase.ImportAsset(destination,ImportAssetOptions.ForceUpdate);count++;
                }
                string message=$"Imported {count} radio recordings. Mono 44.1 kHz; trimmed, gently denoised and normalized. Unity references preserved.";
                if(ignored.Count>0)message+="\nUnrecognized filenames (unchanged): "+string.Join(", ",ignored);
                Debug.Log(message);EditorUtility.DisplayDialog("Radio recordings",message,"OK");
            }
            catch(Exception e){Debug.LogError(e.Message);EditorUtility.DisplayDialog("Radio import needs attention",e.Message,"OK");}
            finally{EditorUtility.ClearProgressBar();}
        }
    }
}
