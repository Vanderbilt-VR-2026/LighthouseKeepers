using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    [CreateAssetMenu(menuName="Lighthouse/Radio program")]
    public sealed class RadioProgram : ScriptableObject
    {
        public float minimumMHz=156, maximumMHz=158, targetMHz=156.8f;
        [Min(.01f)] public float toleranceMHz=.04f;
        [Min(.1f)] public float lockSeconds=1, transmitSeconds=3;
        public int[] callCode={0,2,1,3}; // Red, blue, amber, green. CodeSource is a separate scene object.
        public string ColourName(int index) => index switch {0=>"RED",1=>"AMBER",2=>"BLUE",3=>"GREEN",_=>"?"};
    }
}
