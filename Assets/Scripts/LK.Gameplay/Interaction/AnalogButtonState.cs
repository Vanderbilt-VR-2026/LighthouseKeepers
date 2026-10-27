namespace LighthouseKeepers.Gameplay.Interaction
{
    // XRI StateChange needs frame edges as well as a performed flag; analog jitter gets hysteresis.
    public sealed class AnalogButtonState
    {
        public bool Pressed { get; private set; }
        public int PressedFrame { get; private set; } = -1;
        public int ReleasedFrame { get; private set; } = -1;
        public void Update(float value, int frame)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) value = 0;
            bool next = Pressed ? value > .45f : value >= .55f;
            if (next != Pressed)
            { if (next) PressedFrame = frame; else ReleasedFrame = frame; }
            Pressed = next;
        }
    }
}
