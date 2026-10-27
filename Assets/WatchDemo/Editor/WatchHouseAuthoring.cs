using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.XR.CoreUtils;

namespace LighthouseKeepers.WatchDemo.Editor
{
    /// <summary>Shared House authoring used by both the original builder and ship migration.</summary>
    public static class WatchHouseAuthoring
    {
        public const string WallName="UCX_SM_Wall_House_Side_N1_01";
        public const string BoardName="House wall briefing board";
        public static void CleanHud()
        {
            var camera=Camera.main;var hud=camera.transform.Find("Watch camera HUD");
            var overlay=camera.transform.Find("Camera recording overlay");if(overlay)UnityEngine.Object.DestroyImmediate(overlay.gameObject);
            var controller=new SerializedObject(UnityEngine.Object.FindFirstObjectByType<WatchDemoController>());
            var clock=(TextMesh)controller.FindProperty("clock").objectReferenceValue;
            foreach(Transform child in hud.Cast<Transform>().ToArray())
                if(child!=clock.transform && child.name!="Sleep fade only")UnityEngine.Object.DestroyImmediate(child.gameObject);
            clock.name="Watch readout";clock.transform.localPosition=new Vector3(0,-.30f,.9f);
            clock.characterSize=.006f;clock.fontSize=38;clock.anchor=TextAnchor.MiddleCenter;clock.alignment=TextAlignment.Center;
            clock.text="D1   07:00   0/1 TASKS";
            var fade=hud.Find("Sleep fade only").GetComponent<Renderer>();fade.enabled=false;
        }
        public static void FitWrench()
        {
            var tool=GameObject.Find("Grab wrench — grip to hold");var top=GameObject.Find("Wrench table top").GetComponent<Collider>();
            var visual=tool.transform.Find("Wrench visual");
            foreach(Transform child in visual.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
            visual.localScale=Vector3.one;visual.localPosition=Vector3.zero;visual.localRotation=Quaternion.identity;
            const string path="Assets/WatchDemo/WrenchMesh.asset";var mesh=WatchWrenchMesh.Create();var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved){EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);}else{AssetDatabase.CreateAsset(mesh,path);saved=mesh;}
            var filter=visual.GetComponent<MeshFilter>();if(!filter)filter=visual.gameObject.AddComponent<MeshFilter>();filter.sharedMesh=saved;
            var renderer=visual.GetComponent<MeshRenderer>();if(!renderer)renderer=visual.gameObject.AddComponent<MeshRenderer>();
            var steel=AssetDatabase.LoadAssetAtPath<Material>("Assets/WatchDemo/WrenchMetal.mat");steel.SetColor("_BaseColor",new Color(.68f,.70f,.72f));steel.SetFloat("_Metallic",.2f);steel.SetFloat("_Smoothness",.35f);EditorUtility.SetDirty(steel);renderer.sharedMaterial=steel;renderer.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            foreach(var c in tool.GetComponents<Collider>())UnityEngine.Object.DestroyImmediate(c);
            var handle=tool.AddComponent<BoxCollider>();handle.center=new Vector3(0,0,-.026f);handle.size=new Vector3(.022f,.010f,.188f);
            var jaw=tool.AddComponent<BoxCollider>();jaw.center=new Vector3(0,0,.099f);jaw.size=new Vector3(.072f,.010f,.062f);
            var body=tool.GetComponent<Rigidbody>();body.isKinematic=true;body.useGravity=true;body.mass=.3f;
            var old=tool.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if(!(old is WatchWrenchGrab)){var pose=tool.GetComponent<WatchWrenchHandPose>();var haptics=tool.GetComponent<WrenchHaptics>();
                if(pose)UnityEngine.Object.DestroyImmediate(pose);if(haptics)UnityEngine.Object.DestroyImmediate(haptics);
                UnityEngine.Object.DestroyImmediate(old);old=tool.AddComponent<WatchWrenchGrab>();
                tool.AddComponent<WatchWrenchHandPose>();tool.AddComponent<WrenchHaptics>();}
            var grab=(WatchWrenchGrab)old;grab.useDynamicAttach=false;grab.throwOnDetach=true;grab.movementType=UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Kinematic;
            grab.colliders.Clear();grab.colliders.Add(handle);grab.colliders.Add(jaw);
            Transform Attach(string name,float yaw){var t=tool.transform.Find(name);if(!t){t=new GameObject(name).transform;t.SetParent(tool.transform,false);}t.localPosition=new Vector3(0,0,-.0325f);t.localRotation=Quaternion.Euler(0,yaw,0);return t;}
            var data=new SerializedObject(grab);data.FindProperty("leftAttach").objectReferenceValue=Attach("Handle centre — left",-79);data.FindProperty("rightAttach").objectReferenceValue=Attach("Handle centre — right",79);data.ApplyModifiedPropertiesWithoutUndo();
            if(!tool.GetComponent<WatchWrenchReturn>())tool.AddComponent<WatchWrenchReturn>();
            var controller=new SerializedObject(UnityEngine.Object.FindFirstObjectByType<WatchDemoController>());controller.FindProperty("wrench").objectReferenceValue=grab;controller.ApplyModifiedPropertiesWithoutUndo();
            Physics.SyncTransforms();tool.transform.SetPositionAndRotation(new Vector3(top.bounds.center.x,top.bounds.max.y+.005f+.001f,top.bounds.center.z),Quaternion.Euler(0,20,0));
        }
        public static void CleanHouse()
        {
            var reference=GameObject.Find("PF_Shell_ScaleReference");
            if(reference)UnityEngine.Object.DestroyImmediate(reference);
            var wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/WatchDemo/Wood.mat");
            wood.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_LighthouseKeepers/Art/Textures/LK_Wood_Wear.png"));
            wood.SetColor("_BaseColor",new Color(.42f,.27f,.14f));wood.SetColor("_Color",new Color(.42f,.27f,.14f));
            wood.SetFloat("_Smoothness",.18f);EditorUtility.SetDirty(wood);
            var top=GameObject.Find("Wrench table top").transform;
            top.SetPositionAndRotation(new Vector3(-3.02f,.82f,-8.4f),Quaternion.Euler(0,90,0));
            top.GetComponent<Renderer>().sharedMaterial=wood;
            int i=0;
            foreach(var leg in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Wrench table leg").OrderBy(t=>t.GetSiblingIndex()))
            {leg.position=new Vector3(-3.02f+(i%2==0?-.26f:.26f),.38f,-8.4f+(i<2?-.67f:.67f));leg.GetComponent<Renderer>().sharedMaterial=wood;i++;}
        }
        public static void MountBoard()
        {
            var wall=GameObject.Find(WallName).GetComponent<Collider>();
            Physics.SyncTransforms();
            var root=GameObject.Find(BoardName);
            if(!root)root=new GameObject(BoardName);
            root.transform.SetParent(GameObject.Find("Watch demo").transform,false);
            // The nearest wall is the west wall. +X is its inward normal; board back is local Z=-.05.
            root.transform.SetPositionAndRotation(new Vector3(wall.bounds.max.x+.01f+.05f,1.5f,-8.4f),Quaternion.LookRotation(Vector3.right));
            Place("Wall-mounted checklist brown frame",root.transform,Vector3.zero,Vector3.zero,new Vector3(1.95f,1.22f,.10f));
            Place("Wall-mounted checklist white face",root.transform,new Vector3(0,0,.065f),Vector3.zero,new Vector3(1.76f,1.02f,.024f));
            // TextMesh's readable face is -Z. Rotate only its local frame to face into the room.
            var text=GameObject.Find("Watch checklist").GetComponent<TextMesh>();
            text.transform.SetParent(root.transform,false);text.transform.localPosition=new Vector3(.80f,.44f,.080f);
            text.transform.localRotation=Quaternion.Euler(0,180,0);text.transform.localScale=Vector3.one;
            text.characterSize=.017f;text.fontSize=34;text.fontStyle=FontStyle.Normal;text.color=new Color(.012f,.018f,.014f);
            var origin=UnityEngine.Object.FindFirstObjectByType<XROrigin>();
            origin.transform.SetPositionAndRotation(new Vector3(-.9f,.04f,-8.4f),Quaternion.LookRotation(Vector3.left));
            PrefabUtility.RecordPrefabInstancePropertyModifications(origin.transform);
        }
        static void Place(string name,Transform parent,Vector3 position,Vector3 rotation,Vector3 scale)
        {
            var t=GameObject.Find(name).transform;t.SetParent(parent,false);t.localPosition=position;t.localRotation=Quaternion.Euler(rotation);t.localScale=scale;
        }
    }
}
