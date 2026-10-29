using Unity.XR.CoreUtils;
using UnityEngine;
namespace LighthouseKeepers.Net
{
    public sealed class RemoteHandVisual : MonoBehaviour
    {
        public bool left;
        float nextTry;
        void Update()
        {
            if(Time.unscaledTime<nextTry)return;nextTry=Time.unscaledTime+1;
            var rig=FindAnyObjectByType<XROrigin>();if(!rig)return;
            Transform controller=null;
            foreach(var t in rig.GetComponentsInChildren<Transform>(true))
                if(t.name==(left?"Left Controller":"Right Controller")){controller=t;break;}
            if(!controller)return;
            foreach(var skin in controller.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if(!skin.sharedMesh || skin.name!="Skinned glove")continue;
                var clone=Instantiate(skin.gameObject,transform);clone.name="Remote tan glove visual only";
                clone.transform.localPosition=controller.InverseTransformPoint(skin.transform.position);
                clone.transform.localRotation=Quaternion.Inverse(controller.rotation)*skin.transform.rotation;
                clone.transform.localScale=skin.transform.lossyScale;
                foreach(var component in clone.GetComponentsInChildren<Behaviour>(true))component.enabled=false;
                foreach(var collider in clone.GetComponentsInChildren<Collider>(true))collider.enabled=false;
                enabled=false;return;
            }
        }
    }
}
