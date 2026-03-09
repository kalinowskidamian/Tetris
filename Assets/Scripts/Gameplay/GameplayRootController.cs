using System.Collections.Generic;
using Tetris.Gameplay.Domain;
using Tetris.Gameplay.Rendering;
using Tetris.Gameplay.Runtime;
using Tetris.Input;
using UnityEngine;

namespace Tetris.Gameplay
{
    public sealed class GameplayRootController : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField, Min(4)] private int boardWidth = 10;
        [SerializeField, Min(8)] private int boardHeight = 20;
        [SerializeField, Min(0.02f)] private float gravitySeconds = 0.7f;

        [Header("Scene References")]
        [SerializeField] private BoardLayoutAnchor boardAnchor;
        [SerializeField] private HUDLayoutAnchor hudAnchor;
        [SerializeField] private NextPiecePreviewAnchor nextPiecePreviewAnchor;
        [SerializeField] private ScoreInfoAnchor scoreInfoAnchor;
        [SerializeField] private GameplayInputRouter inputRouter;
        [SerializeField] private MobileTouchGameplayInputSource touchInputSource;

        [Header("Debug")]
        [SerializeField] private bool enableKeyboardDebugInEditor = true;

        private GameplayRuntime gameplayRuntime;
        private GameplayBoardRenderer boardRenderer;
        private float gravityTimer;

        private void Awake()
        {
            ResolveAnchors();
            if (boardAnchor == null)
            {
                Debug.LogError("GameplayRootController requires a BoardLayoutAnchor in the scene.");
                enabled = false;
                return;
            }

            var board = new BoardModel(boardWidth, boardHeight);
            var pieces = ClassicTetrominoLibrary.CreateDefinitions();
            var generator = new BagPieceGenerator(pieces);
            gameplayRuntime = new GameplayRuntime(board, generator);

            boardRenderer = boardAnchor.gameObject.GetComponent<GameplayBoardRenderer>();
            if (boardRenderer == null)
            {
                boardRenderer = boardAnchor.gameObject.AddComponent<GameplayBoardRenderer>();
            }

            ConfigureInputRouting();
        }

        private void Start()
        {
            if (gameplayRuntime == null)
            {
                return;
            }

            gameplayRuntime.Start();
            Render();
        }

        private void Update()
        {
            if (gameplayRuntime == null || gameplayRuntime.IsGameOver)
            {
                return;
            }

            var changed = ProcessInputStep();
            changed |= ProcessGravityStep();

            if (changed)
            {
                Render();
            }
        }

        private void ResolveAnchors()
        {
            if (boardAnchor == null)
            {
                boardAnchor = FindFirstObjectByType<BoardLayoutAnchor>();
            }

            if (hudAnchor == null)
            {
                hudAnchor = FindFirstObjectByType<HUDLayoutAnchor>();
            }

            if (nextPiecePreviewAnchor == null)
            {
                nextPiecePreviewAnchor = FindFirstObjectByType<NextPiecePreviewAnchor>();
            }

            if (scoreInfoAnchor == null)
            {
                scoreInfoAnchor = FindFirstObjectByType<ScoreInfoAnchor>();
            }

            if (inputRouter == null)
            {
                inputRouter = FindFirstObjectByType<GameplayInputRouter>();
            }

            if (touchInputSource == null && inputRouter != null)
            {
                touchInputSource = inputRouter.GetComponent<MobileTouchGameplayInputSource>();
            }
        }

        private void ConfigureInputRouting()
        {
            if (inputRouter == null)
            {
                return;
            }

            if (touchInputSource == null)
            {
                touchInputSource = inputRouter.gameObject.GetComponent<MobileTouchGameplayInputSource>();
                if (touchInputSource == null)
                {
                    touchInputSource = inputRouter.gameObject.AddComponent<MobileTouchGameplayInputSource>();
                }
            }

            var sources = new List<IGameplayInputSource> { touchInputSource };

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (enableKeyboardDebugInEditor)
            {
                sources.Add(new KeyboardGameplayInputSource());
            }
#endif

            inputRouter.BindSource(new CompositeGameplayInputSource(sources));
        }

        private bool ProcessInputStep()
        {
            if (inputRouter == null || !inputRouter.TryReadSnapshot(out var snapshot) || !snapshot.HasAnyAction)
            {
                return false;
            }

            var changed = false;

            if (snapshot.HoldRequested && snapshot.GestureKind == GestureKind.SwipeDown)
            {
                gameplayRuntime.HardDropAndLock();
                gravityTimer = 0f;
                return true;
            }

            if (snapshot.TapRequested)
            {
                switch (snapshot.GestureKind)
                {
                    case GestureKind.SwipeLeft:
                        changed |= gameplayRuntime.TryMove(-1, 0);
                        break;
                    case GestureKind.SwipeRight:
                        changed |= gameplayRuntime.TryMove(1, 0);
                        break;
                    default:
                        changed |= gameplayRuntime.TryRotate(1);
                        break;
                }
            }

            if (snapshot.GestureKind == GestureKind.SwipeDown)
            {
                gameplayRuntime.StepDownAndLockIfNeeded();
                gravityTimer = 0f;
                changed = true;
            }

            return changed;
        }

        private bool ProcessGravityStep()
        {
            gravityTimer += Time.deltaTime;
            if (gravityTimer < gravitySeconds)
            {
                return false;
            }

            gravityTimer = 0f;
            gameplayRuntime.StepDownAndLockIfNeeded();
            return true;
        }

        private void Render()
        {
            boardRenderer.Render(gameplayRuntime.Board, gameplayRuntime.ActivePiece);
        }
    }
}
