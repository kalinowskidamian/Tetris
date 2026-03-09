using System;
using System.Collections.Generic;
using Tetris.Gameplay.Domain;

namespace Tetris.Gameplay.Runtime
{
    public sealed class BagPieceGenerator : IPieceGenerator
    {
        private readonly List<PieceDefinition> source;
        private readonly Queue<PieceDefinition> bag = new();
        private readonly System.Random random;

        public BagPieceGenerator(IReadOnlyList<PieceDefinition> definitions, int? seed = null)
        {
            source = new List<PieceDefinition>(definitions);
            random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

            if (source.Count == 0)
            {
                throw new ArgumentException("At least one piece definition is required.");
            }
        }

        public PieceDefinition NextPiece()
        {
            if (bag.Count == 0)
            {
                RefillBag();
            }

            return bag.Dequeue();
        }

        private void RefillBag()
        {
            var shuffled = new List<PieceDefinition>(source);
            for (var i = shuffled.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            for (var i = 0; i < shuffled.Count; i++)
            {
                bag.Enqueue(shuffled[i]);
            }
        }
    }
}
