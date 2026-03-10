using UnityInput = UnityEngine.Input;
using UnityEngine;

namespace Tetris.Input
{
    public sealed class KeyboardGameplayInputSource : IGameplayInputSource
    {
        public GameplayInputSnapshot ReadSnapshot()
        {
            var tap = false;
            var hold = false;
            var pause = false;
            var gesture = GestureKind.None;

            if (UnityInput.GetKeyDown(KeyCode.LeftArrow))
            {
                tap = true;
                gesture = GestureKind.SwipeLeft;
            }
            else if (UnityInput.GetKeyDown(KeyCode.RightArrow))
            {
                tap = true;
                gesture = GestureKind.SwipeRight;
            }
            else if (UnityInput.GetKeyDown(KeyCode.DownArrow))
            {
                gesture = GestureKind.SwipeDown;
            }
            else if (UnityInput.GetKeyDown(KeyCode.UpArrow) || UnityInput.GetKeyDown(KeyCode.X) || UnityInput.GetKeyDown(KeyCode.Z))
            {
                tap = true;
            }
            else if (UnityInput.GetKeyDown(KeyCode.Space))
            {
                hold = true;
                gesture = GestureKind.SwipeDown;
            }
            else if (UnityInput.GetKeyDown(KeyCode.C) || UnityInput.GetKeyDown(KeyCode.LeftShift))
            {
                hold = true;
                gesture = GestureKind.SwipeUp;
            }
            else if (UnityInput.GetKeyDown(KeyCode.Escape) || UnityInput.GetKeyDown(KeyCode.P))
            {
                pause = true;
            }

            return new GameplayInputSnapshot(tap, hold, pause, gesture);
        }
    }
}
