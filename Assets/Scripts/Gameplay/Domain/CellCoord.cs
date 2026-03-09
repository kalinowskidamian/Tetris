namespace Tetris.Gameplay.Domain
{
    public readonly struct CellCoord
    {
        public CellCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public static CellCoord operator +(CellCoord a, CellCoord b)
        {
            return new CellCoord(a.X + b.X, a.Y + b.Y);
        }
    }
}
