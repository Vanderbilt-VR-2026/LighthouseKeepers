using System.Linq;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Resolve once when building: real shell wall, with the whole fitting clear of stair rails.
    public static class WallFixtureMount
    {
        public static bool Place(Transform root,float height,float preferredAngle,bool speaker)
        {
            var rails=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                .Where(r=>r.enabled&&r.name.StartsWith("SM_Rail_")).ToArray();
            for(int n=0;n<24;n++)
            {
                int offset=n==0?0:(n%2==1?(n+1)/2:-n/2);
                var direction=Quaternion.Euler(0,preferredAngle+offset*15,0)*Vector3.forward;
                foreach(var hit in Physics.RaycastAll(new Vector3(0,height,0),direction,5,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))
                {
                    bool frame=height>9.9f&&hit.collider.name.Contains("Frame_Lantern")&&!hit.collider.name.Contains("Top");
                    if(!hit.collider.name.StartsWith("UCX_")||(!hit.collider.name.Contains("Wall")&&!frame))continue;
                    var normal=frame?-direction:hit.normal;
                    var position=hit.point+normal*.025f;
                    var bounds=new Bounds(position+normal*.14f+Vector3.down*(speaker?.43f:0),speaker?new Vector3(.65f,1.48f,.65f):new Vector3(.32f,.44f,.32f));
                    if(rails.Any(r=>r.bounds.Intersects(bounds)))break;
                    root.SetPositionAndRotation(position,Quaternion.LookRotation(normal));return true;
                }
            }
            Debug.LogWarning("No clear wall mount for "+root.name);return false;
        }
    }
}
