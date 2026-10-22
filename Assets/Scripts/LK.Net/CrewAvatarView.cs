using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
namespace LighthouseKeepers.Net
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class CrewAvatarView : MonoBehaviour
    {
        [SerializeField] GameObject playerBody;
        public UnityEvent<int,float,float> LeftPoseChanged=new(),RightPoseChanged=new();
        NetworkObject identity;
        GameObject visuals;
        RemoteGlove left,right;
        RemoteKeeperBody body;
        TextMeshPro nameplate;
        Camera observer;
        float nextName;
        void Awake()
        {
            identity=GetComponent<NetworkObject>();
            visuals=new GameObject("Remote keeper / authored body and gloves");visuals.transform.SetParent(transform,false);
            left=Glove(true);right=Glove(false);
            var source=playerBody?playerBody:Resources.Load<GameObject>("Hands/PF_PlayerBody");
            if(source)
            {
                var root=new GameObject("Remote body pose");root.transform.SetParent(visuals.transform,false);root.SetActive(false);
                var appearance=Instantiate(source,root.transform,false);body=root.AddComponent<RemoteKeeperBody>();body.Bind(appearance);root.SetActive(true);
            }
            else Debug.LogWarning("A must publish Resources/Hands/PF_PlayerBody. Remote hands render; no primitive body substitute is created.",this);
            nameplate=new GameObject("Keeper name").AddComponent<TextMeshPro>();nameplate.transform.SetParent(visuals.transform,false);
            nameplate.font=TMP_Settings.defaultFontAsset;nameplate.fontSize=2;nameplate.rectTransform.sizeDelta=new Vector2(2,.3f);
            nameplate.alignment=TextAlignmentOptions.Center;nameplate.richText=false;nameplate.textWrappingMode=TextWrappingModes.NoWrap;
            visuals.SetActive(false);
        }
        RemoteGlove Glove(bool isLeft)
        {
            var hand=new GameObject(isLeft?"Remote left glove":"Remote right glove");hand.transform.SetParent(visuals.transform,false);
            var result=hand.AddComponent<RemoteGlove>();result.Prepare(isLeft);return result;
        }
        void LateUpdate()
        {
            var stream=CrewPoseStream.Active;
            bool visible=identity && identity.IsValid && stream && identity.Runner==stream.Channel.Runner &&
                identity.InputAuthority!=identity.Runner.LocalPlayer && stream.Sample(identity.InputAuthority.RawEncoded,out _);
            visuals.SetActive(visible);if(!visible || !stream.Sample(identity.InputAuthority.RawEncoded,out var p))return;
            left.transform.SetPositionAndRotation(p.Left,p.LeftRotation);right.transform.SetPositionAndRotation(p.Right,p.RightRotation);
            left.Apply(p.LeftPose,p.LeftGrip,p.LeftTrigger);right.Apply(p.RightPose,p.RightGrip,p.RightTrigger);
            if(body)body.Apply(p);
            LeftPoseChanged.Invoke(p.LeftPose,p.LeftGrip,p.LeftTrigger);RightPoseChanged.Invoke(p.RightPose,p.RightGrip,p.RightTrigger);
            if(!observer)observer=Camera.main;if(!observer)return;
            nameplate.transform.SetPositionAndRotation(p.Head+Vector3.up*.35f,observer.transform.rotation);
            nameplate.alpha=Mathf.Clamp01(7-Vector3.Distance(observer.transform.position,p.Head));
            if(Time.unscaledTime>=nextName){nextName=Time.unscaledTime+1;nameplate.text=stream.NameFor(identity.InputAuthority.RawEncoded);}
        }
    }
}
