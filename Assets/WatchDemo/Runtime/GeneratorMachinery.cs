using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Original portable diesel set: continuous tubular cage, rounded tank, recessed fascia,
    // engine/alternator and transport wheels. Existing accepted control anchors are untouched.
    public static class GeneratorMachinery
    {
        public static Mesh[] Build()
        {
            var paint=new WorkshopMesh();var iron=new WorkshopMesh();var brass=new WorkshopMesh();
            // Protective end frames have continuous rounded bends, not square floating posts.
            foreach(float x in new[]{-.86f,.86f}){
                RoundedLoop(iron,x,.18f,1.71f,-.30f,.98f,.12f,.035f);
                iron.Tube(new Vector3(x,.30f,-.27f),new Vector3(x,.30f,.89f),.030f,12);
                iron.Tube(new Vector3(x,.23f,-.02f),new Vector3(x+Mathf.Sign(x)*.095f,.23f,-.02f),.07f,12);
                // Rubber tire with shallow sidewall beads and recessed steel wheel.
                iron.Tube(new Vector3(x,.245f,-.02f),new Vector3(x+Mathf.Sign(x)*.10f,.245f,-.02f),.23f,32);
                paint.Tube(new Vector3(x+Mathf.Sign(x)*.101f,.245f,-.02f),new Vector3(x+Mathf.Sign(x)*.111f,.245f,-.02f),.145f,24);
                brass.Tube(new Vector3(x+Mathf.Sign(x)*.113f,.245f,-.02f),new Vector3(x+Mathf.Sign(x)*.130f,.245f,-.02f),.040f,6);
            }
            foreach(float y in new[]{.20f,1.62f})foreach(float z in new[]{-.26f,.94f})iron.Tube(new Vector3(-.84f,y,z),new Vector3(.84f,y,z),.030f,12);
            iron.Case(new Vector3(0,.25f,.35f),new Vector3(1.52f,.10f,1.09f),.035f);
            // Wide domed tank and its folded seam. Controls below remain open to either hand.
            paint.Case(new Vector3(0,1.48f,.28f),new Vector3(1.46f,.30f,.91f),.13f);
            iron.Case(new Vector3(0,1.36f,.28f),new Vector3(1.49f,.035f,.94f),.012f);
            for(int side=-1;side<=1;side+=2){
                iron.Case(new Vector3(side*.53f,1.49f,.28f),new Vector3(.026f,.305f,.915f),.010f);
                iron.Tube(new Vector3(side*.65f,.28f,.68f),new Vector3(side*.65f,.37f,.68f),.075f,12);
                brass.Tube(new Vector3(side*.65f,.37f,.68f),new Vector3(side*.65f,.39f,.68f),.026f,6);
            }
            // Recessed front instrument panel: one supported continuous service face.
            paint.Case(new Vector3(0,1.15f,.84f),new Vector3(1.61f,.43f,.15f),.045f);
            iron.Case(new Vector3(0,1.16f,.923f),new Vector3(1.52f,.34f,.018f),.035f);
            for(int side=-1;side<=1;side+=2)foreach(float y in new[]{1.01f,1.31f})brass.Tube(new Vector3(side*.71f,y,.94f),new Vector3(side*.71f,y,.951f),.016f,6);
            // Compact cast engine and rounded valve cover; layered cooling fins read at arm's reach.
            iron.Case(new Vector3(-.34f,.66f,.25f),new Vector3(.67f,.59f,.70f),.12f);
            paint.Case(new Vector3(-.34f,.95f,.24f),new Vector3(.67f,.17f,.70f),.07f);
            for(int i=0;i<7;i++)iron.Case(new Vector3(-.34f,.67f+i*.037f,.34f),new Vector3(.69f,.018f,.66f),.008f);
            // Recoil starter shroud is part of the engine, behind the existing cord housing.
            paint.Tube(new Vector3(-.37f,.59f,.62f),new Vector3(-.37f,.59f,.76f),.255f,32);
            iron.Tube(new Vector3(-.37f,.59f,.762f),new Vector3(-.37f,.59f,.779f),.20f,32);
            for(int i=0;i<9;i++){float x=-.53f+i*.04f;float h=Mathf.Sqrt(.18f*.18f-(x+.37f)*(x+.37f));paint.Case(new Vector3(x,.59f,.79f),new Vector3(.010f,h*2,.012f),.003f);}
            brass.Tube(new Vector3(-.37f,.59f,.78f),new Vector3(-.37f,.59f,.81f),.055f,6);
            // Alternator: one ribbed body, end cap and fan guard; its axis aligns with engine shaft.
            iron.Tube(new Vector3(.45f,.62f,-.08f),new Vector3(.45f,.62f,.68f),.26f,24);
            for(int i=0;i<9;i++)paint.Tube(new Vector3(.45f,.62f,-.05f+i*.075f),new Vector3(.45f,.62f,-.027f+i*.075f),.273f,24);
            iron.Tube(new Vector3(.45f,.62f,.685f),new Vector3(.45f,.62f,.72f),.24f,24);
            for(int i=0;i<9;i++){float x=.28f+i*.042f;float h=Mathf.Sqrt(.21f*.21f-(x-.45f)*(x-.45f));paint.Case(new Vector3(x,.62f,.73f),new Vector3(.011f,h*2,.014f),.003f);}
            // Air filter, silencer, feed hose and mounted filler neck. No disconnected pipework.
            iron.Tube(new Vector3(-.67f,.91f,.08f),new Vector3(-.67f,1.19f,.08f),.10f,20);
            iron.Pipe(.037f,new Vector3(-.58f,.89f,-.07f),new Vector3(-.72f,.94f,-.14f),new Vector3(-.72f,1.84f,-.14f),new Vector3(-.72f,2.03f,-.05f));
            iron.Tube(new Vector3(-.72f,1.44f,-.14f),new Vector3(-.72f,1.82f,-.14f),.082f,20);
            brass.Pipe(.027f,new Vector3(.62f,1.10f,1.08f),new Vector3(.62f,1.10f,.81f),new Vector3(.66f,1.35f,.65f),new Vector3(.56f,1.48f,.65f));
            iron.Pipe(.014f,new Vector3(-.23f,1.33f,.18f),new Vector3(-.23f,1.02f,.05f),new Vector3(-.51f,.86f,.05f));
            iron.Pipe(.012f,new Vector3(.20f,1.18f,.87f),new Vector3(.20f,1.00f,.84f),new Vector3(-.20f,.89f,.65f));
            for(int i=0;i<5;i++)brass.Tube(new Vector3(-.60f+i*.13f,.43f,.61f),new Vector3(-.60f+i*.13f,.43f,.64f),.015f,6);
            return new[]{paint.Finish("Weathered tank fascia and alternator"),iron.Finish("Continuous cage diesel and wheels"),brass.Finish("Filler fasteners and wheel hubs")};
        }
        static void RoundedLoop(WorkshopMesh mesh,float x,float bottom,float top,float back,float front,float radius,float tube)
        {
            var points=new Vector3[33];int n=0;
            for(int corner=0;corner<4;corner++)for(int j=0;j<8;j++){
                float angle=(corner*90+j*90f/7)*Mathf.Deg2Rad;
                float y=corner<2?top-radius:bottom+radius,z=corner==0||corner==3?front-radius:back+radius;
                points[n++]=new Vector3(x,y+Mathf.Sin(angle)*radius,z+Mathf.Cos(angle)*radius);
            }
            points[32]=points[0];for(int i=1;i<points.Length;i++)if((points[i]-points[i-1]).sqrMagnitude>.000001f)mesh.Tube(points[i-1],points[i],tube,10);
        }
    }
}
