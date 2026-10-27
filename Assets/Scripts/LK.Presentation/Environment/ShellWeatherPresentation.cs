using UnityEngine;
namespace LighthouseKeepers.Presentation.Environment
{
    // Environment-only weather. No gameplay, power or flood authority.
    public sealed class ShellWeatherPresentation : MonoBehaviour
    {
        [SerializeField] Transform head;
        [SerializeField] ParticleSystem rain;
        [SerializeField] Light lightning;
        [SerializeField] AudioSource exteriorRain,interiorRain,ocean,thunder;
        [SerializeField] AudioLowPassFilter thunderFilter;
        ParticleSystem.Particle[] particles;
        float nextStrike,flashStarted=-100,thunderAt=-1;
        public static bool Sheltered(Vector3 p)
        {
            float radius=p.y<9.9f?4.4f:3.1f;
            return (p.y<14.8f&&p.x*p.x+p.z*p.z<radius*radius)
                || (Mathf.Abs(p.x)<3.95f&&p.z> -10.3f&&p.z< -4.4f&&p.y<4.4f)
                || (Mathf.Abs(p.x)<.8f&&p.z> -5.6f&&p.z< -3.8f&&p.y<2.9f);
        }
        void Awake(){particles=new ParticleSystem.Particle[rain.main.maxParticles];nextStrike=Time.time+12;}
        void LateUpdate()
        {
            if(!head)return;
            rain.transform.position=head.position+Vector3.up*5;
            int count=rain.GetParticles(particles);
            for(int i=0;i<count;i++)
            {
                var p=particles[i].position;
                if(Sheltered(p)||(p.y>9.4f&&p.y<9.9f&&p.x*p.x+p.z*p.z<4.25f*4.25f))particles[i].remainingLifetime=0;
            }
            rain.SetParticles(particles,count);
            float outside=Sheltered(head.position)?0:1;
            exteriorRain.volume=Mathf.MoveTowards(exteriorRain.volume,outside*.22f,Time.deltaTime*.2f);
            interiorRain.volume=Mathf.MoveTowards(interiorRain.volume,(1-outside)*.12f,Time.deltaTime*.2f);
            ocean.volume=Mathf.MoveTowards(ocean.volume,Mathf.Lerp(.045f,.14f,outside),Time.deltaTime*.12f);
            thunderFilter.cutoffFrequency=Mathf.Lerp(1100,12000,outside);
            if(Time.time>=nextStrike){flashStarted=Time.time;nextStrike=Time.time+Random.Range(14f,28f);thunderAt=Time.time+Random.Range(1.2f,3.5f);}
            float age=Time.time-flashStarted;
            lightning.intensity=age<.07f?1.2f:age>.18f&&age<.26f?.6f:0;
            if(thunderAt>0&&Time.time>=thunderAt){thunder.Play();thunderAt=-1;}
        }
    }
}
