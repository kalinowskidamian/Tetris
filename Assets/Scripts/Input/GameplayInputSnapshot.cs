namespace Tetris.Input
{
    public readonly struct GameplayInputSnapshot
    {
        public GameplayInputSnapshot(bool tapRequested, bool holdRequested, GestureKind gestureKind)
        {
            TapRequested = tapRequested;
            HoldRequested = holdRequested;
            GestureKind = gestureKind;
        }

        public bool TapRequested { get; }
        public bool HoldRequested { get; }
        public GestureKind GestureKind { get; }
    }
}
