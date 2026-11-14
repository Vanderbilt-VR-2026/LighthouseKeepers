using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // A coherent skid-mounted engine, bolted service face, ribbed alternator and fuel plumbing.
    // Existing interactive transforms stay exactly where they were accepted in the headset.
    public static class GeneratorMachinery
    {
        public static Mesh[] Build()
        {
            var paint=new WorkshopMesh();var iron=new WorkshopMesh();var brass=new WorkshopMesh();
            paint.Case(new Vector3(-.26f,.70f,.30f),new Vector3(1.18f,.64f,.90f),.14f);
            paint.Case(new Vector3(0,1.23f,.91f),new Vector3(1.58f,.82f,.16f),.085f);
            paint.Case(new Vector3(0,1.27f,.25f),new Vector3(1.64f,.48f,1.05f),.14f);
            // Rounded horizontal tank behind the controls, with two retaining straps.
            paint.Tube(new Vector3(-.49f,1.53f,.20f),new Vector3(.49f,1.53f,.20f),.19f,24);
            for(int side=-1;side<=1;side+=2){
                iron.Case(new Vector3(side*.76f,.14f,.35f),new Vector3(.15f,.18f,1.34f),.025f);
                iron.Case(new Vector3(0,.14f,.35f+side*.49f),new Vector3(1.82f,.13f,.13f),.025f);
                for(int end=-1;end<=1;end+=2){iron.Tube(new Vector3(side*.70f,.21f,.35f+end*.39f),new Vector3(side*.70f,.34f,.35f+end*.39f),.085f,12);brass.Tube(new Vector3(side*.70f,.34f,.35f+end*.39f),new Vector3(side*.70f,.36f,.35f+end*.39f),.035f,6);}
                iron.Tube(new Vector3(side*.33f-.015f,1.53f,.20f),new Vector3(side*.33f+.015f,1.53f,.20f),.198f,24);
            }
            // Separate iron cylinder heads, injection lines, yellow valve covers.
            for(int i=0;i<3;i++){
                float x=-.64f+i*.30f;
                iron.Case(new Vector3(x,1.05f,.35f),new Vector3(.24f,.40f,.57f),.05f);
                paint.Case(new Vector3(x,1.25f,.35f),new Vector3(.26f,.10f,.60f),.04f);
                brass.Pipe(.012f,new Vector3(x,.62f,.80f),new Vector3(x-.05f,.88f,.80f),new Vector3(x-.05f,1.08f,.65f));
            }
            // Ribbed cylindrical alternator below the front service panel.
            iron.Tube(new Vector3(.51f,.56f,-.12f),new Vector3(.51f,.56f,.73f),.29f,24);
            for(int i=0;i<11;i++)paint.Tube(new Vector3(.51f,.56f,-.11f+i*.075f),new Vector3(.51f,.56f,-.085f+i*.075f),.31f,24);
            iron.Tube(new Vector3(.51f,.56f,.73f),new Vector3(.51f,.56f,.755f),.255f,24);
            for(int i=0;i<10;i++){float x=.51f-.20f+i*.044f;float h=Mathf.Sqrt(.24f*.24f-(x-.51f)*(x-.51f));paint.Case(new Vector3(x,.56f,.766f),new Vector3(.014f,h*2,.015f),.003f);}
            // Flywheel with central axle, spokes and rim; visible below the fascia.
            for(int i=0;i<32;i++){float a=i*Mathf.PI*2/32,b=(i+1)*Mathf.PI*2/32;iron.Tube(new Vector3(-.43f+Mathf.Cos(a)*.25f,.56f+Mathf.Sin(a)*.25f,.81f),new Vector3(-.43f+Mathf.Cos(b)*.25f,.56f+Mathf.Sin(b)*.25f,.81f),.038f,8);}
            for(int i=0;i<6;i++){float a=i*Mathf.PI/3;iron.Tube(new Vector3(-.43f,.56f,.81f),new Vector3(-.43f+Mathf.Cos(a)*.23f,.56f+Mathf.Sin(a)*.23f,.81f),.026f,8);}
            brass.Tube(new Vector3(-.43f,.56f,.79f),new Vector3(-.43f,.56f,.87f),.06f,6);
            // Exhaust ends at the original smoke anchor. Fuel pipe visibly feeds tank from cap.
            iron.Pipe(.065f,new Vector3(-.65f,1.0f,.10f),new Vector3(-.72f,1.22f,.10f),new Vector3(-.72f,1.84f,.10f),new Vector3(-.72f,2.03f,-.05f));
            for(int i=0;i<8;i++)brass.Tube(new Vector3(-.72f,1.40f+i*.055f,.10f),new Vector3(-.72f,1.42f+i*.055f,.10f),.069f,12);
            brass.Pipe(.035f,new Vector3(.62f,1.08f,1.08f),new Vector3(.72f,1.08f,.78f),new Vector3(.72f,1.47f,.40f),new Vector3(.49f,1.53f,.20f));
            // Fasteners and shallow louvres; no fake floating boxes.
            for(int x=-1;x<=1;x+=2)foreach(float y in new[]{.86f,1.43f})brass.Tube(new Vector3(x*.72f,y,.982f),new Vector3(x*.72f,y,1.003f),.022f,6);
            for(int i=0;i<9;i++)iron.Case(new Vector3(-.77f+i*.19f,.78f,.94f),new Vector3(.13f,.022f,.015f),.008f);
            return new[]{paint.Finish("Diesel castings and painted shroud"),iron.Finish("Skid alternator and exhaust"),brass.Finish("Fuel lines and brass fasteners")};
        }
    }
}
