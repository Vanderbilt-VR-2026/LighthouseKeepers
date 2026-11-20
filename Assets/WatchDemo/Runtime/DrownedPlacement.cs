using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Evaluated only while the invisible warning searches, never a per-frame scene scan.
    public sealed class DrownedPlacement
    {
        readonly MeshRenderer[] surfaces;
        readonly Transform figure,localRig;
        static readonly float[] angles={0,15,-15,30,-30,40,-40,52,-52,65,-65,85,-85,110,-110};
        static readonly float[] distances={2.2f,2.7f,3.2f,1.85f};
        public DrownedPlacement(Transform figure,Transform localRig){this.figure=figure;this.localRig=localRig;surfaces=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);}
        public static bool InView(Vector3 head,Vector3 forward,Vector3 feet)
        {var delta=feet+Vector3.up*1.5f-head;return delta.sqrMagnitude>=2.25f&&Vector3.Dot(delta.normalized,forward.normalized)>.70f;}
        public bool ClearSight(Vector3 head,Vector3 point)
        {
            var delta=point-head;float distance=delta.magnitude;var ray=new Ray(head,delta.normalized);
            if(Physics.Raycast(ray,out var hit,distance-.1f,~0,QueryTriggerInteraction.Ignore)&&(!localRig||!hit.transform.IsChildOf(localRig)))return false;
            foreach(var r in surfaces){
                if(!r||!r.enabled||!r.gameObject.activeInHierarchy||r.transform.IsChildOf(figure)||localRig&&r.transform.IsChildOf(localRig))continue;
                var b=r.bounds;var size=b.size;
                // Large shell surfaces use their real colliders; an AABB would fill the room.
                if(size.y<.09f||size.x>3||size.z>3||size.y>3.2f||b.Contains(head))continue;
                if(b.IntersectRay(ray,out float at)&&at>.2f&&at<distance-.18f)return false;
            }
            return true;
        }
        public bool Visible(Vector3 head,Vector3 forward,Vector3 feet,float face=1.65f)=>InView(head,forward,feet)&&ClearSight(head,feet+Vector3.up*face)&&ClearSight(head,feet+Vector3.up*(face*.65f));
        public bool Ceiling(Vector3 head,Vector3 forward,Vector3 other,bool two,out Vector3 point,out float yaw)
        {
            point=default;yaw=0;var horizontal=Vector3.ProjectOnPlane(forward,Vector3.up).normalized;
            foreach(float distance in new[]{3.4f,3.8f,4.2f})foreach(float angle in new[]{0f,25f,-25f,45f,-45f}){
                var direction=Quaternion.Euler(0,angle,0)*horizontal;var origin=head+direction*distance;
                if(!Physics.Raycast(origin,Vector3.up,out var hit,2.5f,~0,QueryTriggerInteraction.Ignore)||hit.normal.y>-.8f||!hit.collider.name.Contains("Floor"))continue;
                var p=hit.point-Vector3.up*.35f;var facing=Quaternion.LookRotation(-direction)*Quaternion.Euler(90,0,0);var face=p+facing*(Vector3.up*1.55f);
                if((head-face).sqrMagnitude<4||two&&(other-face).sqrMagnitude<4||!ClearSight(head,face))continue;
                if(Physics.CheckCapsule(p,face,.23f,~0,QueryTriggerInteraction.Ignore))continue;
                point=p;yaw=Quaternion.LookRotation(-direction).eulerAngles.y;return true;
            }return false;
        }
        public bool Find(Vector3 head,Vector3 forward,Vector3 other,bool two,out Vector3 feet,float face=1.65f,float height=2.1f)
        {
            feet=default;float floor=Mathf.Clamp(Mathf.Round((head.y-1.45f)/3.3f)*3.3f,0,9.9f);
            var facing=Vector3.ProjectOnPlane(forward,Vector3.up).normalized;if(facing.sqrMagnitude<.5f)facing=Vector3.forward;
            foreach(float angle in angles)foreach(float distance in distances){
                var candidate=head+Quaternion.Euler(0,angle,0)*facing*distance;candidate.y=floor;
                if(!Physics.Raycast(candidate+Vector3.up*.7f,Vector3.down,out var ground,1.1f,~0,QueryTriggerInteraction.Ignore)||ground.normal.y<.95f||Mathf.Abs(ground.point.y-floor)>.3f)continue;
                candidate.y=ground.point.y+.015f;
                if(!DrownedAppearance.HasComfortClearance(head,candidate)||two&&!DrownedAppearance.HasComfortClearance(other,candidate))continue;
                if(Physics.CheckCapsule(candidate+Vector3.up*.45f,candidate+Vector3.up*(height-.22f),.42f,~0,QueryTriggerInteraction.Ignore))continue;
                if(!ClearSight(head,candidate+Vector3.up*face)||!ClearSight(head,candidate+Vector3.up*(face*.65f)))continue;
                feet=candidate;return true;
            }
            return false;
        }
    }
}
