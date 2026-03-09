using UnityEngine;

namespace Tetris.Input
{
    public sealed class GameplayInputRouter : MonoBehaviour
    {
        [Header("Planned Gesture Thresholds")]
        [SerializeField, Min(0f)] private float tapMaxDuration = 0.2f;
        [SerializeField, Min(0f)] private float holdMinDuration = 0.35f;
        [SerializeField, Min(1f)] private float swipeMinDistancePixels = 64f;

        public float TapMaxDuration => tapMaxDuration;
        public float HoldMinDuration => holdMinDuration;
        public float SwipeMinDistancePixels => swipeMinDistancePixels;

        public IGameplayInputSource InputSource { get; private set; }

        public void BindSource(IGameplayInputSource source)
        {
            InputSource = source;
        }

        public bool TryReadSnapshot(out GameplayInputSnapshot snapshot)
        {
            if (InputSource == null)
            {
                snapshot = default;
                return false;
            }

            snapshot = InputSource.ReadSnapshot();
            return true;
        }
    }
}
