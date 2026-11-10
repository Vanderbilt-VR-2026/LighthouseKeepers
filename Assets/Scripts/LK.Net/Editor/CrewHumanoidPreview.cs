using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.Net.Editor
{
    /// <summary>Isolated, unsaved scene to inspect the actual runtime skin and solver without Photon/XR hardware.</summary>
    public sealed class CrewHumanoidPreview : EditorWindow
    {
        CrewHumanoidAvatar avatar;
        float crouch,grip;
        bool walking,reaching;
        double lastTime;
        float clock;
        [MenuItem("Lighthouse Keepers/Sprint 4/Avatars/Preview full-body fisherman")]
        static void Open()
        {
            if(EditorApplication.isPlaying||!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Avatars/PF_RiggedFisherman.prefab");
            if(!prefab){Debug.LogError("Full-body fisherman prefab is missing.");return;}
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var model=Instantiate(prefab);model.name="Fisherman pose preview (unsaved)";
            var avatar=model.GetComponent<CrewHumanoidAvatar>();
            if(!avatar.TryInitialize(out var error)){Debug.LogError(error,avatar);DestroyImmediate(model);return;}
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="Preview ground";
            floor.transform.position=new Vector3(0,-.05f,0);floor.transform.localScale=new Vector3(8,.1f,8);
            var light=new GameObject("Preview light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=2;
            light.transform.rotation=Quaternion.Euler(45,-25,0);
            RenderSettings.ambientLight=new Color(.45f,.45f,.45f);
            Physics.SyncTransforms();
            var window=GetWindow<CrewHumanoidPreview>("Fisherman poses");window.avatar=avatar;window.clock=0;
            window.Tick();Selection.activeGameObject=model;
            SceneView.lastActiveSceneView?.Frame(new Bounds(Vector3.up*.9f,new Vector3(2.8f,2.4f,2.8f)),false);
        }
        void OnEnable(){lastTime=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}
        void OnDisable()=>EditorApplication.update-=Tick;
        void OnGUI()
        {
            EditorGUILayout.HelpBox("Uses the runtime skin and pose solver with synthetic tracking. Inspect in Scene view. This does not verify Fusion, controllers or headset performance.",MessageType.Info);
            if(!avatar){EditorGUILayout.LabelField("Reopen the preview menu to create a new scene.");return;}
            crouch=EditorGUILayout.Slider("Crouch (metres)",crouch,0,.55f);
            grip=EditorGUILayout.Slider("Grip / finger curl",grip,0,1);
            reaching=EditorGUILayout.Toggle("Reach forward",reaching);
            walking=EditorGUILayout.Toggle("Walk in a circle",walking);
        }
        void Tick()
        {
            double now=EditorApplication.timeSinceStartup;float dt=Mathf.Clamp((float)(now-lastTime),0,.05f);lastTime=now;
            if(!avatar||EditorApplication.isPlaying)return;
            if(walking)clock+=dt;
            float angle=clock*.7f;
            var root=walking?new Vector3(Mathf.Sin(angle)*.7f,0,Mathf.Cos(angle)*.7f):Vector3.zero;
            var yaw=Quaternion.Euler(0,walking?angle*Mathf.Rad2Deg+90:0,0);
            var pose=new KeeperPoseFrame{Actor=1,Sequence=1,Head=root+Vector3.up*(1.665f-crouch),HeadRotation=yaw,
                Left=root+yaw*new Vector3(-.28f,1.12f-crouch,reaching?.48f:.12f),
                Right=root+yaw*new Vector3(.28f,1.12f-crouch,reaching?.48f:.12f),
                LeftRotation=yaw,RightRotation=yaw,LeftGrip=grip,RightGrip=grip,LeftTrigger=grip,RightTrigger=grip};
            avatar.ApplyPose(pose,dt);SceneView.RepaintAll();
        }
    }
}
