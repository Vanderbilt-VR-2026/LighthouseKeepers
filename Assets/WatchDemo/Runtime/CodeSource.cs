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
            var text=new StringBuilder("SOLO CALL CODE\n");
            for(int i=0;i<program.callCode.Length;i++){if(i>0)text.Append(" > ");text.Append(program.ColourName(program.callCode[i]));}
            readout.text=text.ToString();
        }
        void Start()=>Refresh();
    }
}
