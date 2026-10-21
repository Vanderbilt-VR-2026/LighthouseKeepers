using System;
using System.IO;
using UnityEngine;

namespace LighthouseKeepers.Net
{
    // Wire values 0..4 are the shared HandPose enum: Open, Fist, WrapGrip, Pinch, PumpHandle.
    // This is a packet, not a second gameplay HandPose definition. Finger bones are never transmitted.
    public struct KeeperPoseFrame
    {
        public int Actor, Sequence;
        public Vector3 Head, Left, Right;
        public Quaternion HeadRotation, LeftRotation, RightRotation;
        public byte LeftPose, RightPose;
        public float LeftGrip, LeftTrigger, RightGrip, RightTrigger;
        public const int Bytes = 110;
        public bool Valid => Actor > 0 && Sequence > 0 && Position(Head) && Position(Left) && Position(Right) &&
            Rotation(HeadRotation) && Rotation(LeftRotation) && Rotation(RightRotation) &&
            (Left - Head).sqrMagnitude <= 9 && (Right - Head).sqrMagnitude <= 9 &&
            LeftPose <= 4 && RightPose <= 4 && Analog(LeftGrip) && Analog(LeftTrigger) && Analog(RightGrip) && Analog(RightTrigger);
        public static bool Finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        static bool Analog(float v) => Finite(v) && v >= 0 && v <= 1;
        static bool Position(Vector3 v) => Finite(v.x) && Finite(v.y) && Finite(v.z) && v.sqrMagnitude < 1000000;
        static bool Rotation(Quaternion q)
        {
            float n = q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w;
            return Finite(n) && n > .8f && n < 1.2f;
        }
        public byte[] Encode()
        {
            using var stream = new MemoryStream(Bytes);
            using var writer = new BinaryWriter(stream);
            writer.Write(Actor); writer.Write(Sequence);
            Write(writer, Head, HeadRotation); Write(writer, Left, LeftRotation); Write(writer, Right, RightRotation);
            writer.Write(LeftPose); writer.Write(LeftGrip); writer.Write(LeftTrigger);
            writer.Write(RightPose); writer.Write(RightGrip); writer.Write(RightTrigger);
            return stream.ToArray();
        }
        public static bool TryDecode(byte[] data, out KeeperPoseFrame frame)
        {
            frame = default;
            if (data == null || data.Length != Bytes) return false;
            using var reader = new BinaryReader(new MemoryStream(data, false));
            frame.Actor = reader.ReadInt32(); frame.Sequence = reader.ReadInt32();
            Read(reader, out frame.Head, out frame.HeadRotation); Read(reader, out frame.Left, out frame.LeftRotation); Read(reader, out frame.Right, out frame.RightRotation);
            frame.LeftPose = reader.ReadByte(); frame.LeftGrip = reader.ReadSingle(); frame.LeftTrigger = reader.ReadSingle();
            frame.RightPose = reader.ReadByte(); frame.RightGrip = reader.ReadSingle(); frame.RightTrigger = reader.ReadSingle();
            return frame.Valid;
        }
        static void Write(BinaryWriter w, Vector3 p, Quaternion q)
        { w.Write(p.x); w.Write(p.y); w.Write(p.z); w.Write(q.x); w.Write(q.y); w.Write(q.z); w.Write(q.w); }
        static void Read(BinaryReader r, out Vector3 p, out Quaternion q)
        { p = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()); q = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()); }
        public static KeeperPoseFrame Lerp(KeeperPoseFrame a, KeeperPoseFrame b, float t)
        {
            var f = b;
            f.Head = Vector3.Lerp(a.Head,b.Head,t); f.Left = Vector3.Lerp(a.Left,b.Left,t); f.Right = Vector3.Lerp(a.Right,b.Right,t);
            f.HeadRotation = Quaternion.Slerp(a.HeadRotation,b.HeadRotation,t);
            f.LeftRotation = Quaternion.Slerp(a.LeftRotation,b.LeftRotation,t); f.RightRotation = Quaternion.Slerp(a.RightRotation,b.RightRotation,t);
            f.LeftGrip = Mathf.Lerp(a.LeftGrip,b.LeftGrip,t); f.LeftTrigger = Mathf.Lerp(a.LeftTrigger,b.LeftTrigger,t);
            f.RightGrip = Mathf.Lerp(a.RightGrip,b.RightGrip,t); f.RightTrigger = Mathf.Lerp(a.RightTrigger,b.RightTrigger,t);
            if (t < 1) { f.LeftPose = a.LeftPose; f.RightPose = a.RightPose; }
            return f;
        }
    }

    public sealed class KeeperPoseBuffer
    {
        const int Capacity = 16;
        readonly KeeperPoseFrame[] frames = new KeeperPoseFrame[Capacity];
        readonly double[] times = new double[Capacity];
        int count, start;
        public double LastReceived { get; private set; } = double.NegativeInfinity;
        public int Sequence { get; private set; }
        public bool Add(KeeperPoseFrame frame, double receivedAt)
        {
            if (!frame.Valid || frame.Sequence <= Sequence || receivedAt < LastReceived || double.IsNaN(receivedAt) || double.IsInfinity(receivedAt)) return false;
            if (count == Capacity) { start = (start + 1) % Capacity; count--; }
            int at = (start + count++) % Capacity;
            frames[at] = frame; times[at] = receivedAt; Sequence = frame.Sequence; LastReceived = receivedAt;
            return true;
        }
        public bool Sample(double now, out KeeperPoseFrame frame)
        {
            frame = default;
            if (count == 0 || now - LastReceived > .6) return false;
            double target = now - .10; // 100 ms receive-time interpolation, bounded; never extrapolate a disconnected hand.
            int a = start;
            if (target <= times[a]) { frame = frames[a]; return true; }
            for (int i = 1; i < count; i++)
            {
                int b = (start + i) % Capacity;
                if (times[b] >= target)
                {
                    double span = times[b] - times[a];
                    frame = KeeperPoseFrame.Lerp(frames[a], frames[b], span <= 0 ? 1 : (float)((target-times[a])/span)); return true;
                }
                a = b;
            }
            frame = frames[a]; return true;
        }
    }
}
