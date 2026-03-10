namespace Tetris.Input
{
    public readonly struct GameplayInputSnapshot
    {
        public GameplayInputSnapshot(bool tapRequested, bool holdRequested, bool pauseRequested, GestureKind gestureKind)
        {
            TapRequested = tapRequested;
            HoldRequested = holdRequested;
            PauseRequested = pauseRequested;
            GestureKind = gestureKind;
        }

        public bool TapRequested { get; }
        public bool HoldRequested { get; }
        public bool PauseRequested { get; }
        public GestureKind GestureKind { get; }

        public bool HasAnyAction => TapRequested || HoldRequested || PauseRequested || GestureKind != GestureKind.None;

        public GameplayInputSnapshot Merge(GameplayInputSnapshot other)
        {
            var mergedTap = TapRequested || other.TapRequested;
            var mergedHold = HoldRequested || other.HoldRequested;
            var mergedPause = PauseRequested || other.PauseRequested;
            var mergedGesture = GestureKind != GestureKind.None ? GestureKind : other.GestureKind;
            return new GameplayInputSnapshot(mergedTap, mergedHold, mergedPause, mergedGesture);
        }
    }
}
