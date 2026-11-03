using LighthouseKeepers.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.XR;

namespace LighthouseKeepers.WatchDemo
{
    // Runs after the constraint computes travel, before HandRig's visual LateUpdate.
    // Only a selected starter changes pose. Tracked controllers and free-hand rotation stay untouched.
    [DefaultExecutionOrder(100)]
    public sealed class S4StarterGrip : MonoBehaviour
    {
        PhysicalGrip grip;
        Transform left, right;
        void Awake()
        {
            grip=GetComponent<PhysicalGrip>();
            left=Anchor("Starter left palm",-.014f);right=Anchor("Starter right palm",.014f);
        }
        Transform Anchor(string name,float x)
        {
            var t=new GameObject(name).transform;t.SetParent(transform,false);
            // Bar across X, fingers into engine (-Z), knuckles up (+Y).
            t.localPosition=new Vector3(x,.012f,.020f);t.localRotation=Quaternion.Euler(0,180,0);return t;
        }
        void Update()
        {
            var hand=grip.HoldingHand;if(!hand)return;
            hand.SetConstrainedGrip(hand.Node==XRNode.LeftHand?left:right);
        }
        void OnDisable(){if(grip&&grip.HoldingHand)grip.HoldingHand.ClearPose();}
    }
}
