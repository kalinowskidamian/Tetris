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
        [SerializeField, Range(0f, 0.45f)] private float cellPaddingRatio = 0.08f;
        [SerializeField] private Color iPieceColor = new(0f, 0.95f, 1f, 1f);
        [SerializeField] private Color oPieceColor = new(1f, 0.92f, 0.25f, 1f);
        [SerializeField] private Color tPieceColor = new(0.82f, 0.45f, 1f, 1f);
        [SerializeField] private Color sPieceColor = new(0.32f, 1f, 0.45f, 1f);
        [SerializeField] private Color zPieceColor = new(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private Color jPieceColor = new(0.35f, 0.62f, 1f, 1f);
        [SerializeField] private Color lPieceColor = new(1f, 0.65f, 0.25f, 1f);

        private readonly List<Image> blocks = new();
        private Sprite blockSprite;
        private RectTransform rootRect;

        private void Awake()
        {
            rootRect = (RectTransform)transform;
            blockSprite = CreateBlockSprite();
        }

        public void Render(BoardModel board, ActivePieceState? activePiece)
        {
            if (rootRect == null)
            {
                rootRect = (RectTransform)transform;
            }

            var needed = CountBoardCells(board) + CountActiveCells(activePiece);
            EnsurePool(needed);

            var metrics = BoardMetrics.Create(rootRect.rect.size, board.Width, board.Height, cellPaddingRatio);
            var index = 0;
            index = RenderBoard(board, index, metrics);
            index = RenderActive(activePiece, index, metrics);

            for (var i = index; i < blocks.Count; i++)
            {
                blocks[i].gameObject.SetActive(false);
            }
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
                    SetupBlock(block, x, y, GetPieceColor(pieceId.Value), metrics);
                }
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
                SetupBlock(block, worldCell.X, worldCell.Y, active.Definition.TokenColor, metrics);
            }

            return index;
        }

        private void SetupBlock(Image block, int x, int y, Color color, BoardMetrics metrics)
        {
            block.gameObject.SetActive(true);
            block.color = color;

            var rect = (RectTransform)block.transform;
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

            public BoardMetrics(float cellStep, Vector2 cellSize, float startX, float startY)
            {
                this.cellStep = cellStep;
                CellSize = cellSize;
                this.startX = startX;
                this.startY = startY;
            }

            public Vector2 CellSize { get; }

            public static BoardMetrics Create(Vector2 bounds, int boardWidth, int boardHeight, float paddingRatio)
            {
                var step = Mathf.Min(bounds.x / boardWidth, bounds.y / boardHeight);
                var boardPixelWidth = step * boardWidth;
                var boardPixelHeight = step * boardHeight;
                var offsetX = (bounds.x - boardPixelWidth) * 0.5f;
                var offsetY = (bounds.y - boardPixelHeight) * 0.5f;
                var padded = Mathf.Max(1f, step * (1f - paddingRatio));
                return new BoardMetrics(step, new Vector2(padded, padded), offsetX, offsetY);
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
