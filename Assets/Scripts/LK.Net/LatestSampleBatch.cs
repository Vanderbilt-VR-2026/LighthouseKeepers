using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
namespace LighthouseKeepers.Net
{
    // Latest sample per actor/prop. One acknowledged batch in flight bounds latency
    // under congestion instead of queueing an ever-growing history of old poses.
    public sealed class LatestSampleBatch
    {
        public readonly struct Sample
        {public readonly int Kind;public readonly byte[] Data;public Sample(int kind,byte[] data){Kind=kind;Data=data;}}
        readonly Dictionary<long,Sample> pending=new();
        public int Count=>pending.Count;
        public void Put(int kind,int slot,byte[] data)
        {
            if(data==null||data.Length==0||data.Length>2048||slot<0)return;
            if(kind!=10&&kind!=11&&kind!=20&&kind!=21&&kind!=41)return;
            long key=((long)kind<<32)|(uint)slot;
            if(pending.Count>=32&&!pending.ContainsKey(key))return;
            pending[key]=new Sample(kind,data);
        }
        public byte[] Pack(int sequence)
        {
            using var output=new MemoryStream();
            using(var zip=new DeflateStream(output,CompressionLevel.Fastest,true))
            using(var writer=new BinaryWriter(zip))
            {
                writer.Write(sequence);writer.Write(pending.Count);
                foreach(var sample in pending.Values){writer.Write(sample.Kind);writer.Write(sample.Data.Length);writer.Write(sample.Data);}
            }
            pending.Clear();return output.ToArray();
        }
        public static bool TryUnpack(byte[] data,out int sequence,out List<Sample> samples)
        {
            sequence=0;samples=new List<Sample>();
            if(data==null||data.Length==0||data.Length>2048)return false;
            try
            {
                using var input=new MemoryStream(data);using var zip=new DeflateStream(input,CompressionMode.Decompress);
                using var reader=new BinaryReader(zip);
                sequence=reader.ReadInt32();int count=reader.ReadInt32(),total=0;
                if(sequence<=0||count<1||count>32)return false;
                for(int i=0;i<count;i++)
                {
                    int kind=reader.ReadInt32(),size=reader.ReadInt32();total+=size;
                    if(size<1||size>2048||total>16384||(kind!=10&&kind!=11&&kind!=20&&kind!=21&&kind!=41))return false;
                    var bytes=reader.ReadBytes(size);if(bytes.Length!=size)return false;samples.Add(new Sample(kind,bytes));
                }
                return reader.BaseStream.ReadByte()==-1;
            }
            catch(InvalidDataException){return false;}
            catch(IOException){return false;}
            catch(ArgumentException){return false;}
        }
    }
}
