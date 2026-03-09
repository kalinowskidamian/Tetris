using Tetris.Core;
using UnityEngine;

namespace Tetris.VFX
{
    public sealed class ScreenFeedbackController : MonoBehaviour
    {
        private const string RegistryResourcePath = "Configs/ProjectConfigRegistry";

        [SerializeField] private VFXFeedbackConfig feedbackConfig;

        private void Awake()
        {
            ResolveConfigIfNeeded();
        }

        private void ResolveConfigIfNeeded()
        {
            if (feedbackConfig != null)
            {
                return;
            }

            ProjectConfigRegistry registry = Resources.Load<ProjectConfigRegistry>(RegistryResourcePath);
            feedbackConfig = registry != null ? registry.VFXFeedbackConfig : null;
        }


        public void TriggerFlash()
        {
            if (!HasConfig())
            {
                return;
            }
        }

        public void TriggerPulse()
        {
            if (!HasConfig())
            {
                return;
            }
        }

        public void TriggerLineClearFeedback()
        {
            if (!HasConfig())
            {
                return;
            }
        }

        private bool HasConfig()
        {
            ResolveConfigIfNeeded();
            if (feedbackConfig != null)
            {
                return true;
            }

            Debug.LogWarning($"{nameof(ScreenFeedbackController)} is missing {nameof(VFXFeedbackConfig)}.");
            return false;
        }
    }
}
