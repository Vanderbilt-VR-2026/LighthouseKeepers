using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    public static class DrownedSculpt
    {
        public static Mesh Body()=>DrownedForms.Build(DrownedForm.SeaHag);
        public static Mesh Effects()
        {
            var v=new List<Vector3>();var uv=new List<Vector2>();var seed=new List<Vector2>();var t=new List<int>();
            // Eight low mist cards, 24 drips, two small breath cards. No ParticleSystem.
            for(int i=0;i<34;i++)
            {
                int k=v.Count;float kind=i<8?0:i<32?1:2;
                for(int j=0;j<4;j++){v.Add(Vector3.zero);uv.Add(new Vector2(j==0||j==3?-1:1,j<2?-1:1));seed.Add(new Vector2((i*.618034f)%1,kind));}
                t.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});
            }
            var m=new Mesh{name="Bounded Drowned mist and dripping water"};m.SetVertices(v);m.SetUVs(0,uv);m.SetUVs(1,seed);m.SetTriangles(t,0);m.bounds=new Bounds(new Vector3(0,1,0),new Vector3(2,2.5f,2));return m;
        }
    }
}
