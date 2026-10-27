using System;
using System.Collections.Generic;
using LighthouseKeepers.Core.Progression;

namespace LighthouseKeepers.Presentation.Board
{
    public static class DayBoardContent
    {
        public static string[] Build(IDayDirector director, string heading, string briefing, DayStage stage,
            float elapsedSeconds, DayResult result, bool hasNextDay = true)
        {
            var lines = new List<string> { heading, "" };
            if (stage == DayStage.Results || stage == DayStage.Failed)
            {
                lines.Add(result.Succeeded ? "Watch survived" : "The lighthouse flooded");
                lines.Add("Day " + result.Day + " results");
                lines.Add("Time: " + TimeText(result.Seconds));
                lines.Add("Peak water: " + result.PeakWaterHeightM.ToString("F2") + " m");
                lines.Add("Pump strokes: " + result.PumpStrokes);
                lines.Add("Generator restarts: " + result.GeneratorRestarts);
                lines.Add(result.Succeeded ? (hasNextDay ? "Continue when you are ready." : "All watches survived. Rest well.") : "Retry this day. No days lost.");
                return lines.ToArray();
            }
            lines.Add(director.CurrentObjectiveText);
            lines.Add("");
            if (stage == DayStage.WaitingForCrew || stage == DayStage.Inactive) return lines.ToArray();
            if (elapsedSeconds < 8 && !string.IsNullOrWhiteSpace(briefing)) { lines.Add(briefing); lines.Add(""); }
            foreach (var step in director.Steps) lines.Add((step.Complete ? "[x] " : "[ ] ") + step.Text);
            lines.Add("Watch time: " + TimeText(elapsedSeconds));
            return lines.ToArray();
        }

        static string TimeText(float seconds)
        {
            int whole = Math.Max(0, (int)seconds);
            return (whole / 60).ToString("00") + ":" + (whole % 60).ToString("00");
        }
    }
}
