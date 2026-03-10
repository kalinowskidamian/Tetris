using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tetris.Gameplay.Rendering
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class GameplaySideNeonRenderer : MonoBehaviour
    {
        [SerializeField] private RectTransform boardRect;
        [SerializeField, Range(0f, 20f)] private float zoneInset = 6f;
        [SerializeField, Range(2f, 24f)] private float railWidth = 8f;
        [SerializeField, Range(0.2f, 0.95f)] private float railHeightFactor = 0.88f;
        [SerializeField, Range(0.05f, 0.45f)] private float motionSpanFactor = 0.22f;
        [SerializeField, Range(0.05f, 0.4f)] private float segmentHeightFactor = 0.16f;
        [SerializeField, Range(2f, 18f)] private float minSegmentHeight = 28f;
        [SerializeField, Range(0.1f, 3.5f)] private float animationSpeed = 1.05f;
        [SerializeField] private Color primaryRailColor = new(0.14f, 0.92f, 1f, 0.34f);
        [SerializeField] private Color secondaryRailColor = new(0.86f, 0.36f, 1f, 0.28f);
        [SerializeField] private Color segmentColor = new(0.78f, 0.55f, 1f, 0.3f);
        [SerializeField] private Color innerGlowColor = new(0.36f, 0.94f, 1f, 0.22f);

        private const int SegmentCountPerSide = 4;

        private RectTransform rootRect;
        private Sprite neonSprite;
        private RectTransform decorRoot;
        private Image leftOuterRail;
        private Image rightOuterRail;
        private Image leftInnerRail;
        private Image rightInnerRail;
        private readonly List<Image> leftSegments = new();
        private readonly List<Image> rightSegments = new();

        private void Awake()
        {
            rootRect = (RectTransform)transform;
            neonSprite = CreateNeonSprite();
            EnsureDecor();
        }

        private void Update()
        {
            if (boardRect == null)
            {
                return;
            }

            RenderSideDecor();
        }

        public void BindBoardRect(RectTransform targetBoardRect)
        {
            boardRect = targetBoardRect;
        }

        private void EnsureDecor()
        {
            var existingRoot = transform.Find("SideNeonDecorRoot") as RectTransform;
            if (existingRoot == null)
            {
                existingRoot = new GameObject("SideNeonDecorRoot", typeof(RectTransform)).GetComponent<RectTransform>();
                existingRoot.SetParent(transform, false);
            }

            decorRoot = existingRoot;
            decorRoot.anchorMin = Vector2.zero;
            decorRoot.anchorMax = Vector2.zero;
            decorRoot.pivot = new Vector2(0.5f, 0.5f);
            decorRoot.SetSiblingIndex(0);

            leftOuterRail = EnsureImage("LeftOuterRail", primaryRailColor);
            rightOuterRail = EnsureImage("RightOuterRail", primaryRailColor);
            leftInnerRail = EnsureImage("LeftInnerRail", innerGlowColor);
            rightInnerRail = EnsureImage("RightInnerRail", innerGlowColor);

            EnsureSegmentPool(leftSegments, "LeftSegment");
            EnsureSegmentPool(rightSegments, "RightSegment");
        }

        private Image EnsureImage(string name, Color color)
        {
            var child = decorRoot.Find(name) as RectTransform;
            if (child == null)
            {
                child = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                child.SetParent(decorRoot, false);
            }

            var image = child.GetComponent<Image>();
            image.sprite = neonSprite;
            image.type = Image.Type.Sliced;
            image.raycastTarget = false;
            image.color = color;
            return image;
        }

        private void EnsureSegmentPool(List<Image> pool, string prefix)
        {
            while (pool.Count < SegmentCountPerSide)
            {
                pool.Add(EnsureImage($"{prefix}_{pool.Count}", segmentColor));
            }
        }

        private void RenderSideDecor()
        {
            decorRoot.anchoredPosition = Vector2.zero;
            decorRoot.sizeDelta = rootRect.rect.size;

            var rootBounds = rootRect.rect;
            var boardBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rootRect, boardRect);
            var leftZone = CreateZone(rootBounds.xMin, boardBounds.min.x, rootBounds);
            var rightZone = CreateZone(boardBounds.max.x, rootBounds.xMax, rootBounds);

            RenderZone(leftZone, leftOuterRail, leftInnerRail, leftSegments, false);
            RenderZone(rightZone, rightOuterRail, rightInnerRail, rightSegments, true);
        }

        private Rect CreateZone(float minX, float maxX, Rect fallback)
        {
            var zoneWidth = Mathf.Max(0f, (maxX - minX) - (zoneInset * 2f));
            if (zoneWidth <= 0.01f)
            {
                return new Rect(0f, 0f, 0f, 0f);
            }

            return new Rect(minX + zoneInset, fallback.yMin + zoneInset, zoneWidth, Mathf.Max(0f, fallback.height - (zoneInset * 2f)));
        }

        private void RenderZone(Rect zone, Image outerRail, Image innerRail, List<Image> segments, bool rightSide)
        {
            var hasSpace = zone.width > railWidth + 1f && zone.height > 20f;
            outerRail.gameObject.SetActive(hasSpace);
            innerRail.gameObject.SetActive(hasSpace);
            for (var i = 0; i < segments.Count; i++)
            {
                segments[i].gameObject.SetActive(hasSpace);
            }

            if (!hasSpace)
            {
                return;
            }

            var t = Time.unscaledTime * animationSpeed;
            var pulse = 0.5f + (0.5f * Mathf.Sin(t * 1.55f + (rightSide ? 1.2f : 0f)));
            var railHeight = zone.height * railHeightFactor;
            var railCenter = new Vector2(
                rightSide ? zone.xMax - (zone.width * 0.32f) : zone.xMin + (zone.width * 0.32f),
                zone.center.y);

            Place(outerRail.rectTransform, railCenter, new Vector2(railWidth, railHeight));
            Place(innerRail.rectTransform, new Vector2(railCenter.x + (rightSide ? -railWidth * 1.1f : railWidth * 1.1f), zone.center.y), new Vector2(railWidth * 0.65f, railHeight * 0.82f));
            outerRail.color = WithAlpha(Color.Lerp(primaryRailColor, secondaryRailColor, pulse), 0.24f + (0.24f * pulse));
            innerRail.color = WithAlpha(innerGlowColor, 0.15f + (0.18f * (1f - pulse)));

            RenderSegments(zone, railCenter.x, segments, t, rightSide);
        }

        private void RenderSegments(Rect zone, float railX, List<Image> segments, float t, bool rightSide)
        {
            var segmentHeight = Mathf.Max(minSegmentHeight, zone.height * segmentHeightFactor);
            var movementSpan = Mathf.Max(8f, zone.height * motionSpanFactor);
            var laneHeight = zone.height - segmentHeight;
            var laneStart = zone.yMin + (segmentHeight * 0.5f);
            var spacing = segments.Count > 1 ? laneHeight / (segments.Count - 1) : 0f;

            for (var i = 0; i < segments.Count; i++)
            {
                var phase = (t * 1.8f) + (i * 1.3f);
                var travel = Mathf.Sin(phase) * movementSpan;
                if (rightSide)
                {
                    travel *= -1f;
                }

                var y = Mathf.Clamp(laneStart + (spacing * i) + travel, zone.yMin + (segmentHeight * 0.5f), zone.yMax - (segmentHeight * 0.5f));
                var width = railWidth * (1.65f + (0.22f * Mathf.Sin((t * 3.2f) + i)));
                Place(segments[i].rectTransform, new Vector2(railX, y), new Vector2(width, segmentHeight));

                var wave = 0.5f + (0.5f * Mathf.Sin((t * 3.5f) + (i * 1.05f)));
                segments[i].color = WithAlpha(segmentColor, 0.16f + (0.20f * wave));
            }
        }

        private static void Place(RectTransform rect, Vector2 center, Vector2 size)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = center;
            rect.sizeDelta = size;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        private static Sprite CreateNeonSprite()
        {
            var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width, 0u, SpriteMeshType.FullRect, Vector4.one * 2f);
        }
    }
}
