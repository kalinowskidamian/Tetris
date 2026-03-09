using UnityEngine;

namespace Tetris.Core
{
    [CreateAssetMenu(fileName = "VisualThemeConfig", menuName = "Tetris/Config/Visual Theme Config")]
    public sealed class VisualThemeConfig : ScriptableObject
    {
        [Header("Color Palette")]
        [SerializeField] private Color backgroundBaseColor = new(0.03f, 0.04f, 0.09f);
        [SerializeField] private Color backgroundAccentColor = new(0.08f, 0.13f, 0.24f);
        [SerializeField] private Color primaryGlowColor = new(0.08f, 0.95f, 1f);
        [SerializeField] private Color secondaryGlowColor = new(1f, 0.15f, 0.9f);

        [Header("Intensity")]
        [SerializeField, Range(0f, 5f)] private float glowIntensity = 1.35f;
        [SerializeField, Range(0f, 3f)] private float backgroundVignetteIntensity = 0.7f;

        public Color BackgroundBaseColor => backgroundBaseColor;
        public Color BackgroundAccentColor => backgroundAccentColor;
        public Color PrimaryGlowColor => primaryGlowColor;
        public Color SecondaryGlowColor => secondaryGlowColor;
        public float GlowIntensity => glowIntensity;
        public float BackgroundVignetteIntensity => backgroundVignetteIntensity;
    }
}
