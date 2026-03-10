using System.Collections.Generic;
using Tetris.Gameplay.Domain;

namespace Tetris.Gameplay.Runtime
{
    public readonly struct LineClearFeedbackSnapshot
    {
        public LineClearFeedbackSnapshot(int row, IReadOnlyList<PieceId> cells)
        {
            Row = row;
            Cells = cells;
        }

        public int Row { get; }
        public IReadOnlyList<PieceId> Cells { get; }
    }
}
