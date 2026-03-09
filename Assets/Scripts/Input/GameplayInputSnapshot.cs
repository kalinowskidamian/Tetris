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

        public bool HasAnyAction => TapRequested || HoldRequested || GestureKind != GestureKind.None;

        public GameplayInputSnapshot Merge(GameplayInputSnapshot other)
        {
            var mergedTap = TapRequested || other.TapRequested;
            var mergedHold = HoldRequested || other.HoldRequested;
            var mergedGesture = GestureKind != GestureKind.None ? GestureKind : other.GestureKind;
            return new GameplayInputSnapshot(mergedTap, mergedHold, mergedGesture);
        }
    }
}
