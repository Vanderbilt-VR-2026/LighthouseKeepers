using System.Text;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Relocate this clue object for later co-op; the console owns no duplicate visible code.
    public sealed class CodeSource : MonoBehaviour
    {
        [SerializeField] RadioProgram program;
        [SerializeField] TextMesh readout;
        public RadioProgram Program=>program;
        public void Refresh()
        {
            var text=new StringBuilder("STORM WARNING / CALL CARD\nCHANNEL ");text.Append(program.targetMHz.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)).Append(" MHz\n");
            for(int i=0;i<program.callCode.Length;i++){if(i>0)text.Append(" > ");text.Append(program.ColourName(program.callCode[i]));}
            text.Append("\nLOCK > CODE > HOLD MIC TRIGGER");readout.text=text.ToString();
        }
        void Start()=>Refresh();
    }
}
