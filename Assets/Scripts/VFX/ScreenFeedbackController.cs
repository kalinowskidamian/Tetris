using UnityEngine;

namespace Tetris.VFX
{
    public sealed class ScreenFeedbackController : MonoBehaviour
    {
        [SerializeField] private VFXFeedbackConfig feedbackConfig;

        public void TriggerFlash()
        {
            if (feedbackConfig == null)
            {
                Debug.LogWarning($"{nameof(ScreenFeedbackController)} is missing {nameof(VFXFeedbackConfig)}.");
            }
        }

        public void TriggerPulse()
        {
            if (feedbackConfig == null)
            {
                Debug.LogWarning($"{nameof(ScreenFeedbackController)} is missing {nameof(VFXFeedbackConfig)}.");
            }
        }

        public void TriggerLineClearFeedback()
        {
            if (feedbackConfig == null)
            {
                Debug.LogWarning($"{nameof(ScreenFeedbackController)} is missing {nameof(VFXFeedbackConfig)}.");
            }
        }
    }
}
