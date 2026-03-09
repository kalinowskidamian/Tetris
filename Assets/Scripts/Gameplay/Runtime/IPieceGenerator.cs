using Tetris.Gameplay.Domain;

namespace Tetris.Gameplay.Runtime
{
    public interface IPieceGenerator
    {
        PieceDefinition NextPiece();
    }
}
