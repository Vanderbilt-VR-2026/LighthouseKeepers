using System;
using System.Collections.Generic;
namespace LighthouseKeepers.Core.Progression
{
    // Build once, sample without allocations. Standard 1/3-unit marks and 1/3/7-unit gaps.
    public sealed class MorseFilament
    {
        static readonly string[] Alphabet={".-","-...","-.-.","-..",".","..-.","--.","....","..",".---","-.-",".-..","--","-.","---",".--.","--.-",".-.","...","-","..-","...-",".--","-..-","-.--","--.."};
        static readonly string[] Digits={"-----",".----","..---","...--","....-",".....","-....","--...","---..","----."};
        readonly float[] starts,ends;
        public float Duration {get;}
        public float DotSeconds {get;}
        public int MarkCount=>starts.Length;
        public MorseFilament(string message,float dotSeconds)
        {
            if(string.IsNullOrWhiteSpace(message)||!float.IsFinite(dotSeconds)||dotSeconds<.35f)throw new ArgumentOutOfRangeException();
            DotSeconds=dotSeconds;var a=new List<float>();var b=new List<float>();float units=0;
            var words=message.ToUpperInvariant().Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);
            foreach(var word in words){foreach(char c in word){string code=c>='A'&&c<='Z'?Alphabet[c-'A']:c>='0'&&c<='9'?Digits[c-'0']:null;if(code==null)continue;
                foreach(char mark in code){a.Add(units*dotSeconds);units+=mark=='-'?3:1;b.Add(units*dotSeconds);units++;}units+=2;
            }units+=4;}
            starts=a.ToArray();ends=b.ToArray();Duration=Math.Max(dotSeconds,units*dotSeconds);
        }
        public float Sample(float seconds,float floor=.06f)
        {
            if(!float.IsFinite(seconds))return floor;
            float t=((seconds%Duration)+Duration)%Duration,edge=DotSeconds*.30f;
            for(int i=0;i<starts.Length;i++)if(t>=starts[i]&&t<ends[i]){
                float f=Math.Clamp(Math.Min(t-starts[i],ends[i]-t)/edge,0,1);f=f*f*(3-2*f);return floor+(1-floor)*f;
            }
            return floor;
        }
    }
}
