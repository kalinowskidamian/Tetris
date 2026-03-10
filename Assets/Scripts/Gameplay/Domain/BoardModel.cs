using System.Collections.Generic;

namespace Tetris.Gameplay.Domain
{
    public sealed class BoardModel
    {
        private readonly PieceId?[,] cells;

        public BoardModel(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new PieceId?[width, height];
        }

        public int Width { get; }
        public int Height { get; }

        public bool IsInBounds(CellCoord coord)
        {
            return coord.X >= 0 && coord.X < Width && coord.Y >= 0 && coord.Y < Height;
        }

        public bool IsCellOccupied(CellCoord coord)
        {
            return IsInBounds(coord) && cells[coord.X, coord.Y].HasValue;
        }

        public PieceId? GetCell(CellCoord coord)
        {
            return IsInBounds(coord) ? cells[coord.X, coord.Y] : null;
        }

        public bool CanPlace(PieceDefinition piece, CellCoord origin, int rotationIndex)
        {
            var state = piece.GetRotationState(rotationIndex);
            for (var i = 0; i < state.Length; i++)
            {
                var cell = origin + state[i];
                if (!IsInBounds(cell) || IsCellOccupied(cell))
                {
                    return false;
                }
            }

            return true;
        }

        public void Lock(PieceDefinition piece, CellCoord origin, int rotationIndex)
        {
            var state = piece.GetRotationState(rotationIndex);
            for (var i = 0; i < state.Length; i++)
            {
                var cell = origin + state[i];
                if (IsInBounds(cell))
                {
                    cells[cell.X, cell.Y] = piece.Id;
                }
            }
        }

        public IReadOnlyList<int> ClearFullLines()
        {
            var clearedRows = GetFullLines();
            ClearLines(clearedRows);
            return clearedRows;
        }

        public IReadOnlyList<int> GetFullLines()
        {
            var clearedRows = new List<int>();

            for (var y = 0; y < Height; y++)
            {
                if (!IsRowFull(y))
                {
                    continue;
                }

                clearedRows.Add(y);
            }

            return clearedRows;
        }

        public void ClearLines(IReadOnlyList<int> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                return;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                CollapseRowsFrom(rows[i] - i);
            }
        }

        private bool IsRowFull(int y)
        {
            for (var x = 0; x < Width; x++)
            {
                if (!cells[x, y].HasValue)
                {
                    return false;
                }
            }

            return true;
        }

        private void CollapseRowsFrom(int clearedY)
        {
            for (var y = clearedY; y < Height - 1; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    cells[x, y] = cells[x, y + 1];
                }
            }

            for (var x = 0; x < Width; x++)
            {
                cells[x, Height - 1] = null;
            }
        }
    }
}
