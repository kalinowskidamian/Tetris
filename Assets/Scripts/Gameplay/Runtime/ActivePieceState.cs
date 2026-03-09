using Tetris.Gameplay.Domain;

namespace Tetris.Gameplay.Runtime
{
    public readonly struct ActivePieceState
    {
        public ActivePieceState(PieceDefinition definition, CellCoord origin, int rotationIndex)
        {
            Definition = definition;
            Origin = origin;
            RotationIndex = rotationIndex;
        }

        public PieceDefinition Definition { get; }
        public CellCoord Origin { get; }
        public int RotationIndex { get; }

        public ActivePieceState With(CellCoord origin, int rotationIndex)
        {
            return new ActivePieceState(Definition, origin, rotationIndex);
        }
    }
}
