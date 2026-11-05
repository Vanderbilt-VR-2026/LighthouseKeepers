#if UNITY_EDITOR
using System;
using System.IO;
using System.Diagnostics;
namespace LighthouseKeepers.WatchDemo.Editor
{
    // Invokable from the Editor bridge as well as the menu's conversion path.
    public static class RadioImportValidation
    {
        public static string Run(string directory)
        {
            Directory.CreateDirectory(directory);var ffmpeg=ImportRadioRecordings.FindFfmpeg();
            string source=Path.Combine(directory,"quiet voice test.wav");
            const int rate=24000,count=rate*4;
            using(var writer=new BinaryWriter(File.Create(source)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);
                for(int i=0;i<count;i++){double t=(double)i/rate;writer.Write((short)(t<.5||t>3.5?0:Math.Sin(t*2*Math.PI*220)*1800));}
            }
            var results=new System.Text.StringBuilder();
            foreach(var format in new[]{"wav","m4a","mp3"})
            {
                string input=source;
                if(format!="wav")
                {
                    input=Path.Combine(directory,"quiet voice test."+format);
                    using var process=Process.Start(new ProcessStartInfo(ffmpeg,"-hide_banner -loglevel error -nostdin -y -i \""+source+"\" \""+input+"\""){UseShellExecute=false,CreateNoWindow=true});
                    if(!process.WaitForExit(30000)||process.ExitCode!=0)throw new InvalidOperationException("Fixture conversion failed: "+format);
                }
                string output=Path.Combine(directory,"normalized-"+format+".wav");ImportRadioRecordings.Convert(ffmpeg,input,output);
                using var reader=new BinaryReader(File.OpenRead(output));reader.BaseStream.Position=12;int channels=0,sampleRate=0,bits=0,samples=0;double squares=0;float peak=0;
                while(reader.BaseStream.Position+8<=reader.BaseStream.Length)
                {
                    string tag=new string(reader.ReadChars(4));int size=reader.ReadInt32();long end=reader.BaseStream.Position+size;
                    if(tag=="fmt "){reader.ReadInt16();channels=reader.ReadInt16();sampleRate=reader.ReadInt32();reader.ReadInt32();reader.ReadInt16();bits=reader.ReadInt16();}
                    if(tag=="data")for(int i=0;i<size/2;i++){float value=reader.ReadInt16()/32768f;squares+=value*value;peak=Math.Max(peak,Math.Abs(value));samples++;}
                    reader.BaseStream.Position=end+(size%2);
                }
                double duration=(double)samples/sampleRate,rms=Math.Sqrt(squares/Math.Max(samples,1));
                if(channels!=1||sampleRate!=44100||bits!=16||duration<2.8||duration>3.3||rms<.08||rms>.25||peak>.9)throw new InvalidDataException($"{format}: channels={channels} rate={sampleRate} bits={bits} duration={duration} rms={rms} peak={peak}");
                results.AppendLine($"PASS {format}: mono {sampleRate}Hz {bits}-bit, {duration:F3}s, RMS {rms:F4}, peak {peak:F4}");
            }
            File.WriteAllText(Path.Combine(directory,"result.txt"),results.ToString());return results.ToString();
        }
    }
}
#endif
