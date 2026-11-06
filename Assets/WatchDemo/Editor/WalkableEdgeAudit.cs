using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;
public static class WalkableEdgeAudit {
 public static object Run(string path){var rows=new List<string>();int samples=0,edges=0;var gaps=new List<Vector3>();
 bool Solid(Collider c)=>c.enabled&&!c.isTrigger&&!c.GetComponentInParent<Rigidbody>()&&!c.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>()&&!c.name.Contains("water",StringComparison.OrdinalIgnoreCase);
 bool Ground(Vector3 p,float depth,out RaycastHit h){h=Physics.RaycastAll(p,Vector3.down,depth,~0,QueryTriggerInteraction.Ignore).Where(x=>Solid(x.collider)&&x.normal.y>.65f).OrderBy(x=>x.distance).FirstOrDefault();return h.collider;}
 foreach(float y in new[]{0f,3.3f,6.6f,9.9f})for(float x=-4.3f;x<=4.3f;x+=.25f)for(float z=y==0?-12.5f:-4.3f;z<=4.3f;z+=.25f){
  if(!Ground(new Vector3(x,y+.18f,z),.30f,out var h))continue;samples++;var p=h.point;
  foreach(var dir in new[]{Vector3.right,Vector3.left,Vector3.forward,Vector3.back}){
   if(Ground(p+dir*.42f+Vector3.up*.70f,1.18f,out var next)&&next.point.y<=p.y+.55f)continue;edges++;
   bool blocked=Physics.CapsuleCastAll(p+Vector3.up*.30f,p+Vector3.up*1.5f,.20f,dir,.50f,~0,QueryTriggerInteraction.Ignore).Any(q=>Solid(q.collider)&&q.normal.y<.65f);
   if(!blocked){gaps.Add(p);rows.Add($"GAP floor={y} from={p:F3} dir={dir} support={h.collider.name}");}
  }
 }
 File.WriteAllLines(path,rows.Prepend($"samples={samples} edgeProbes={edges} unguarded={gaps.Count}"));return new{samples,edges,gaps=gaps.Count};}
}
