using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Original metal right-angle hand torch. A narrow ribbed handle fits the existing glove cavity.
    public static class FlashlightMesh
    {
        public static readonly Vector3 Handle=Vector3.zero,Hook=new(0,.085f,-.028f),Emitter=new(0,.115f,.065f);
        // Measured in the authored WrapGrip glove, clear of finger and palm surfaces.
        public static readonly Vector3 HandContact=new(0,-.052f,.015f);
        public const float GripRadius=.0125f;
        public static Mesh Body(){var m=new WorkshopMesh();
            m.Tube(new Vector3(0,-.092f,0),new Vector3(0,.098f,0),GripRadius,16);
            for(int i=0;i<8;i++){float y=-.07f+i*.019f;m.Tube(new Vector3(0,y,0),new Vector3(0,y+.003f,0),GripRadius+.001f,16);}
            m.Tube(new Vector3(0,-.103f,0),new Vector3(0,-.088f,0),.018f,16);
            m.Tube(new Vector3(0,.080f,0),new Vector3(0,.122f,0),.025f,16);
            m.Tube(new Vector3(0,.115f,-.016f),new Vector3(0,.115f,.048f),.033f,20);
            m.Tube(new Vector3(0,.115f,.047f),new Vector3(0,.115f,.062f),.037f,24);
            m.Case(new Vector3(0,.046f,-.017f),new Vector3(.017f,.027f,.008f),.003f);
            m.Pipe(.003f,new Vector3(-.008f,.094f,-.019f),new Vector3(-.008f,.078f,-.030f),new Vector3(.008f,.078f,-.030f),new Vector3(.008f,.094f,-.019f));
            return m.Finish("Ribbed steel hand torch, battery cap and reflector bezel");}
        public static Mesh Lens(){var m=new WorkshopMesh();m.Tube(Emitter-Vector3.forward*.004f,Emitter,.029f,24);return m.Finish("Recessed flashlight lens");}
    }
}
