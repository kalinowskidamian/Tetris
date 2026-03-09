using System;
using UnityEngine;

namespace Tetris.Gameplay.Domain
{
    public sealed class PieceDefinition
    {
        public PieceDefinition(PieceId id, CellCoord[][] rotationStates, int spawnRotationIndex, Color tokenColor)
        {
            if (rotationStates == null || rotationStates.Length == 0)
            {
                throw new ArgumentException("Piece definition must have at least one rotation state.");
            }

            Id = id;
            RotationStates = rotationStates;
            SpawnRotationIndex = Mathf.Clamp(spawnRotationIndex, 0, rotationStates.Length - 1);
            TokenColor = tokenColor;
        }

        public PieceId Id { get; }
        public CellCoord[][] RotationStates { get; }
        public int SpawnRotationIndex { get; }
        public Color TokenColor { get; }

        public int RotationStateCount => RotationStates.Length;

        public CellCoord[] GetRotationState(int rotationIndex)
        {
            var wrapped = WrapRotation(rotationIndex);
            return RotationStates[wrapped];
        }

        public int WrapRotation(int rotationIndex)
        {
            var count = RotationStateCount;
            var value = rotationIndex % count;
            return value < 0 ? value + count : value;
        }
    }
}
