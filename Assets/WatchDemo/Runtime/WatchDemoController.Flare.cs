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
            for(int i=0;i<2;i++)flareGuns[i]=WatchFlareGun.Create(this,i);
            FlareFlight.Create(this);
            var rack=new GameObject("House emergency flare rack");rack.transform.SetParent(transform,false);rack.transform.SetPositionAndRotation(new Vector3(-2.20f,.98f,-6.01f),Quaternion.Euler(0,90,0));
            var p=rack.AddComponent<M2StationParts>();p.Initialize();
            p.Part("Bolted flare shelf",rack.transform,new Vector3(0,-.14f,0),new Vector3(.70f,.035f,.25f),p.Dark);
            p.Part("Flare rack back",rack.transform,new Vector3(0,.015f,-.11f),new Vector3(.70f,.32f,.035f),p.Dark);
            p.Text("Flare rack stencil",KeeperStrings.Get("flare.rack"),rack.transform,new Vector3(0,.20f,-.085f),.003f);
            p.Text("Flare input stencil",KeeperStrings.Get("flare.input"),rack.transform,new Vector3(0,-.12f,.13f),.002f);
        }
        public bool FireFlare(int slot)=>slot>=0&&slot<2&&flareGuns[slot]&&flareGuns[slot].HostFire();
        public Component FlareTarget(float slot)=>float.IsFinite(slot)&&slot==(int)slot&&slot>=0&&slot<2?flareGuns[(int)slot]?.Grab:null;
    }
}
