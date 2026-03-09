using Tetris.Core;
using UnityEngine;

namespace Tetris.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private bool applyOnAwake = true;
        private const string RegistryResourcePath = "Configs/ProjectConfigRegistry";

        [SerializeField] private UIThemeConfig uiThemeConfig;

        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            ResolveThemeConfig();

            if (applyOnAwake)
            {
                ApplySafeArea();
            }
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
            {
                ApplySafeArea();
            }
        }

        private void ResolveThemeConfig()
        {
            if (uiThemeConfig != null)
            {
                return;
            }

            ProjectConfigRegistry registry = Resources.Load<ProjectConfigRegistry>(RegistryResourcePath);
            uiThemeConfig = registry != null ? registry.UIThemeConfig : null;
        }

        public void ApplySafeArea()
        {
            ResolveThemeConfig();

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (uiThemeConfig != null && !uiThemeConfig.EnforceSafeArea)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                return;
            }

            float safeAreaPadding = uiThemeConfig != null ? uiThemeConfig.SafeAreaPadding : 0f;
            Vector2 minAnchor = safeArea.position;
            Vector2 maxAnchor = safeArea.position + safeArea.size;

            minAnchor.x = (minAnchor.x + safeAreaPadding) / Screen.width;
            minAnchor.y = (minAnchor.y + safeAreaPadding) / Screen.height;
            maxAnchor.x = (maxAnchor.x - safeAreaPadding) / Screen.width;
            maxAnchor.y = (maxAnchor.y - safeAreaPadding) / Screen.height;

            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
