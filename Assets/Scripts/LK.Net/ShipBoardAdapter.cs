using System;
using System.Linq;
using System.Reflection;
using LighthouseKeepers.Core.Progression;
using UnityEngine;
namespace LighthouseKeepers.Net
{
    /// <summary>Adapts D's existing line callback to C's typed checkbox renderer. No UI geometry or watch rules.</summary>
    public sealed class ShipBoardAdapter : MonoBehaviour
    {
        MonoBehaviour view;
        MethodInfo writeRows;
        public IDayDirector Projection {get;set;}
        public void Bind(MonoBehaviour renderer)
        {
            view=renderer;
            writeRows=view.GetType().GetMethod("SetLines",new[]{typeof(string),typeof(System.Collections.Generic.IReadOnlyList<(string,bool)>)});
            if(writeRows==null)throw new InvalidOperationException("C's house board must expose SetLines(title, checkbox rows).");
        }
        public void SetLines(string[] presentation)
        {
            var source=Projection??FindAnyObjectByType<DayDirector>();
            if(source==null || !view || writeRows==null)return;
            writeRows.Invoke(view,new object[]{"KEEPER WATCH · DAY "+source.CurrentDay,source.Steps.Select(step=>(step.Text,step.Complete)).ToArray()});
        }
    }
}
