using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Gameplay.Domain
{
    public static class ClassicTetrominoLibrary
    {
        public static IReadOnlyList<PieceDefinition> CreateDefinitions()
        {
            return new[]
            {
                new PieceDefinition(PieceId.I, BuildI(), 0, new Color(0f, 0.95f, 1f)),
                new PieceDefinition(PieceId.O, BuildO(), 0, new Color(1f, 0.92f, 0.25f)),
                new PieceDefinition(PieceId.T, BuildT(), 0, new Color(0.82f, 0.45f, 1f)),
                new PieceDefinition(PieceId.S, BuildS(), 0, new Color(0.32f, 1f, 0.45f)),
                new PieceDefinition(PieceId.Z, BuildZ(), 0, new Color(1f, 0.35f, 0.35f)),
                new PieceDefinition(PieceId.J, BuildJ(), 0, new Color(0.35f, 0.62f, 1f)),
                new PieceDefinition(PieceId.L, BuildL(), 0, new Color(1f, 0.65f, 0.25f))
            };
        }

        private static CellCoord[][] BuildI() => new[]
        {
            Row((-1, 0), (0, 0), (1, 0), (2, 0)),
            Row((1, -1), (1, 0), (1, 1), (1, 2)),
            Row((-1, 1), (0, 1), (1, 1), (2, 1)),
            Row((0, -1), (0, 0), (0, 1), (0, 2))
        };

        private static CellCoord[][] BuildO() => new[]
        {
            Row((0, 0), (1, 0), (0, 1), (1, 1)),
            Row((0, 0), (1, 0), (0, 1), (1, 1)),
            Row((0, 0), (1, 0), (0, 1), (1, 1)),
            Row((0, 0), (1, 0), (0, 1), (1, 1))
        };

        private static CellCoord[][] BuildT() => new[]
        {
            Row((-1, 0), (0, 0), (1, 0), (0, 1)),
            Row((0, -1), (0, 0), (0, 1), (1, 0)),
            Row((-1, 0), (0, 0), (1, 0), (0, -1)),
            Row((0, -1), (0, 0), (0, 1), (-1, 0))
        };

        private static CellCoord[][] BuildS() => new[]
        {
            Row((-1, 0), (0, 0), (0, 1), (1, 1)),
            Row((0, 1), (0, 0), (1, 0), (1, -1)),
            Row((-1, -1), (0, -1), (0, 0), (1, 0)),
            Row((-1, 1), (-1, 0), (0, 0), (0, -1))
        };

        private static CellCoord[][] BuildZ() => new[]
        {
            Row((-1, 1), (0, 1), (0, 0), (1, 0)),
            Row((1, 1), (1, 0), (0, 0), (0, -1)),
            Row((-1, 0), (0, 0), (0, -1), (1, -1)),
            Row((0, 1), (0, 0), (-1, 0), (-1, -1))
        };

        private static CellCoord[][] BuildJ() => new[]
        {
            Row((-1, 0), (0, 0), (1, 0), (-1, 1)),
            Row((0, -1), (0, 0), (0, 1), (1, 1)),
            Row((-1, 0), (0, 0), (1, 0), (1, -1)),
            Row((0, -1), (0, 0), (0, 1), (-1, -1))
        };

        private static CellCoord[][] BuildL() => new[]
        {
            Row((-1, 0), (0, 0), (1, 0), (1, 1)),
            Row((0, -1), (0, 0), (0, 1), (1, -1)),
            Row((-1, 0), (0, 0), (1, 0), (-1, -1)),
            Row((0, -1), (0, 0), (0, 1), (-1, 1))
        };

        private static CellCoord[] Row(params (int x, int y)[] cells)
        {
            var result = new CellCoord[cells.Length];
            for (var i = 0; i < cells.Length; i++)
            {
                result[i] = new CellCoord(cells[i].x, cells[i].y);
            }

            return result;
        }
    }
}
