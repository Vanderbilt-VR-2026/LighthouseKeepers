using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Adapted from parked ShipBeaconPresentation: light/beam/emission follow the existing day authority.
    public sealed class WatchBeacon:MonoBehaviour
    {
        public Transform rotor;
        public Renderer[] lenses;
        public Light lantern,beam;
        public AudioSource audioSource;
        public AudioClip powerDown,powerUp;
        [Min(.1f)] public float spinUpSeconds=1.5f;
        [Range(.05f,1),Tooltip("Lens glass opacity; the transparent FresnelAmber material keeps its emission tint.")] public float lensAlpha=.4f;
        public float degreesPerSecond=18;
        WatchState watch;bool powered,initialized;float speed;
        MaterialPropertyBlock block;
        Renderer visibleBeam;float brightness=1,previousAlignment=.5f;
        public bool Lit=>lantern.enabled;
        public float SpinFraction=>speed;
        void Awake(){block=new MaterialPropertyBlock();Apply(false);}
        public void Bind(WatchState state){watch=state;watch.Changed+=Changed;Changed();}
        void Changed()
        {
            if(watch.Beacon!=null){float angle=watch.Beacon.Alignment;rotor.Rotate(Vector3.up,(angle-previousAlignment)*270,Space.Self);previousAlignment=angle;}
            brightness=watch.Beacon?.Brightness??1;bool next=watch.BeaconShining&&(!watch.Immersion||!watch.Failed);if(initialized&&powered==next){if(watch.Beacon!=null)Apply(powered);return;}
            powered=next;Apply(powered);
            audioSource.PlayOneShot(powered?powerUp:powerDown,.35f);
            if(!powered)speed=0;
            initialized=true;
        }
        void Apply(bool on)
        {
            lantern.enabled=beam.enabled=on;
            if(visibleBeam)visibleBeam.enabled=on;
            block.SetColor("_BaseColor",on?new Color(.8f,.42f,.12f,lensAlpha):new Color(.07f,.065f,.05f,lensAlpha));
            block.SetColor("_EmissionColor",on?new Color(1,.30f,.045f)*(.7f*brightness):Color.black);
            foreach(var lens in lenses)lens.SetPropertyBlock(block);
            if(visibleBeam){block.SetColor("_BaseColor",new Color(1,.55f,.16f,.12f*brightness));visibleBeam.SetPropertyBlock(block);}
        }
        void Update()
        {
            if(!powered)return;
            speed=Mathf.MoveTowards(speed,1,Time.deltaTime/spinUpSeconds);
            rotor.Rotate(Vector3.up,degreesPerSecond*speed*Time.deltaTime,Space.Self);
            lantern.intensity=1.8f*speed*brightness;beam.intensity=6*speed*brightness;
        }
        public void SetVisibleBeam(Renderer renderer){visibleBeam=renderer;Apply(powered);}
        void OnDestroy(){if(watch!=null)watch.Changed-=Changed;}
    }
}
