namespace LighthouseKeepers.Gameplay.Interaction
{
    // Physical readiness only; D/B retain day/run/power authority.
    public sealed class GeneratorStartState
    {
        public int PrimerStrokes { get; private set; }
        public void Prime() { if (PrimerStrokes < 3) PrimerStrokes++; }
        public bool TryStart(bool chokeCorrect)
        {
            if (PrimerStrokes < 3 || !chokeCorrect) return false;
            PrimerStrokes = 0; return true;
        }
        public void Reset() => PrimerStrokes = 0;
    }
}
