using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Shared small material palette and simple physical fittings for the switched stations.
    public sealed class M2StationParts : MonoBehaviour
    {
        readonly List<Material> owned=new();
        public Material Metal,Dark,Brass,Red,Green;
        public void Initialize()
        {Metal=Mat("steel",new Color(.4f,.43f,.4f),.7f);Dark=Mat("instrument enamel",new Color(.045f,.075f,.07f),.3f);Brass=Mat("brass",new Color(.65f,.38f,.09f),.7f);Red=Mat("fuel red",new Color(.65f,.035f,.025f),.2f);Green=Mat("status green",new Color(.05f,.7f,.1f),.1f);}
        Material Mat(string name,Color colour,float metal)
        {var template=Object.FindFirstObjectByType<WatchGenerator>().GetComponentInChildren<Renderer>().sharedMaterial;var m=new Material(template){name="M2 "+name};m.SetColor("_BaseColor",colour);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",.4f);m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",colour*.14f);owned.Add(m);return m;}
        public static Transform Root(string name,Transform parent,Vector3 position)
        {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;}
        public GameObject Part(string name,Transform parent,Vector3 pos,Vector3 scale,Material material,PrimitiveType shape=PrimitiveType.Cube)
        {var o=GameObject.CreatePrimitive(shape);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=scale;var c=o.GetComponent<Collider>();c.enabled=false;Destroy(c);var r=o.GetComponent<Renderer>();r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return o;}
        public TextMesh Text(string name,string text,Transform parent,Vector3 position,float size=.0035f)
        {var t=Root(name,parent,position);t.localRotation=Quaternion.Euler(0,180,0);var label=t.gameObject.AddComponent<TextMesh>();label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=64;label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=new Color(.95f,.89f,.7f);label.text=text;label.GetComponent<Renderer>().sharedMaterial=label.font.material;return label;}
        void OnDestroy(){foreach(var m in owned)if(m)Destroy(m);}
    }
}
