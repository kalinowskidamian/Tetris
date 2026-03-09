using UnityEngine;
using UnityEngine.EventSystems;
using UnityInput = UnityEngine.Input;

namespace Tetris.Input
{
    public sealed class MobileTouchGameplayInputSource : MonoBehaviour, IGameplayInputSource
    {
        [SerializeField] private GameplayInputRouter inputRouter;
        [SerializeField] private RectTransform boardArea;
        [SerializeField, Range(0f, 1f)] private float rotateTapMinY = 0.58f;
        [SerializeField, Min(32f)] private float hardDropMinSwipePixels = 220f;
        [SerializeField, Min(0.1f)] private float hardDropMaxDuration = 0.4f;
        [SerializeField, Min(48f)] private float holdMinSwipePixels = 120f;

        private bool hasActiveTouch;
        private int activeFingerId;
        private Vector2 touchStartPosition;
        private float touchStartTime;

        public GameplayInputSnapshot ReadSnapshot()
        {
            if (UnityInput.touchCount <= 0)
            {
                return default;
            }

            for (var i = 0; i < UnityInput.touchCount; i++)
            {
                var touch = UnityInput.GetTouch(i);

                if (!hasActiveTouch && touch.phase == TouchPhase.Began)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        continue;
                    }

                    BeginTouch(touch);
                    continue;
                }

                if (!hasActiveTouch || touch.fingerId != activeFingerId)
                {
                    continue;
                }

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    return CompleteTouch(touch);
                }
            }

            return default;
        }

        private void BeginTouch(Touch touch)
        {
            hasActiveTouch = true;
            activeFingerId = touch.fingerId;
            touchStartPosition = touch.position;
            touchStartTime = Time.unscaledTime;
        }

        private GameplayInputSnapshot CompleteTouch(Touch touch)
        {
            var duration = Time.unscaledTime - touchStartTime;
            var delta = touch.position - touchStartPosition;
            var absX = Mathf.Abs(delta.x);
            var absY = Mathf.Abs(delta.y);

            hasActiveTouch = false;

            var swipeThreshold = inputRouter != null ? inputRouter.SwipeMinDistancePixels : 64f;

            if (absY >= swipeThreshold && absY > absX && delta.y < 0f)
            {
                var hardDrop = absY >= hardDropMinSwipePixels && duration <= hardDropMaxDuration;
                return new GameplayInputSnapshot(false, hardDrop, GestureKind.SwipeDown);
            }

            if (absY >= holdMinSwipePixels && absY > absX && delta.y > 0f)
            {
                return new GameplayInputSnapshot(false, true, GestureKind.SwipeUp);
            }

            var tapMaxDuration = inputRouter != null ? inputRouter.TapMaxDuration : 0.2f;
            if (duration <= tapMaxDuration && absX <= swipeThreshold && absY <= swipeThreshold)
            {
                var isRotateTap = IsRotateTapArea(touchStartPosition);
                var gesture = GestureKind.None;

                if (!isRotateTap)
                {
                    gesture = touchStartPosition.x < Screen.width * 0.5f ? GestureKind.SwipeLeft : GestureKind.SwipeRight;
                }

                return new GameplayInputSnapshot(true, false, gesture);
            }

            return default;
        }

        private bool IsRotateTapArea(Vector2 screenPosition)
        {
            if (boardArea != null)
            {
                return RectTransformUtility.RectangleContainsScreenPoint(boardArea, screenPosition, null);
            }

            return screenPosition.y >= Screen.height * rotateTapMinY;
        }

        private void OnValidate()
        {
            if (inputRouter == null)
            {
                inputRouter = GetComponent<GameplayInputRouter>();
            }
        }
    }
}
