using System;
using System.Collections.Generic;
using Tetris.Gameplay.Domain;

namespace Tetris.Gameplay.Runtime
{
    public sealed class GameplayRuntime
    {
        private readonly BoardModel board;
        private readonly IPieceGenerator generator;

        public GameplayRuntime(BoardModel board, IPieceGenerator generator)
        {
            this.board = board;
            this.generator = generator;
        }

        public BoardModel Board => board;
        public ActivePieceState? ActivePiece { get; private set; }
        public PieceDefinition NextPiece { get; private set; }
        public bool IsGameOver { get; private set; }
        public int Score { get; private set; }
        public int LinesCleared { get; private set; }
        public int Level { get; private set; } = 1;
        public PieceDefinition HeldPiece { get; private set; }
        public bool CanHoldThisTurn { get; private set; }

        public event Action<IReadOnlyList<int>> LinesClearedFeedbackRequested;
        public event Action GameOver;

        public void Start()
        {
            IsGameOver = false;
            Score = 0;
            LinesCleared = 0;
            Level = 1;
            HeldPiece = null;
            CanHoldThisTurn = true;
            NextPiece = generator.NextPiece();
            SpawnFromNext();
        }

        public bool TryHoldPiece()
        {
            if (!ActivePiece.HasValue || IsGameOver || !CanHoldThisTurn)
            {
                return false;
            }

            var pieceToHold = ActivePiece.Value.Definition;
            CanHoldThisTurn = false;

            if (HeldPiece == null)
            {
                HeldPiece = pieceToHold;
                SpawnFromNext();
                return true;
            }

            var swapPiece = HeldPiece;
            HeldPiece = pieceToHold;
            SpawnPiece(swapPiece, refreshHoldAllowance: false);
            return true;
        }

        public bool TryMove(int x, int y)
        {
            if (!ActivePiece.HasValue || IsGameOver)
            {
                return false;
            }

            var active = ActivePiece.Value;
            var target = new CellCoord(active.Origin.X + x, active.Origin.Y + y);
            if (!board.CanPlace(active.Definition, target, active.RotationIndex))
            {
                return false;
            }

            ActivePiece = active.With(target, active.RotationIndex);
            return true;
        }

        public bool TryRotate(int direction)
        {
            if (!ActivePiece.HasValue || IsGameOver)
            {
                return false;
            }

            var active = ActivePiece.Value;
            var targetRotation = active.Definition.WrapRotation(active.RotationIndex + direction);
            if (!board.CanPlace(active.Definition, active.Origin, targetRotation))
            {
                return false;
            }

            ActivePiece = active.With(active.Origin, targetRotation);
            return true;
        }

        public int HardDropDistance()
        {
            if (!ActivePiece.HasValue)
            {
                return 0;
            }

            var distance = 0;
            while (CanMoveDown(distance + 1))
            {
                distance++;
            }

            return distance;
        }

        public IReadOnlyList<int> HardDropAndLock()
        {
            if (!ActivePiece.HasValue)
            {
                return Array.Empty<int>();
            }

            var distance = HardDropDistance();
            if (distance > 0)
            {
                TryMove(0, -distance);
            }

            return LockActivePiece();
        }

        public IReadOnlyList<int> StepDownAndLockIfNeeded()
        {
            if (!ActivePiece.HasValue)
            {
                return Array.Empty<int>();
            }

            if (TryMove(0, -1))
            {
                return Array.Empty<int>();
            }

            return LockActivePiece();
        }

        public bool IsActivePieceGrounded()
        {
            if (!ActivePiece.HasValue)
            {
                return false;
            }

            return !CanMoveDown(1);
        }

        public ActivePieceState? GetGhostPiece()
        {
            if (!ActivePiece.HasValue)
            {
                return null;
            }

            var active = ActivePiece.Value;
            var distance = HardDropDistance();
            return active.With(new CellCoord(active.Origin.X, active.Origin.Y - distance), active.RotationIndex);
        }

        public bool TryStepDown()
        {
            return TryMove(0, -1);
        }

        public IReadOnlyList<int> LockActivePieceNow()
        {
            if (!ActivePiece.HasValue)
            {
                return Array.Empty<int>();
            }

            return LockActivePiece();
        }

        private IReadOnlyList<int> LockActivePiece()
        {
            var active = ActivePiece.Value;
            board.Lock(active.Definition, active.Origin, active.RotationIndex);
            var cleared = board.ClearFullLines();
            ApplyProgression(cleared.Count);

            if (cleared.Count > 0)
            {
                LinesClearedFeedbackRequested?.Invoke(cleared);
            }

            SpawnFromNext();
            return cleared;
        }

        private void ApplyProgression(int clearedCount)
        {
            if (clearedCount <= 0)
            {
                return;
            }

            var scoreByLineCount = clearedCount switch
            {
                1 => 100,
                2 => 300,
                3 => 500,
                _ => 800
            };

            Score += scoreByLineCount * Level;
            LinesCleared += clearedCount;
            Level = 1 + (LinesCleared / 10);
        }

        private bool CanMoveDown(int offset)
        {
            var active = ActivePiece.Value;
            var target = new CellCoord(active.Origin.X, active.Origin.Y - offset);
            return board.CanPlace(active.Definition, target, active.RotationIndex);
        }

        private void SpawnFromNext()
        {
            var spawnPiece = NextPiece;
            NextPiece = generator.NextPiece();
            SpawnPiece(spawnPiece, refreshHoldAllowance: true);
        }

        private void SpawnPiece(PieceDefinition spawnPiece, bool refreshHoldAllowance)
        {
            var spawn = new CellCoord(board.Width / 2, board.Height - 2);
            var spawnRotation = spawnPiece.SpawnRotationIndex;

            if (!board.CanPlace(spawnPiece, spawn, spawnRotation))
            {
                ActivePiece = null;
                IsGameOver = true;
                GameOver?.Invoke();
                return;
            }

            ActivePiece = new ActivePieceState(spawnPiece, spawn, spawnRotation);
            if (refreshHoldAllowance)
            {
                CanHoldThisTurn = true;
            }
        }
    }
}
