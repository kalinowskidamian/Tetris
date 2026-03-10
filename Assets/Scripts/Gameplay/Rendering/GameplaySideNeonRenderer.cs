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

        public void BindBoardRect(RectTransform targetBoardRect)
        {
            boardRect = targetBoardRect;
        }

        public void BindLayout(RectTransform container, RectTransform board, RectTransform hud, RectTransform darkBackdrop, RectTransform neonWash)
        {
            layoutContainerRect = container;
            boardRect = board;
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
            zone.anchorMin = Vector2.zero;
            zone.anchorMax = Vector2.zero;
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
            var boardBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(layoutContainerRect, boardRect);
            var hudBounds = ResolveHudBounds();
            var topLimit = hudBounds.HasValue ? Mathf.Min(rootBounds.yMax - zoneInset, hudBounds.Value.yMin - zoneInset) : rootBounds.yMax - zoneInset;
            var bottomLimit = Mathf.Max(rootBounds.yMin + zoneInset, boardBounds.min.y + zoneInset);
            var boardTopLimit = boardBounds.max.y - zoneInset;
            var zoneTop = Mathf.Min(topLimit, boardTopLimit);
            var zoneHeight = Mathf.Max(0f, zoneTop - bottomLimit);

            var leftMin = rootBounds.xMin + zoneInset;
            var leftMax = boardBounds.min.x - zoneInset;
            var rightMin = boardBounds.max.x + zoneInset;
            var rightMax = rootBounds.xMax - zoneInset;

            var leftRect = CreateZoneRect(leftMin, leftMax, bottomLimit, zoneHeight);
            var rightRect = CreateZoneRect(rightMin, rightMax, bottomLimit, zoneHeight);

            RenderZone(leftZone, leftDecor, leftRect, false);
            RenderZone(rightZone, rightDecor, rightRect, true);
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
            var hasSpace = zoneRect.width > railWidth * 1.8f && zoneRect.height > minSegmentHeight * 2f;
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

            var localRailX = ((zoneRect.width * 0.5f) - (zoneRect.width * 0.26f)) * horizontalSign;
            var railHeight = zoneRect.height * railHeightFactor;

            Place(decor.OuterRail.rectTransform, new Vector2(localRailX, 0f), new Vector2(railWidth, railHeight));
            Place(decor.InnerRail.rectTransform, new Vector2(localRailX + (horizontalSign * railWidth * 1.08f), 0f), new Vector2(railWidth * 0.6f, railHeight * 0.84f));

            decor.OuterRail.color = WithAlpha(Color.Lerp(outerRailCyan, outerRailMagenta, pulse), 0.25f + (0.28f * pulse));
            decor.InnerRail.color = WithAlpha(innerGlowBlue, 0.16f + (0.22f * (1f - pulse)));

            RenderSegments(decor.Segments, localRailX, zoneRect.size, t, rightSide);
        }

        private void RenderSegments(List<Image> segments, float railX, Vector2 zoneSize, float timeValue, bool rightSide)
        {
            var segmentHeight = Mathf.Max(minSegmentHeight, zoneSize.y * segmentHeightFactor);
            var movementSpan = Mathf.Max(10f, zoneSize.y * motionSpanFactor);
            var laneHeight = Mathf.Max(1f, zoneSize.y - segmentHeight);
            var laneStart = (-zoneSize.y * 0.5f) + (segmentHeight * 0.5f);
            var spacing = segments.Count > 1 ? laneHeight / (segments.Count - 1) : 0f;

            for (var i = 0; i < segments.Count; i++)
            {
                var phase = (timeValue * 1.85f) + (i * 1.15f);
                var travel = Mathf.Sin(phase) * movementSpan;
                if (rightSide)
                {
                    travel *= -1f;
                }

                var y = Mathf.Clamp(laneStart + (spacing * i) + travel, (-zoneSize.y * 0.5f) + (segmentHeight * 0.5f), (zoneSize.y * 0.5f) - (segmentHeight * 0.5f));
                var width = railWidth * (1.5f + (0.26f * Mathf.Sin((timeValue * 3.15f) + i)));
                Place(segments[i].rectTransform, new Vector2(railX, y), new Vector2(width, segmentHeight));

                var wave = 0.5f + (0.5f * Mathf.Sin((timeValue * 3.4f) + (i * 0.85f)));
                segments[i].color = WithAlpha(segmentPurple, 0.18f + (0.24f * wave));
            }
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
