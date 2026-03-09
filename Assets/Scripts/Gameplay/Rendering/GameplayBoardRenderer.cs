using System.Collections.Generic;
using Tetris.Gameplay.Domain;
using Tetris.Gameplay.Runtime;
using UnityEngine;

namespace Tetris.Gameplay.Rendering
{
    public sealed class GameplayBoardRenderer : MonoBehaviour
    {
        [SerializeField] private float cellSize = 0.9f;
        [SerializeField] private float cellPadding = 0.05f;
        [SerializeField] private Color boardCellColor = new(0.12f, 0.2f, 0.34f, 0.9f);
        [SerializeField] private Color activeCellColor = new(0.95f, 0.95f, 1f, 0.95f);

        private readonly List<SpriteRenderer> blocks = new();
        private Sprite sprite;

        public void Render(BoardModel board, ActivePieceState? activePiece)
        {
            EnsureSprite();
            var needed = CountBoardCells(board) + CountActiveCells(activePiece);
            EnsurePool(needed);

            var index = 0;
            index = RenderBoard(board, index);
            index = RenderActive(activePiece, index);

            for (var i = index; i < blocks.Count; i++)
            {
                blocks[i].gameObject.SetActive(false);
            }
        }

        private int RenderBoard(BoardModel board, int index)
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

                    var renderer = blocks[index++];
                    renderer.gameObject.SetActive(true);
                    renderer.color = boardCellColor;
                    renderer.transform.localPosition = ToWorld(x, y);
                }
            }

            return index;
        }

        private int RenderActive(ActivePieceState? activePiece, int index)
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
                var renderer = blocks[index++];
                renderer.gameObject.SetActive(true);
                renderer.color = activeCellColor;
                renderer.transform.localPosition = ToWorld(worldCell.X, worldCell.Y);
            }

            return index;
        }

        private Vector3 ToWorld(int x, int y)
        {
            var size = cellSize - cellPadding;
            return new Vector3(x * cellSize + size * 0.5f, y * cellSize + size * 0.5f, 0f);
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

        private int CountActiveCells(ActivePieceState? activePiece)
        {
            return activePiece.HasValue
                ? activePiece.Value.Definition.GetRotationState(activePiece.Value.RotationIndex).Length
                : 0;
        }

        private void EnsurePool(int count)
        {
            while (blocks.Count < count)
            {
                var block = new GameObject($"Cell_{blocks.Count}");
                block.transform.SetParent(transform, false);
                var renderer = block.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = 5;
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = new Vector2(cellSize - cellPadding, cellSize - cellPadding);
                blocks.Add(renderer);
            }
        }

        private void EnsureSprite()
        {
            if (sprite != null)
            {
                return;
            }

            var texture = Texture2D.whiteTexture;
            sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
