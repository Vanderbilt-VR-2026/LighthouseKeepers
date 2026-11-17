using System;
using UnityEngine;
using UnityEngine.AI;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    // A single navigation dataset for the existing room/stair geometry. No runtime rebake or teleport fallback.
    public sealed class HunterNavigation:IDisposable
    {
        readonly HorrorTuning tuning;
        NavMeshDataInstance instance;
        readonly NavMeshPath path=new();readonly RaycastHit[] hits=new RaycastHit[32];
        public bool Ready=>instance.valid;
        public HunterNavigation(HorrorTuning tuning,NavMeshData data)
        {this.tuning=tuning;if(data)instance=NavMesh.AddNavMeshData(data);}
        public bool SampleFloor(Vector3 point,out Vector3 result)
        {
            result=default;if(!Ready||!NavMesh.SamplePosition(point,out var hit,tuning.navSampleRange,NavMesh.AllAreas))return false;
            // Never snap a bunk/floor target to a stair on the next floor.
            if(Mathf.Abs(hit.position.y-point.y)>tuning.navClimb)return false;
            result=hit.position;return true;
        }
        public bool Route(Vector3 start,Vector3 end,out Vector3[] corners)
        {
            corners=Array.Empty<Vector3>();if(!SampleFloor(start,out var a)||!SampleFloor(end,out var b)||!NavMesh.CalculatePath(a,b,NavMesh.AllAreas,path)||path.status!=NavMeshPathStatus.PathComplete)return false;
            corners=path.corners;return corners.Length>=2;
        }
        public bool SegmentClear(Vector3 from,Vector3 to)
        {
            var delta=to-from;float distance=delta.magnitude;if(distance<.001f)return true;
            // Lower clearance follows the authored stair ramp; never ignores a vertical wall face.
            int count=Physics.CapsuleCastNonAlloc(from+Vector3.up*(tuning.navClimb+tuning.radius),from+Vector3.up*(tuning.navigationHeight-tuning.radius),tuning.radius*.9f,delta.normalized,hits,distance,~0,QueryTriggerInteraction.Ignore);
            if(count==hits.Length)return false;
            for(int i=0;i<count;i++){
                var hit=hits[i];if(hit.normal.y>.45f)continue;
                if(hit.collider.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>()||hit.collider.GetComponentInParent<CharacterController>())continue;
                return false;
            }return true;
        }
        public void Dispose(){if(instance.valid)instance.Remove();}
    }
}
