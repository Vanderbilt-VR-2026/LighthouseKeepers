using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
namespace LighthouseKeepers.Editor
{
 // Explicit editor-only access through Unity's signed-in Package Manager; no token handling.
 public static class LighthouseAssetStoreAccess
 {
  static double start;static bool finished;static readonly string output="Builds/AssetStoreAccess.txt";
  public static void InspectAccount()
  {
   Directory.CreateDirectory("Builds");File.WriteAllText(output,"Querying Unity Package Manager for Free Horror Ambience 2.\n");
   start=EditorApplication.timeSinceStartup;EditorApplication.update+=Timeout;
   try{
    var assembly=AppDomain.CurrentDomain.GetAssemblies().First(a=>a.GetType("UnityEditor.PackageManager.UI.Internal.ServicesContainer")!=null);
    var containerType=assembly.GetType("UnityEditor.PackageManager.UI.Internal.ServicesContainer");
    var instance=containerType.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);
    var type=assembly.GetType("UnityEditor.PackageManager.UI.Internal.IAssetStoreRestAPI");
    var service=containerType.GetMethod("Resolve").MakeGenericMethod(type).Invoke(instance,null);
    var method=type.GetMethod("GetPurchases");var args=method.GetParameters();
    var query=Activator.CreateInstance(args[0].ParameterType,new object[]{0,100,"Free Horror Ambience 2",null});
    method.Invoke(service,new[]{query,Callback(args[1].ParameterType,nameof(Success)),Callback(args[2].ParameterType,nameof(Failure))});
   }catch(Exception e){Finish("Access probe failed: "+e.GetBaseException().Message,1);}
  }
  static Delegate Callback(Type t,string method){var p=Expression.Parameter(t.GenericTypeArguments[0]);return Expression.Lambda(t,Expression.Call(typeof(LighthouseAssetStoreAccess).GetMethod(method,BindingFlags.Static|BindingFlags.Public),Expression.Convert(p,typeof(object))),p).Compile();}
  public static void Success(object value){
   // Return only the requested asset's purchase metadata, never account/authentication data.
   Finish("Purchase query succeeded: "+EditorJsonUtility.ToJson(value),0);
  }
  public static void Failure(object value){var p=value.GetType().GetProperty("message");Finish("Asset Store access error: "+p?.GetValue(value),1);}
  static void Timeout(){if(EditorApplication.timeSinceStartup-start>45)Finish("Asset Store account query timed out after 45 seconds.",1);}
  static void Finish(string message,int code){if(finished)return;finished=true;File.AppendAllText(output,message+"\n");EditorApplication.update-=Timeout;EditorApplication.Exit(code);}
 }
}
