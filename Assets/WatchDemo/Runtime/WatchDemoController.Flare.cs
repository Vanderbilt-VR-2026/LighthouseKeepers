using UnityEngine;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchDemoController
    {
        readonly WatchFlareGun[] flareGuns=new WatchFlareGun[2];
        void CreateFlares()
        {
            if(!state.Sprint5||!state.Sprint5.flareDefense)return;
            flareGuns[0]=WatchFlareGun.Create(this,0);
            FlareFlight.Create(this);
        }
        public bool FireFlare(int slot)=>slot>=0&&slot<2&&flareGuns[slot]&&flareGuns[slot].HostFire();
        public Component FlareTarget(float slot)=>float.IsFinite(slot)&&slot==(int)slot&&slot>=0&&slot<2?flareGuns[(int)slot]?.Grab:null;
    }
}
