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
        private readonly List<Image> cellBackgrounds = new();
        private readonly List<Image> gridLines = new();
        private readonly List<Image> lineClearEffectBlocks = new();
        private readonly List<LineClearFeedbackSnapshot> lineClearRowSnapshots = new();
        private float lineClearRowsPulse;
        private float lineClearEffectTimer;
        private float lineClearEffectDuration = 0.5f;
        [SerializeField, Range(2f, 24f)] private float lineClearBlinkFrequency = 12f;
        [SerializeField, Range(0f, 0.5f)] private float lineClearEndFadeFraction = 0.30f;
        [SerializeField] private Color lineClearEnergizedColor = new(0.88f, 0.97f, 1f, 1f);

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
            boardBackground.color = boardBackgroundColor;
            boardFrame.color = boardFrameColor;
            boardOuterGlow.color = boardOuterGlowColor;
            SetRect(boardBackground.rectTransform, boardRect.center, boardRect.size);
            SetRect(boardFrame.rectTransform, boardRect.center, boardRect.size + framePadding * 2f);
        }


        public void TriggerLineClearRows(IReadOnlyList<LineClearFeedbackSnapshot> rows, float intensity, float durationSeconds = 0.5f)
        {
            lineClearRowSnapshots.Clear();
            if (rows != null)
            {
                for (var i = 0; i < rows.Count; i++)
                {
                    lineClearRowSnapshots.Add(rows[i]);
                }
            }

            if (lineClearRowSnapshots.Count == 0)
            {
                lineClearRowsPulse = 0f;
                lineClearEffectTimer = 0f;
                return;
            }

            lineClearRowsPulse = Mathf.Clamp01(Mathf.Max(lineClearRowsPulse, intensity));
            lineClearEffectDuration = Mathf.Max(0.1f, durationSeconds);
            lineClearEffectTimer = lineClearEffectDuration;
        }

        private void UpdateLineClearPulse()
        {
            if (lineClearEffectTimer > 0f)
            {
                lineClearEffectTimer = Mathf.Max(0f, lineClearEffectTimer - Time.deltaTime);
                return;
            }

            lineClearRowsPulse = 0f;
            if (lineClearRowSnapshots.Count > 0)
            {
                lineClearRowSnapshots.Clear();
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
                var rowHasLineClearEffect = IsRowInLineClearEffectSet(y);
                for (var x = 0; x < board.Width; x++)
                {
                    var pieceId = board.GetCell(new CellCoord(x, y));
                    if (!pieceId.HasValue || rowHasLineClearEffect)
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
            var highlightCellCount = CountLineClearEffectCells();
            EnsureLineClearEffectPool(highlightCellCount);

            if (highlightCellCount == 0)
            {
                DisableUnusedLineClearEffects(0);
                return;
            }

            var elapsed = Mathf.Max(0f, lineClearEffectDuration - lineClearEffectTimer);
            var normalizedTime = lineClearEffectDuration > 0f
                ? Mathf.Clamp01(elapsed / lineClearEffectDuration)
                : 1f;
            var blink = 0.5f + (0.5f * Mathf.Sin(elapsed * lineClearBlinkFrequency * Mathf.PI * 2f));
            var pulse = Mathf.Lerp(0.32f, 1f, blink) * lineClearRowsPulse;
            var fade = CalculateLineClearFade(normalizedTime);

            var index = 0;
            for (var i = 0; i < lineClearRowSnapshots.Count; i++)
            {
                var snapshot = lineClearRowSnapshots[i];
                var cells = snapshot.Cells;
                if (cells == null)
                {
                    continue;
                }

                for (var x = 0; x < cells.Count; x++)
                {
                    var effectBlock = lineClearEffectBlocks[index++];
                    effectBlock.transform.SetAsLastSibling();
                    var baseColor = GetLockedCellColor(GetPieceColor(cells[x]));
                    var flashColor = Color.Lerp(baseColor, lineClearEnergizedColor, pulse);
                    flashColor.a = Mathf.Lerp(baseColor.a, 1f, pulse) * fade;
                    SetupBlock(effectBlock, x, snapshot.Row, flashColor, metrics);
                }
            }

            DisableUnusedLineClearEffects(index);
        }

        private float CalculateLineClearFade(float normalizedTime)
        {
            var fadeFraction = Mathf.Clamp01(lineClearEndFadeFraction);
            if (fadeFraction <= 0f)
            {
                return 1f;
            }

            var fadeStart = 1f - fadeFraction;
            if (normalizedTime <= fadeStart)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - ((normalizedTime - fadeStart) / fadeFraction));
        }

        private int CountLineClearEffectCells()
        {
            var count = 0;
            for (var i = 0; i < lineClearRowSnapshots.Count; i++)
            {
                count += lineClearRowSnapshots[i].Cells?.Count ?? 0;
            }

            return count;
        }

        private void DisableUnusedLineClearEffects(int usedCount)
        {
            for (var i = usedCount; i < lineClearEffectBlocks.Count; i++)
            {
                lineClearEffectBlocks[i].gameObject.SetActive(false);
            }
        }

        private int CountBoardCells(BoardModel board)
        {
            var count = 0;
            for (var y = 0; y < board.Height; y++)
            {
                if (IsRowInLineClearEffectSet(y))
                {
                    continue;
                }

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

        private bool IsRowInLineClearEffectSet(int row)
        {
            if (lineClearEffectTimer <= 0f)
            {
                return false;
            }

            for (var i = 0; i < lineClearRowSnapshots.Count; i++)
            {
                if (lineClearRowSnapshots[i].Row == row)
                {
                    return true;
                }
            }

            return false;
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
