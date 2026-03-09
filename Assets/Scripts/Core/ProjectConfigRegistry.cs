using UnityEngine;
using Tetris.VFX;

namespace Tetris.Core
{
    [CreateAssetMenu(fileName = "ProjectConfigRegistry", menuName = "Tetris/Config/Project Config Registry")]
    public sealed class ProjectConfigRegistry : ScriptableObject
    {
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private VisualThemeConfig visualThemeConfig;
        [SerializeField] private UIThemeConfig uiThemeConfig;
        [SerializeField] private VFXFeedbackConfig vfxFeedbackConfig;

        public GameConfig GameConfig => gameConfig;
        public VisualThemeConfig VisualThemeConfig => visualThemeConfig;
        public UIThemeConfig UIThemeConfig => uiThemeConfig;
        public VFXFeedbackConfig VFXFeedbackConfig => vfxFeedbackConfig;
    }
}
