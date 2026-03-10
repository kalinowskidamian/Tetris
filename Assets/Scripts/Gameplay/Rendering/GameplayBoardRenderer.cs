using System.Collections.Generic;
using Tetris.Gameplay.Domain;
using Tetris.Gameplay.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Tetris.Gameplay.Rendering
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class GameplayBoardRenderer : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.45f)] private float cellPaddingRatio = 0.07f;
        [SerializeField, Range(0f, 3f)] private float visibleTopPaddingRows = 1.9f;
        [SerializeField, Range(0f, 48f)] private float boardFrameThickness = 10f;
        [SerializeField, Range(0f, 28f)] private float boardOuterGlowThickness = 18f;
        [SerializeField] private Color boardBackgroundColor = new(0.03f, 0.04f, 0.09f, 0.96f);
        [SerializeField] private Color boardFrameColor = new(0.15f, 0.85f, 1f, 0.72f);
        [SerializeField] private Color boardOuterGlowColor = new(0.10f, 0.34f, 0.52f, 0.26f);
        [SerializeField, Range(10f, 180f)] private float neonRailOffset = 54f;
        [SerializeField, Range(2f, 20f)] private float neonRailWidth = 8f;
        [SerializeField, Range(18f, 160f)] private float neonSegmentHeight = 52f;
        [SerializeField, Range(16f, 180f)] private float neonSegmentTravel = 70f;
        [SerializeField] private Color neonRailColor = new(0.16f, 0.88f, 1f, 0.18f);
        [SerializeField] private Color neonRailGlowColor = new(0.68f, 0.35f, 1f, 0.11f);
        [SerializeField] private Color neonSegmentColor = new(0.72f, 0.95f, 1f, 0.24f);
        [SerializeField] private Color boardGridColor = new(0.35f, 0.82f, 1f, 0.56f);
        [SerializeField, Range(0.5f, 3.5f)] private float gridLineThickness = 2.65f;
        [SerializeField] private Color boardCellToneA = new(0.05f, 0.10f, 0.17f, 0.58f);
        [SerializeField] private Color boardCellToneB = new(0.10f, 0.17f, 0.26f, 0.72f);
        [SerializeField] private Color boardCellBorderColor = new(0.30f, 0.72f, 0.92f, 0.42f);
        [SerializeField] private Color iPieceColor = new(0f, 0.95f, 1f, 1f);
        [SerializeField] private Color oPieceColor = new(1f, 0.92f, 0.25f, 1f);
        [SerializeField] private Color tPieceColor = new(0.82f, 0.45f, 1f, 1f);
        [SerializeField] private Color sPieceColor = new(0.32f, 1f, 0.45f, 1f);
        [SerializeField] private Color zPieceColor = new(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private Color jPieceColor = new(0.35f, 0.62f, 1f, 1f);
        [SerializeField] private Color lPieceColor = new(1f, 0.65f, 0.25f, 1f);
        [SerializeField, Range(0.05f, 0.8f)] private float ghostAlpha = 0.24f;

        private readonly List<Image> blocks = new();
        private Sprite blockSprite;
        private RectTransform rootRect;
        private Image boardBackground;
        private Image boardFrame;
        private Image boardOuterGlow;
        private RectTransform neonDecorRoot;
        private Image leftRail;
        private Image rightRail;
        private Image leftInnerGlow;
        private Image rightInnerGlow;
        private Image leftSegment;
        private Image rightSegment;
        private readonly List<Image> cellBackgrounds = new();
        private readonly List<Image> gridLines = new();
        private readonly List<Image> lineClearEffectBlocks = new();
        private float lineClearPulse;
        private float lineClearRowsPulse;
        private readonly List<LineClearCellSnapshot> lineClearSnapshots = new();

        private void Awake()
        {
            rootRect = (RectTransform)transform;
            blockSprite = CreateBlockSprite();
            EnsureBoardChrome();
        }

        public void Render(BoardModel board, ActivePieceState? activePiece, ActivePieceState? ghostPiece)
        {
            if (rootRect == null)
            {
                rootRect = (RectTransform)transform;
            }

            var metrics = BoardMetrics.Create(rootRect.rect.size, board.Width, board.Height, cellPaddingRatio, visibleTopPaddingRows);
            UpdateBoardChrome(metrics);
            RenderCellBackgrounds(board, metrics);
            RenderGrid(board, metrics);
            UpdateLineClearPulse();

            var needed = CountBoardCells(board) + CountActiveCells(activePiece) + CountGhostCells(activePiece, ghostPiece);
            EnsurePool(needed);

            var index = 0;
            index = RenderBoard(board, index, metrics);
            index = RenderGhost(activePiece, ghostPiece, index, metrics);
            index = RenderActive(activePiece, index, metrics);
            RenderLineClearEffect(metrics);

            for (var i = index; i < blocks.Count; i++)
            {
                blocks[i].gameObject.SetActive(false);
            }
        }

        private void EnsureBoardChrome()
        {
            boardOuterGlow = EnsureChromeImage("BoardOuterGlow", -3, boardOuterGlowColor);
            boardBackground = EnsureChromeImage("BoardBackground", -2, boardBackgroundColor);
            boardFrame = EnsureChromeImage("BoardFrame", -1, boardFrameColor);
            EnsureNeonDecor();
        }

        private Image EnsureChromeImage(string name, int siblingIndex, Color color)
        {
            var existing = transform.Find(name);
            Image image;
            if (existing == null)
            {
                var child = new GameObject(name, typeof(RectTransform), typeof(Image));
                child.transform.SetParent(transform, false);
                image = child.GetComponent<Image>();
            }
            else
            {
                image = existing.GetComponent<Image>();
            }

            image.raycastTarget = false;
            image.sprite = blockSprite != null ? blockSprite : CreateBlockSprite();
            image.type = Image.Type.Sliced;
            image.color = color;
            image.transform.SetSiblingIndex(Mathf.Max(0, siblingIndex + 2));
            return image;
        }

        private void UpdateBoardChrome(BoardMetrics metrics)
        {
            var boardRect = metrics.BoardRect;
            var framePadding = new Vector2(boardFrameThickness, boardFrameThickness);
            var glowPadding = new Vector2(boardOuterGlowThickness, boardOuterGlowThickness);

            SetRect(boardOuterGlow.rectTransform, boardRect.center, boardRect.size + glowPadding * 2f);
            var pulse = Mathf.Clamp01(lineClearPulse);
            boardBackground.color = Color.Lerp(boardBackgroundColor, new Color(0.35f, 0.8f, 1f, 0.34f), pulse);
            boardFrame.color = Color.Lerp(boardFrameColor, new Color(0.35f, 0.95f, 1f, 0.96f), pulse);
            boardOuterGlow.color = Color.Lerp(boardOuterGlowColor, new Color(0.28f, 0.85f, 1f, 0.42f), pulse);
            SetRect(boardBackground.rectTransform, boardRect.center, boardRect.size);
            SetRect(boardFrame.rectTransform, boardRect.center, boardRect.size + framePadding * 2f);
            UpdateNeonDecor(metrics, pulse);
        }

        private void EnsureNeonDecor()
        {
            var root = transform.Find("BoardNeonDecorRoot") as RectTransform;
            if (root == null)
            {
                root = new GameObject("BoardNeonDecorRoot", typeof(RectTransform)).GetComponent<RectTransform>();
                root.SetParent(transform, false);
            }

            neonDecorRoot = root;
            neonDecorRoot.SetSiblingIndex(0);
            neonDecorRoot.anchorMin = Vector2.zero;
            neonDecorRoot.anchorMax = Vector2.zero;
            neonDecorRoot.pivot = new Vector2(0.5f, 0.5f);

            leftRail = EnsureDecorImage(neonDecorRoot, "LeftRail", neonRailColor);
            rightRail = EnsureDecorImage(neonDecorRoot, "RightRail", neonRailColor);
            leftInnerGlow = EnsureDecorImage(neonDecorRoot, "LeftInnerGlow", neonRailGlowColor);
            rightInnerGlow = EnsureDecorImage(neonDecorRoot, "RightInnerGlow", neonRailGlowColor);
            leftSegment = EnsureDecorImage(neonDecorRoot, "LeftSegment", neonSegmentColor);
            rightSegment = EnsureDecorImage(neonDecorRoot, "RightSegment", neonSegmentColor);
        }

        private Image EnsureDecorImage(RectTransform parent, string name, Color color)
        {
            var child = parent.Find(name) as RectTransform;
            if (child == null)
            {
                child = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                child.SetParent(parent, false);
            }

            var image = child.GetComponent<Image>();
            image.raycastTarget = false;
            image.sprite = blockSprite != null ? blockSprite : CreateBlockSprite();
            image.type = Image.Type.Sliced;
            image.color = color;
            return image;
        }

        private void UpdateNeonDecor(BoardMetrics metrics, float lineClearPulseAmount)
        {
            if (neonDecorRoot == null)
            {
                return;
            }

            var boardRect = metrics.BoardRect;
            neonDecorRoot.anchoredPosition = Vector2.zero;
            neonDecorRoot.sizeDelta = rootRect.rect.size;

            var railHeight = Mathf.Max(24f, boardRect.height * 0.88f);
            var railCenterY = boardRect.center.y;
            var basePulse = 0.5f + (0.5f * Mathf.Sin(Time.unscaledTime * 1.55f));
            var segmentWave = Mathf.Sin(Time.unscaledTime * 1.9f);
            var segmentOffset = segmentWave * neonSegmentTravel;

            PlaceDecorStrip(leftRail.rectTransform, new Vector2(boardRect.xMin - neonRailOffset, railCenterY), new Vector2(neonRailWidth, railHeight));
            PlaceDecorStrip(rightRail.rectTransform, new Vector2(boardRect.xMax + neonRailOffset, railCenterY), new Vector2(neonRailWidth, railHeight));
            PlaceDecorStrip(leftInnerGlow.rectTransform, new Vector2(boardRect.xMin - (neonRailOffset * 0.66f), railCenterY), new Vector2(neonRailWidth * 0.75f, railHeight * 0.78f));
            PlaceDecorStrip(rightInnerGlow.rectTransform, new Vector2(boardRect.xMax + (neonRailOffset * 0.66f), railCenterY), new Vector2(neonRailWidth * 0.75f, railHeight * 0.78f));
            PlaceDecorStrip(leftSegment.rectTransform, new Vector2(boardRect.xMin - neonRailOffset, railCenterY + segmentOffset), new Vector2(neonRailWidth * 1.7f, neonSegmentHeight));
            PlaceDecorStrip(rightSegment.rectTransform, new Vector2(boardRect.xMax + neonRailOffset, railCenterY - segmentOffset), new Vector2(neonRailWidth * 1.7f, neonSegmentHeight));

            var pulseBoost = Mathf.Lerp(0f, 0.14f, lineClearPulseAmount);
            leftRail.color = WithAlpha(neonRailColor, 0.11f + (0.09f * basePulse) + pulseBoost);
            rightRail.color = WithAlpha(neonRailColor, 0.11f + (0.09f * (1f - basePulse)) + pulseBoost);
            leftInnerGlow.color = WithAlpha(neonRailGlowColor, 0.08f + (0.06f * basePulse) + (pulseBoost * 0.8f));
            rightInnerGlow.color = WithAlpha(neonRailGlowColor, 0.08f + (0.06f * (1f - basePulse)) + (pulseBoost * 0.8f));
            leftSegment.color = WithAlpha(neonSegmentColor, 0.12f + (0.14f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.4f))) + pulseBoost);
            rightSegment.color = WithAlpha(neonSegmentColor, 0.12f + (0.14f * (0.5f + 0.5f * Mathf.Sin((Time.unscaledTime * 2.4f) + 1.2f))) + pulseBoost);
        }

        private static void PlaceDecorStrip(RectTransform rect, Vector2 center, Vector2 size)
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

        public void TriggerLineClearPulse(float intensity)
        {
            lineClearPulse = Mathf.Clamp01(Mathf.Max(lineClearPulse, intensity));
        }

        public void TriggerLineClearRows(IReadOnlyList<LineClearFeedbackSnapshot> rows, float intensity)
        {
            lineClearSnapshots.Clear();
            if (rows != null)
            {
                for (var i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    for (var x = 0; x < row.Cells.Count; x++)
                    {
                        var color = GetLockedCellColor(GetPieceColor(row.Cells[x]));
                        lineClearSnapshots.Add(new LineClearCellSnapshot(x, row.Row, color));
                    }
                }
            }

            lineClearRowsPulse = Mathf.Clamp01(Mathf.Max(lineClearRowsPulse, intensity));
        }

        private void UpdateLineClearPulse()
        {
            lineClearPulse = Mathf.Max(0f, lineClearPulse - Time.deltaTime * 4.8f);
            lineClearRowsPulse = Mathf.Max(0f, lineClearRowsPulse - Time.deltaTime * 8.5f);

            if (lineClearRowsPulse <= 0f)
            {
                lineClearSnapshots.Clear();
            }
        }

        private void RenderGrid(BoardModel board, BoardMetrics metrics)
        {
            var lineCount = (board.Width + 1) + (board.Height + 1);
            EnsureGridPool(lineCount);
            var index = 0;

            for (var x = 0; x <= board.Width; x++)
            {
                var line = gridLines[index++];
                line.gameObject.SetActive(true);
                line.color = new Color(boardGridColor.r, boardGridColor.g, boardGridColor.b, boardGridColor.a * 0.95f);
                var rect = line.rectTransform;
                rect.sizeDelta = new Vector2(gridLineThickness, metrics.BoardRect.height);
                rect.anchoredPosition = new Vector2(metrics.BoardRect.xMin + (x * metrics.CellStep), metrics.BoardRect.center.y);
            }

            for (var y = 0; y <= board.Height; y++)
            {
                var line = gridLines[index++];
                line.gameObject.SetActive(true);
                line.color = new Color(boardGridColor.r, boardGridColor.g, boardGridColor.b, boardGridColor.a * 1.08f);
                var rect = line.rectTransform;
                rect.sizeDelta = new Vector2(metrics.BoardRect.width, gridLineThickness);
                rect.anchoredPosition = new Vector2(metrics.BoardRect.center.x, metrics.BoardRect.yMin + (y * metrics.CellStep));
            }

            for (var i = index; i < gridLines.Count; i++)
            {
                gridLines[i].gameObject.SetActive(false);
            }
        }

        private void RenderCellBackgrounds(BoardModel board, BoardMetrics metrics)
        {
            var cellCount = board.Width * board.Height;
            EnsureCellBackgroundPool(cellCount);

            var index = 0;
            for (var y = 0; y < board.Height; y++)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    var cell = cellBackgrounds[index++];
                    cell.gameObject.SetActive(true);
                    cell.color = ((x + y) & 1) == 0 ? boardCellToneA : boardCellToneB;
                    var rect = cell.rectTransform;
                    rect.sizeDelta = metrics.CellSize;
                    rect.anchoredPosition = metrics.GetCellPosition(x, y);
                }
            }
        }

        private void EnsureCellBackgroundPool(int count)
        {
            while (cellBackgrounds.Count < count)
            {
                var go = new GameObject($"BoardCell_{cellBackgrounds.Count}", typeof(RectTransform), typeof(Image), typeof(Outline));
                go.transform.SetParent(transform, false);
                go.transform.SetSiblingIndex(1);

                var image = go.GetComponent<Image>();
                image.raycastTarget = false;
                image.sprite = blockSprite != null ? blockSprite : CreateBlockSprite();
                image.type = Image.Type.Simple;

                var border = go.GetComponent<Outline>();
                border.effectColor = boardCellBorderColor;
                border.effectDistance = new Vector2(1f, -1f);
                border.useGraphicAlpha = true;

                var rect = (RectTransform)go.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);

                cellBackgrounds.Add(image);
            }
        }

        private void EnsureGridPool(int count)
        {
            while (gridLines.Count < count)
            {
                var go = new GameObject($"GridLine_{gridLines.Count}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                go.transform.SetSiblingIndex(3);
                var image = go.GetComponent<Image>();
                image.raycastTarget = false;
                var rect = (RectTransform)go.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
                gridLines.Add(image);
            }
        }

        private static void SetRect(RectTransform rect, Vector2 center, Vector2 size)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = center;
            rect.sizeDelta = size;
        }

        private int RenderBoard(BoardModel board, int index, BoardMetrics metrics)
        {
            for (var y = 0; y < board.Height; y++)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    var pieceId = board.GetCell(new CellCoord(x, y));
                    if (!pieceId.HasValue)
                    {
                        continue;
                    }

                    var block = blocks[index++];
                    SetupBlock(block, x, y, GetLockedCellColor(GetPieceColor(pieceId.Value)), metrics);
                }
            }

            return index;
        }


        private int RenderGhost(ActivePieceState? activePiece, ActivePieceState? ghostPiece, int index, BoardMetrics metrics)
        {
            if (!activePiece.HasValue || !ghostPiece.HasValue)
            {
                return index;
            }

            var active = activePiece.Value;
            var ghost = ghostPiece.Value;
            if (active.Origin.X == ghost.Origin.X && active.Origin.Y == ghost.Origin.Y && active.RotationIndex == ghost.RotationIndex)
            {
                return index;
            }

            var cells = ghost.Definition.GetRotationState(ghost.RotationIndex);
            var ghostColor = GetGhostCellColor(ghost.Definition.TokenColor);

            for (var i = 0; i < cells.Length; i++)
            {
                var worldCell = ghost.Origin + cells[i];
                var block = blocks[index++];
                SetupBlock(block, worldCell.X, worldCell.Y, ghostColor, metrics);
            }

            return index;
        }

        private int RenderActive(ActivePieceState? activePiece, int index, BoardMetrics metrics)
        {
            if (!activePiece.HasValue)
            {
                return index;
            }

            var active = activePiece.Value;
            var cells = active.Definition.GetRotationState(active.RotationIndex);
            for (var i = 0; i < cells.Length; i++)
            {
                var worldCell = active.Origin + cells[i];
                var block = blocks[index++];
                SetupBlock(block, worldCell.X, worldCell.Y, GetActiveCellColor(active.Definition.TokenColor), metrics);
            }

            return index;
        }

        private Color GetLockedCellColor(Color baseColor)
        {
            var shaded = Color.Lerp(baseColor, new Color(0.02f, 0.03f, 0.05f, 1f), 0.17f);
            shaded.a = 0.95f;
            return shaded;
        }

        private static Color GetActiveCellColor(Color baseColor)
        {
            var boosted = Color.Lerp(baseColor, Color.white, 0.20f);
            boosted.a = 1f;
            return boosted;
        }

        private Color GetGhostCellColor(Color baseColor)
        {
            var cooled = Color.Lerp(baseColor, boardGridColor, 0.58f);
            cooled.a = ghostAlpha;
            return cooled;
        }

        private void SetupBlock(Image block, int x, int y, Color color, BoardMetrics metrics)
        {
            block.gameObject.SetActive(true);
            block.color = color;

            var rect = (RectTransform)block.transform;
            rect.anchoredPosition = metrics.GetCellPosition(x, y);
            rect.sizeDelta = metrics.CellSize;
        }

        private void RenderLineClearEffect(BoardMetrics metrics)
        {
            EnsureLineClearEffectPool(lineClearSnapshots.Count);

            var flashPulse = 0.5f + (0.5f * Mathf.Sin(Time.unscaledTime * 88f));
            var flashAmount = Mathf.Clamp01(lineClearRowsPulse * 1.6f) * Mathf.Lerp(0.75f, 1f, flashPulse);
            for (var i = 0; i < lineClearSnapshots.Count; i++)
            {
                var snapshot = lineClearSnapshots[i];
                var effectBlock = lineClearEffectBlocks[i];
                effectBlock.gameObject.SetActive(true);

                var flashColor = Color.Lerp(snapshot.Color, Color.white, flashAmount);
                flashColor.a = Mathf.Lerp(snapshot.Color.a, 1f, flashAmount);
                SetupLineClearEffectBlock(effectBlock, snapshot.X, snapshot.Y, flashColor, metrics);
            }

            for (var i = lineClearSnapshots.Count; i < lineClearEffectBlocks.Count; i++)
            {
                lineClearEffectBlocks[i].gameObject.SetActive(false);
            }
        }

        private void SetupLineClearEffectBlock(Image block, int x, int y, Color color, BoardMetrics metrics)
        {
            block.color = color;
            var rect = block.rectTransform;
            rect.anchoredPosition = metrics.GetCellPosition(x, y);
            rect.sizeDelta = metrics.CellSize;
        }

        private int CountBoardCells(BoardModel board)
        {
            var count = 0;
            for (var y = 0; y < board.Height; y++)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    if (board.GetCell(new CellCoord(x, y)).HasValue)
                    {
                        count++;
                    }
                }
            }

            return count;
        }


        private static int CountGhostCells(ActivePieceState? activePiece, ActivePieceState? ghostPiece)
        {
            if (!activePiece.HasValue || !ghostPiece.HasValue)
            {
                return 0;
            }

            var active = activePiece.Value;
            var ghost = ghostPiece.Value;
            if (active.Origin.X == ghost.Origin.X && active.Origin.Y == ghost.Origin.Y && active.RotationIndex == ghost.RotationIndex)
            {
                return 0;
            }

            return ghost.Definition.GetRotationState(ghost.RotationIndex).Length;
        }

        private static int CountActiveCells(ActivePieceState? activePiece)
        {
            return activePiece.HasValue
                ? activePiece.Value.Definition.GetRotationState(activePiece.Value.RotationIndex).Length
                : 0;
        }

        private void EnsurePool(int count)
        {
            while (blocks.Count < count)
            {
                var block = new GameObject($"Cell_{blocks.Count}", typeof(RectTransform), typeof(Image));
                block.transform.SetParent(transform, false);

                var rect = (RectTransform)block.transform;
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);

                var image = block.GetComponent<Image>();
                image.sprite = blockSprite != null ? blockSprite : CreateBlockSprite();
                image.type = Image.Type.Simple;
                image.raycastTarget = false;
                blocks.Add(image);
            }
        }

        private void EnsureLineClearEffectPool(int count)
        {
            while (lineClearEffectBlocks.Count < count)
            {
                var block = new GameObject($"LineClearEffect_{lineClearEffectBlocks.Count}", typeof(RectTransform), typeof(Image));
                block.transform.SetParent(transform, false);
                block.transform.SetAsLastSibling();

                var rect = (RectTransform)block.transform;
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);

                var image = block.GetComponent<Image>();
                image.sprite = blockSprite != null ? blockSprite : CreateBlockSprite();
                image.type = Image.Type.Simple;
                image.raycastTarget = false;
                lineClearEffectBlocks.Add(image);
            }
        }

        private readonly struct LineClearCellSnapshot
        {
            public LineClearCellSnapshot(int x, int y, Color color)
            {
                X = x;
                Y = y;
                Color = color;
            }

            public int X { get; }
            public int Y { get; }
            public Color Color { get; }
        }

        private static Sprite CreateBlockSprite()
        {
            var texture = Texture2D.whiteTexture;
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }

        private Color GetPieceColor(PieceId pieceId)
        {
            return pieceId switch
            {
                PieceId.I => iPieceColor,
                PieceId.O => oPieceColor,
                PieceId.T => tPieceColor,
                PieceId.S => sPieceColor,
                PieceId.Z => zPieceColor,
                PieceId.J => jPieceColor,
                PieceId.L => lPieceColor,
                _ => Color.white
            };
        }

        private readonly struct BoardMetrics
        {
            private readonly float cellStep;
            private readonly float startX;
            private readonly float startY;

            public BoardMetrics(float cellStep, Vector2 cellSize, float startX, float startY, Rect boardRect)
            {
                this.cellStep = cellStep;
                CellSize = cellSize;
                this.startX = startX;
                this.startY = startY;
                BoardRect = boardRect;
            }

            public Vector2 CellSize { get; }
            public Rect BoardRect { get; }
            public float CellStep => cellStep;

            public static BoardMetrics Create(Vector2 bounds, int boardWidth, int boardHeight, float paddingRatio, float topPaddingRows)
            {
                var paddedBoardHeight = boardHeight + Mathf.Max(0f, topPaddingRows);
                var step = Mathf.Min(bounds.x / boardWidth, bounds.y / paddedBoardHeight);
                var boardPixelWidth = step * boardWidth;
                var boardPixelHeight = step * boardHeight;
                var offsetX = (bounds.x - boardPixelWidth) * 0.5f;
                var extraVertical = Mathf.Max(0f, bounds.y - boardPixelHeight);
                var requestedTopPadding = step * Mathf.Max(0f, topPaddingRows);
                var appliedTopPadding = Mathf.Min(extraVertical, requestedTopPadding);
                var offsetY = (extraVertical - appliedTopPadding) * 0.5f;
                var padded = Mathf.Max(1f, step * (1f - paddingRatio));
                var boardRect = new Rect(offsetX, offsetY, boardPixelWidth, boardPixelHeight);
                return new BoardMetrics(step, new Vector2(padded, padded), offsetX, offsetY, boardRect);
            }

            public Vector2 GetCellPosition(int x, int y)
            {
                return new Vector2(
                    startX + (x + 0.5f) * cellStep,
                    startY + (y + 0.5f) * cellStep);
            }
        }
    }
}
