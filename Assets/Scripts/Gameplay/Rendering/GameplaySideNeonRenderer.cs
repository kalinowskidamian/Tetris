using System.Collections.Generic;
using Tetris.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Tetris.Gameplay.Rendering
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class GameplaySideNeonRenderer : MonoBehaviour
    {
        [SerializeField] private RectTransform layoutContainerRect;
        [SerializeField] private RectTransform boardRect;
        private GameplayBoardRenderer boardRenderer;
        [SerializeField] private RectTransform hudRect;
        [SerializeField] private RectTransform darkBackdropRect;
        [SerializeField] private RectTransform neonWashRect;
        [SerializeField, Range(0f, 24f)] private float zoneInset = 8f;
        [SerializeField, Range(2f, 30f)] private float railWidth = 11f;
        [SerializeField, Range(0.2f, 0.95f)] private float railHeightFactor = 0.9f;
        [SerializeField, Range(0.05f, 0.45f)] private float motionSpanFactor = 0.24f;
        [SerializeField, Range(0.05f, 0.45f)] private float segmentHeightFactor = 0.14f;
        [SerializeField, Range(8f, 40f)] private float minSegmentHeight = 30f;
        [SerializeField, Range(0.1f, 4f)] private float animationSpeed = 1.15f;
        [SerializeField] private Color outerRailCyan = new(0.1f, 0.94f, 1f, 0.35f);
        [SerializeField] private Color outerRailMagenta = new(0.92f, 0.34f, 1f, 0.34f);
        [SerializeField] private Color innerGlowBlue = new(0.34f, 0.78f, 1f, 0.24f);
        [SerializeField] private Color segmentPurple = new(0.62f, 0.47f, 1f, 0.34f);

        private const int SegmentCountPerSide = 5;
        private const float MinRenderableWidth = 3f;
        private const float MinRenderableHeight = 18f;

        private enum ZoneRenderMode
        {
            Minimal,
            Medium,
            Full
        }

        private RectTransform leftZone;
        private RectTransform rightZone;
        private Sprite neonSprite;
        private SideDecor leftDecor;
        private SideDecor rightDecor;

        private sealed class SideDecor
        {
            public Image OuterRail;
            public Image InnerRail;
            public readonly List<Image> Segments = new();
        }

        private void Awake()
        {
            neonSprite = CreateNeonSprite();
            layoutContainerRect ??= transform as RectTransform;
            if (hudRect == null)
            {
                var anchor = FindFirstObjectByType<HUDLayoutAnchor>();
                hudRect = anchor != null ? anchor.GetComponent<RectTransform>() : null;
            }

            EnsureZones();
            EnsureSiblingOrder();
        }

        private void Update()
        {
            if (boardRect == null)
            {
                SetZoneActive(leftZone, false);
                SetZoneActive(rightZone, false);
                return;
            }

            EnsureZones();
            EnsureSiblingOrder();
            RenderSideDecor();
        }

        public void BindBoardRect(RectTransform targetBoardRect, GameplayBoardRenderer renderer = null)
        {
            boardRect = targetBoardRect;
            boardRenderer = renderer;
        }

        public void BindLayout(RectTransform container, RectTransform board, RectTransform hud, RectTransform darkBackdrop, RectTransform neonWash, GameplayBoardRenderer renderer = null)
        {
            layoutContainerRect = container;
            boardRect = board;
            boardRenderer = renderer;
            hudRect = hud;
            darkBackdropRect = darkBackdrop;
            neonWashRect = neonWash;
            EnsureZones();
            EnsureSiblingOrder();
        }

        private void EnsureZones()
        {
            DestroyLegacyDecorRootIfPresent();

            leftZone = EnsureZoneContainer("LeftSideNeonZone", leftZone);
            rightZone = EnsureZoneContainer("RightSideNeonZone", rightZone);
            leftDecor ??= CreateDecor(leftZone, "Left");
            rightDecor ??= CreateDecor(rightZone, "Right");
        }

        private RectTransform EnsureZoneContainer(string name, RectTransform existing)
        {
            var parent = layoutContainerRect != null ? layoutContainerRect : transform as RectTransform;
            if (existing != null)
            {
                if (existing.parent != parent)
                {
                    existing.SetParent(parent, false);
                }

                ConfigureZoneRect(existing);
                return existing;
            }

            var fromHierarchy = parent != null ? FindDirectChildRect(parent, name) : null;
            if (fromHierarchy != null)
            {
                ConfigureZoneRect(fromHierarchy);
                return fromHierarchy;
            }

            var zone = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            zone.SetParent(parent, false);
            ConfigureZoneRect(zone);
            return zone;
        }

        private static void ConfigureZoneRect(RectTransform zone)
        {
            var centeredAnchor = new Vector2(0.5f, 0.5f);
            zone.anchorMin = centeredAnchor;
            zone.anchorMax = centeredAnchor;
            zone.pivot = new Vector2(0.5f, 0.5f);
            zone.anchoredPosition = Vector2.zero;
            zone.sizeDelta = Vector2.zero;
        }

        private SideDecor CreateDecor(RectTransform zone, string prefix)
        {
            var decor = new SideDecor
            {
                OuterRail = EnsureImage(zone, $"{prefix}OuterRail", outerRailCyan),
                InnerRail = EnsureImage(zone, $"{prefix}InnerGlowRail", innerGlowBlue)
            };

            for (var i = 0; i < SegmentCountPerSide; i++)
            {
                decor.Segments.Add(EnsureImage(zone, $"{prefix}Segment_{i}", segmentPurple));
            }

            return decor;
        }

        private Image EnsureImage(RectTransform parent, string name, Color color)
        {
            var child = parent.Find(name) as RectTransform;
            if (child == null)
            {
                child = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                child.SetParent(parent, false);
            }

            var image = child.GetComponent<Image>();
            image.sprite = neonSprite;
            image.type = Image.Type.Sliced;
            image.raycastTarget = false;
            image.color = color;
            return image;
        }

        private void EnsureSiblingOrder()
        {
            var parent = layoutContainerRect != null ? layoutContainerRect : transform as RectTransform;
            if (parent == null || leftZone == null || rightZone == null)
            {
                return;
            }

            if (leftZone.parent != parent)
            {
                leftZone.SetParent(parent, false);
            }

            if (rightZone.parent != parent)
            {
                rightZone.SetParent(parent, false);
            }

            var targetIndex = 0;
            if (darkBackdropRect != null && darkBackdropRect.parent == parent)
            {
                targetIndex = Mathf.Max(targetIndex, darkBackdropRect.GetSiblingIndex() + 1);
            }

            if (neonWashRect != null && neonWashRect.parent == parent)
            {
                targetIndex = Mathf.Max(targetIndex, neonWashRect.GetSiblingIndex() + 1);
            }

            if (boardRect != null && boardRect.parent == parent)
            {
                targetIndex = Mathf.Min(targetIndex, boardRect.GetSiblingIndex());
            }

            if (hudRect != null && hudRect.parent == parent)
            {
                targetIndex = Mathf.Min(targetIndex, hudRect.GetSiblingIndex());
            }

            leftZone.SetSiblingIndex(Mathf.Clamp(targetIndex, 0, parent.childCount - 1));
            rightZone.SetSiblingIndex(Mathf.Clamp(leftZone.GetSiblingIndex() + 1, 0, parent.childCount - 1));
        }

        private void RenderSideDecor()
        {
            if (layoutContainerRect == null || boardRect == null)
            {
                SetZoneActive(leftZone, false);
                SetZoneActive(rightZone, false);
                return;
            }

            var rootBounds = layoutContainerRect.rect;
            if (!TryResolveVisibleBoardBounds(out var boardBounds))
            {
                SetZoneActive(leftZone, false);
                SetZoneActive(rightZone, false);
                return;
            }
            var hudBounds = ResolveHudBounds();
            var verticalInset = ComputeAdaptiveInset(rootBounds.height);
            var topLimit = hudBounds.HasValue ? Mathf.Min(rootBounds.yMax - verticalInset, hudBounds.Value.yMin - verticalInset) : rootBounds.yMax - verticalInset;
            var bottomLimit = Mathf.Max(rootBounds.yMin + verticalInset, boardBounds.yMin + verticalInset);
            var boardTopLimit = boardBounds.yMax - verticalInset;
            var zoneTop = Mathf.Min(topLimit, boardTopLimit);
            var zoneHeight = Mathf.Max(0f, zoneTop - bottomLimit);

            var leftRawWidth = Mathf.Max(0f, boardBounds.xMin - rootBounds.xMin);
            var rightRawWidth = Mathf.Max(0f, rootBounds.xMax - boardBounds.xMax);
            var leftInset = ComputeAdaptiveInset(leftRawWidth);
            var rightInset = ComputeAdaptiveInset(rightRawWidth);

            var leftMin = rootBounds.xMin + leftInset;
            var leftMax = boardBounds.xMin - leftInset;
            var rightMin = boardBounds.xMax + rightInset;
            var rightMax = rootBounds.xMax - rightInset;

            var leftRect = CreateZoneRect(leftMin, leftMax, bottomLimit, zoneHeight);
            var rightRect = CreateZoneRect(rightMin, rightMax, bottomLimit, zoneHeight);

            RenderZone(leftZone, leftDecor, leftRect, false);
            RenderZone(rightZone, rightDecor, rightRect, true);
        }


        private bool TryResolveVisibleBoardBounds(out Rect boardBounds)
        {
            boardBounds = default;
            if (layoutContainerRect == null)
            {
                return false;
            }

            if (boardRenderer != null && boardRenderer.TryGetVisibleBoardBounds(layoutContainerRect, out boardBounds))
            {
                return true;
            }

            if (boardRect == null)
            {
                return false;
            }

            var fallbackBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(layoutContainerRect, boardRect);
            boardBounds = new Rect(fallbackBounds.min.x, fallbackBounds.min.y, fallbackBounds.size.x, fallbackBounds.size.y);
            return true;
        }

        private Rect? ResolveHudBounds()
        {
            if (hudRect == null || layoutContainerRect == null)
            {
                return null;
            }

            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(layoutContainerRect, hudRect);
            return new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
        }

        private static Rect CreateZoneRect(float minX, float maxX, float minY, float height)
        {
            var width = Mathf.Max(0f, maxX - minX);
            if (width <= 1f || height <= 1f)
            {
                return new Rect(0f, 0f, 0f, 0f);
            }

            return new Rect(minX, minY, width, height);
        }

        private void RenderZone(RectTransform zoneRoot, SideDecor decor, Rect zoneRect, bool rightSide)
        {
            var hasSpace = zoneRect.width >= MinRenderableWidth && zoneRect.height >= MinRenderableHeight;
            SetZoneActive(zoneRoot, hasSpace);
            if (!hasSpace)
            {
                return;
            }

            zoneRoot.anchoredPosition = zoneRect.center;
            zoneRoot.sizeDelta = zoneRect.size;

            var t = Time.unscaledTime * animationSpeed;
            var pulse = 0.5f + (0.5f * Mathf.Sin((t * 1.6f) + (rightSide ? 1.15f : 0f)));
            var horizontalSign = rightSide ? -1f : 1f;
            var mode = ResolveRenderMode(zoneRect.width);
            var activeSegments = mode == ZoneRenderMode.Minimal ? 1 : mode == ZoneRenderMode.Medium ? 2 : SegmentCountPerSide;

            var outerRailWidth = ResolveRailWidth(zoneRect.width, mode);
            var railPadding = Mathf.Max(outerRailWidth * 0.6f, zoneRect.width * 0.08f);
            var localRailX = ((zoneRect.width * 0.5f) - railPadding) * horizontalSign;
            var railHeight = zoneRect.height * (mode == ZoneRenderMode.Minimal ? Mathf.Clamp(railHeightFactor - 0.1f, 0.55f, 0.92f) : railHeightFactor);

            var innerRailWidth = Mathf.Max(1.25f, outerRailWidth * (mode == ZoneRenderMode.Full ? 0.62f : 0.5f));
            var innerRailHeight = railHeight * (mode == ZoneRenderMode.Minimal ? 0.72f : 0.84f);
            var innerRailOffset = horizontalSign * (outerRailWidth * 0.72f);

            decor.OuterRail.enabled = true;
            decor.InnerRail.enabled = true;
            Place(decor.OuterRail.rectTransform, new Vector2(localRailX, 0f), new Vector2(outerRailWidth, railHeight));
            Place(decor.InnerRail.rectTransform, new Vector2(localRailX + innerRailOffset, 0f), new Vector2(innerRailWidth, innerRailHeight));

            decor.OuterRail.color = WithAlpha(Color.Lerp(outerRailCyan, outerRailMagenta, pulse), mode == ZoneRenderMode.Minimal ? 0.32f + (0.2f * pulse) : 0.25f + (0.28f * pulse));
            decor.InnerRail.color = WithAlpha(innerGlowBlue, mode == ZoneRenderMode.Minimal ? 0.18f + (0.16f * (1f - pulse)) : 0.16f + (0.22f * (1f - pulse)));

            RenderSegments(decor.Segments, localRailX, zoneRect.size, outerRailWidth, t, rightSide, activeSegments, mode);
        }

        private void RenderSegments(List<Image> segments, float railX, Vector2 zoneSize, float resolvedRailWidth, float timeValue, bool rightSide, int activeCount, ZoneRenderMode mode)
        {
            var visibleCount = Mathf.Clamp(activeCount, 0, segments.Count);
            var segmentHeight = Mathf.Min(zoneSize.y * 0.42f, Mathf.Max(minSegmentHeight * (mode == ZoneRenderMode.Full ? 1f : 0.6f), zoneSize.y * segmentHeightFactor * (mode == ZoneRenderMode.Minimal ? 0.5f : 0.72f)));
            var movementSpan = Mathf.Max(mode == ZoneRenderMode.Minimal ? 4f : 8f, zoneSize.y * motionSpanFactor * (mode == ZoneRenderMode.Full ? 1f : 0.5f));
            var laneHeight = Mathf.Max(1f, zoneSize.y - segmentHeight);
            var laneStart = (-zoneSize.y * 0.5f) + (segmentHeight * 0.5f);
            var spacing = visibleCount > 1 ? laneHeight / (visibleCount - 1) : 0f;

            for (var i = visibleCount; i < segments.Count; i++)
            {
                segments[i].enabled = false;
            }

            for (var i = 0; i < visibleCount; i++)
            {
                segments[i].enabled = true;
                var phase = (timeValue * 1.85f) + (i * 1.15f);
                var travel = Mathf.Sin(phase) * movementSpan;
                if (rightSide)
                {
                    travel *= -1f;
                }

                var y = Mathf.Clamp(laneStart + (spacing * i) + travel, (-zoneSize.y * 0.5f) + (segmentHeight * 0.5f), (zoneSize.y * 0.5f) - (segmentHeight * 0.5f));
                var width = resolvedRailWidth * (mode == ZoneRenderMode.Minimal ? 1.16f : 1.3f + (0.2f * Mathf.Sin((timeValue * 3.15f) + i)));
                Place(segments[i].rectTransform, new Vector2(railX, y), new Vector2(width, segmentHeight));

                var wave = 0.5f + (0.5f * Mathf.Sin((timeValue * 3.4f) + (i * 0.85f)));
                var alpha = mode == ZoneRenderMode.Minimal ? 0.16f + (0.16f * wave) : 0.18f + (0.24f * wave);
                segments[i].color = WithAlpha(segmentPurple, alpha);
            }
        }

        private float ComputeAdaptiveInset(float availableSpan)
        {
            var proportionalInset = availableSpan * 0.14f;
            return Mathf.Clamp(proportionalInset, 1f, zoneInset);
        }

        private ZoneRenderMode ResolveRenderMode(float zoneWidth)
        {
            if (zoneWidth <= railWidth * 1.35f)
            {
                return ZoneRenderMode.Minimal;
            }

            return zoneWidth <= railWidth * 2.15f ? ZoneRenderMode.Medium : ZoneRenderMode.Full;
        }

        private float ResolveRailWidth(float zoneWidth, ZoneRenderMode mode)
        {
            var widthFactor = mode == ZoneRenderMode.Minimal ? 0.36f : mode == ZoneRenderMode.Medium ? 0.3f : 0.26f;
            var maxWidth = mode == ZoneRenderMode.Minimal ? railWidth * 0.78f : mode == ZoneRenderMode.Medium ? railWidth * 0.9f : railWidth;
            return Mathf.Clamp(zoneWidth * widthFactor, 1.5f, maxWidth);
        }

        private static void SetZoneActive(RectTransform zone, bool active)
        {
            if (zone != null && zone.gameObject.activeSelf != active)
            {
                zone.gameObject.SetActive(active);
            }
        }

        private void DestroyLegacyDecorRootIfPresent()
        {
            var parent = layoutContainerRect != null ? layoutContainerRect : transform as RectTransform;
            var legacy = parent != null ? FindDirectChildRect(parent, "SideNeonDecorRoot") : null;
            if (legacy != null)
            {
                Destroy(legacy.gameObject);
            }
        }

        private static RectTransform FindDirectChildRect(RectTransform parent, string name)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).name == name)
                {
                    return parent.GetChild(i) as RectTransform;
                }
            }

            return null;
        }

        private static void Place(RectTransform rect, Vector2 center, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
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
