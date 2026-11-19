using UnityEngine;using UnityEditor;using System.Collections;using System.IO;using LighthouseKeepers.WatchDemo;
public class BulbPhotos:MonoBehaviour {
 string path;public static object Begin(string path){EditorUtility.audioMasterMute=true;var p=new GameObject("Bulb photos").AddComponent<BulbPhotos>();p.path=path;return true;}
 IEnumerator Start(){var w=FindAnyObjectByType<WatchDemoController>();w.State.Lanterns.Extinguish(0);w.State.Lanterns.Extinguish(1);w.State.Grid.Reset(20,15,true);Directory.CreateDirectory(path);
 var poses=new[]{new Vector3(-2.4f,1.8f,-7.3f),new Vector3(2.6f,1.8f,-.5f),new Vector3(-.2f,4.8f,1.7f),new Vector3(1.7f,8.1f,-.1f),new Vector3(-1.4f,11.7f,.8f)};
 for(int i=0;i<5;i++){var p=LanternRoomLighting.RoomLights[i];var overlaps=Physics.OverlapSphere(p,.08f,~0,QueryTriggerInteraction.Ignore);File.AppendAllText(path+"/mounts.txt","Room "+i+": bulb overlaps="+overlaps.Length+"\n");while(LanternRoomLighting.Filament(Time.time,i)<.9f)yield return null;yield return new WaitForEndOfFrame();var cam=new GameObject("Bulb evidence camera").AddComponent<Camera>();cam.enabled=false;cam.transform.position=poses[i];cam.transform.LookAt(p);cam.fieldOfView=55;cam.nearClipPlane=.02f;var rt=new RenderTexture(900,900,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var t=new Texture2D(900,900,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,900,900),0,0);t.Apply();File.WriteAllBytes(path+"/bulb-"+i+".png",t.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;Destroy(t);Destroy(rt);Destroy(cam.gameObject);}
 File.WriteAllText(path+"/done","done");Destroy(gameObject);
 }
}
