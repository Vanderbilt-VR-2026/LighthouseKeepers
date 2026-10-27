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
        public float degreesPerSecond=18;
        WatchState watch;bool powered,initialized;float speed;
        MaterialPropertyBlock block;
        public bool Lit=>lantern.enabled;
        public float SpinFraction=>speed;
        void Awake(){block=new MaterialPropertyBlock();Apply(false);}
        public void Bind(WatchState state){watch=state;watch.Changed+=Changed;Changed();}
        void Changed()
        {
            bool next=watch.Power==PowerState.On;if(initialized&&powered==next)return;
            powered=next;Apply(powered);
            audioSource.PlayOneShot(powered?powerUp:powerDown,.35f);
            if(!powered)speed=0;
            initialized=true;
        }
        void Apply(bool on)
        {
            lantern.enabled=beam.enabled=on;
            block.SetColor("_BaseColor",on?new Color(.8f,.42f,.12f):new Color(.07f,.065f,.05f));
            block.SetColor("_EmissionColor",on?new Color(1,.30f,.045f)*.7f:Color.black);
            foreach(var lens in lenses)lens.SetPropertyBlock(block);
        }
        void Update()
        {
            if(!powered)return;
            speed=Mathf.MoveTowards(speed,1,Time.deltaTime/spinUpSeconds);
            rotor.Rotate(Vector3.up,degreesPerSecond*speed*Time.deltaTime,Space.Self);
            lantern.intensity=1.8f*speed;beam.intensity=6*speed;
        }
        void OnDestroy(){if(watch!=null)watch.Changed-=Changed;}
    }
}
