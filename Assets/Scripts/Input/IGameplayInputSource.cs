namespace Tetris.Input
{
    public interface IGameplayInputSource
    {
        GameplayInputSnapshot ReadSnapshot();
    }
}
