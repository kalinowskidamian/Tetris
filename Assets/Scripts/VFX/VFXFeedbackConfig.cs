using UnityEngine;

namespace Tetris.VFX
{
    [CreateAssetMenu(fileName = "VFXFeedbackConfig", menuName = "Tetris/Config/VFX Feedback Config")]
    public sealed class VFXFeedbackConfig : ScriptableObject
    {
        [Header("Screen Flash")]
        [SerializeField, Min(0f)] private float flashDuration = 0.12f;
        [SerializeField, Range(0f, 2f)] private float flashIntensity = 0.8f;

        [Header("Pulse")]
        [SerializeField, Min(0f)] private float pulseDuration = 0.25f;
        [SerializeField, Range(0f, 2f)] private float pulseScaleStrength = 0.1f;

        [Header("Line Clear Feedback")]
        [SerializeField, Min(0f)] private float lineClearKickDuration = 0.18f;
        [SerializeField, Range(0f, 2f)] private float lineClearKickIntensity = 1f;

        public float FlashDuration => flashDuration;
        public float FlashIntensity => flashIntensity;
        public float PulseDuration => pulseDuration;
        public float PulseScaleStrength => pulseScaleStrength;
        public float LineClearKickDuration => lineClearKickDuration;
        public float LineClearKickIntensity => lineClearKickIntensity;
    }
}
