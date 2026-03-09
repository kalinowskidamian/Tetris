using System.Collections.Generic;

namespace Tetris.Input
{
    public sealed class CompositeGameplayInputSource : IGameplayInputSource
    {
        private readonly IReadOnlyList<IGameplayInputSource> sources;

        public CompositeGameplayInputSource(IReadOnlyList<IGameplayInputSource> sources)
        {
            this.sources = sources;
        }

        public GameplayInputSnapshot ReadSnapshot()
        {
            var merged = default(GameplayInputSnapshot);
            for (var i = 0; i < sources.Count; i++)
            {
                var snapshot = sources[i].ReadSnapshot();
                if (!snapshot.HasAnyAction)
                {
                    continue;
                }

                merged = merged.Merge(snapshot);
            }

            return merged;
        }
    }
}
