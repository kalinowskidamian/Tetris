using Tetris.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Tetris.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public sealed class CanvasScalerBinder : MonoBehaviour
    {
        private const string RegistryResourcePath = "Configs/ProjectConfigRegistry";

        [SerializeField] private UIThemeConfig uiThemeConfig;
        private CanvasScaler canvasScaler;

        private void Awake()
        {
            canvasScaler = GetComponent<CanvasScaler>();
            if (uiThemeConfig == null)
            {
                ProjectConfigRegistry registry = Resources.Load<ProjectConfigRegistry>(RegistryResourcePath);
                uiThemeConfig = registry != null ? registry.UIThemeConfig : null;
            }

            if (uiThemeConfig == null)
            {
                Debug.LogWarning($"{nameof(CanvasScalerBinder)} is missing {nameof(UIThemeConfig)}.");
                return;
            }

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = uiThemeConfig.ReferenceResolution;
            canvasScaler.matchWidthOrHeight = uiThemeConfig.MatchWidthOrHeight;
        }
    }
}
