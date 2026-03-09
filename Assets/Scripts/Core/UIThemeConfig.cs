using UnityEngine;

namespace Tetris.Core
{
    [CreateAssetMenu(fileName = "UIThemeConfig", menuName = "Tetris/Config/UI Theme Config")]
    public sealed class UIThemeConfig : ScriptableObject
    {
        [Header("Typography")]
        [SerializeField] private int headingFontSize = 56;
        [SerializeField] private int bodyFontSize = 28;

        [Header("Animation Timing")]
        [SerializeField, Min(0f)] private float panelFadeDuration = 0.22f;
        [SerializeField, Min(0f)] private float buttonPulseDuration = 0.28f;

        [Header("Mobile Safe Area")]
        [SerializeField] private bool enforceSafeArea = true;
        [SerializeField, Range(0f, 64f)] private float safeAreaPadding = 12f;

        [Header("Scaling")]
        [SerializeField] private Vector2 referenceResolution = new(1080f, 1920f);
        [SerializeField, Range(0f, 1f)] private float matchWidthOrHeight = 1f;

        public int HeadingFontSize => headingFontSize;
        public int BodyFontSize => bodyFontSize;
        public float PanelFadeDuration => panelFadeDuration;
        public float ButtonPulseDuration => buttonPulseDuration;
        public bool EnforceSafeArea => enforceSafeArea;
        public float SafeAreaPadding => safeAreaPadding;
        public Vector2 ReferenceResolution => referenceResolution;
        public float MatchWidthOrHeight => matchWidthOrHeight;
    }
}
