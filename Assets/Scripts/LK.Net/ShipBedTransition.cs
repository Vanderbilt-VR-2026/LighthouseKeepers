using System;
using System.Collections;
using System.Reflection;
using LighthouseKeepers.Core.Progression;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
namespace LighthouseKeepers.Net
{
    /// <summary>A validated bed grip explicitly authorizes the brief fade and return to the House.</summary>
    public sealed class ShipBedTransition : MonoBehaviour
    {
        const int Kind=42;
        DayDirector days;
        XROrigin rig;
        FusionMessageChannel channel;
        EventInfo requested;
        MethodInfo complete;
        Action<int,bool> handler;
        CanvasGroup fade;
        bool moving;
        public void Bind(DayDirector director,XROrigin origin,FusionMessageChannel stream)
        {
            days=director;rig=origin;channel=stream;
            requested=days.GetType().GetEvent("BedTransitionRequested");complete=days.GetType().GetMethod("CompleteBedTransition",Type.EmptyTypes);
            if(requested!=null && complete!=null){handler=BedRequested;requested.AddEventHandler(days,handler);}
            if(channel)channel.Received+=Receive;
        }
        void BedRequested(int day,bool dawn)
        {
            if(moving || channel && !channel.IsHost)return;
            if(channel){var bytes=new byte[5];Buffer.BlockCopy(BitConverter.GetBytes(day),0,bytes,0,4);bytes[4]=(byte)(dawn?1:0);channel.Broadcast(Kind,bytes);}
            StartCoroutine(ReturnHouse(true));
        }
        void Receive(int sender,int kind,byte[] bytes)
        {
            if(sender!=0 || kind!=Kind || !channel || channel.IsHost || moving || bytes.Length!=5 || bytes[4]>1)return;
            int day=BitConverter.ToInt32(bytes,0);if(day<0 || day>7)return;
            StartCoroutine(ReturnHouse(false));
        }
        IEnumerator ReturnHouse(bool acknowledge)
        {
            moving=true;
            if(!fade)
            {
                var root=new GameObject("Bed transition fade",typeof(RectTransform),typeof(Canvas),typeof(CanvasGroup));root.transform.SetParent(rig.Camera.transform,false);
                var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=rig.Camera;
                canvas.planeDistance=rig.Camera.nearClipPlane+.01f;canvas.sortingOrder=32760;
                fade=root.GetComponent<CanvasGroup>();fade.blocksRaycasts=false;fade.alpha=0;
                var fill=new GameObject("Black",typeof(RectTransform),typeof(Image));fill.transform.SetParent(root.transform,false);
                var rect=fill.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
                var image=fill.GetComponent<Image>();image.color=Color.black;image.raycastTarget=false;
            }
            for(float t=0;t<.2f;t+=Time.unscaledDeltaTime){fade.alpha=Mathf.Clamp01(t/.2f);yield return null;}fade.alpha=1;
            var character=rig.GetComponent<CharacterController>();bool wasEnabled=character && character.enabled;if(character)character.enabled=false;
            var settings=ShipRouteSettings.Load();var spawn=settings.houseSpawn;if(channel)spawn.x+=(channel.LocalActorId&1)==0?.55f:-.55f;
            rig.transform.RotateAround(rig.Camera.transform.position,Vector3.up,Mathf.DeltaAngle(rig.Camera.transform.eulerAngles.y,settings.houseSpawnYaw));
            var head=rig.Camera.transform.position;rig.transform.position+=new Vector3(spawn.x-head.x,spawn.y-rig.transform.position.y,spawn.z-head.z);
            Physics.SyncTransforms();if(character)character.enabled=wasEnabled;
            if(acknowledge && days && complete!=null)complete.Invoke(days,null);
            for(float t=0;t<.2f;t+=Time.unscaledDeltaTime){fade.alpha=1-Mathf.Clamp01(t/.2f);yield return null;}fade.alpha=0;moving=false;
        }
        void OnDestroy()
        {
            if(days && requested!=null && handler!=null)requested.RemoveEventHandler(days,handler);
            if(channel)channel.Received-=Receive;if(fade)Destroy(fade.gameObject);
        }
    }
}
