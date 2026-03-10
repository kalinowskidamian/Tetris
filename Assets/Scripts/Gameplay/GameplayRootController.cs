using System.Collections.Generic;
using Tetris.Gameplay.Domain;
using Tetris.Gameplay.Rendering;
using Tetris.Gameplay.Runtime;
using Tetris.Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tetris.Gameplay
{
    public sealed class GameplayRootController : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField, Min(4)] private int boardWidth = 10;
        [SerializeField, Min(8)] private int boardHeight = 20;
        [SerializeField, Min(0.05f)] private float baseGravitySeconds = 0.72f;
        [SerializeField, Range(0.75f, 0.98f)] private float gravityLevelMultiplier = 0.92f;
        [SerializeField, Min(0.05f)] private float minGravitySeconds = 0.09f;
        [SerializeField, Min(0.05f)] private float lockDelaySeconds = 0.45f;

        [Header("Scene References")]
        [SerializeField] private BoardLayoutAnchor boardAnchor;
        [SerializeField] private HUDLayoutAnchor hudAnchor;
        [SerializeField] private NextPiecePreviewAnchor nextPiecePreviewAnchor;
        [SerializeField] private HoldPiecePreviewAnchor holdPiecePreviewAnchor;
        [SerializeField] private ScoreInfoAnchor scoreInfoAnchor;
        [SerializeField] private GameplayInputRouter inputRouter;
        [SerializeField] private MobileTouchGameplayInputSource touchInputSource;

        [Header("Debug")]
        [SerializeField] private bool enableKeyboardDebugInEditor = true;

        private GameplayRuntime gameplayRuntime;
        private GameplayBoardRenderer boardRenderer;
        private GameplayHudRenderer hudRenderer;
        private float gravityTimer;
        private float lockDelayTimer;
        private bool isPaused;
        private bool reducedEffects;
        private float lineClearOverlayTimer;
        private Image lineClearOverlay;

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
            gameplayRuntime.LinesClearedFeedbackRequested += HandleLinesCleared;
            gameplayRuntime.GameOver += HandleGameOver;

            boardRenderer = boardAnchor.gameObject.GetComponent<GameplayBoardRenderer>();
            if (boardRenderer == null)
            {
                boardRenderer = boardAnchor.gameObject.AddComponent<GameplayBoardRenderer>();
            }

            var hudRootRect = hudAnchor != null ? hudAnchor.GetComponent<RectTransform>() : null;
            var scoreRect = scoreInfoAnchor != null ? scoreInfoAnchor.GetComponent<RectTransform>() : null;
            var previewRect = nextPiecePreviewAnchor != null ? nextPiecePreviewAnchor.GetComponent<RectTransform>() : null;
            var holdRect = holdPiecePreviewAnchor != null ? holdPiecePreviewAnchor.GetComponent<RectTransform>() : null;
            hudRenderer = new GameplayHudRenderer(
                hudRootRect,
                scoreRect,
                previewRect,
                holdRect,
                PauseRun,
                ResumeRun,
                RestartRun,
                ToggleEffectsIntensity,
                ReturnToMenuIfAvailable);
            ApplyBackdropChrome();
            ConfigureInputRouting();
        }

        private void Start()
        {
            if (gameplayRuntime == null)
            {
                return;
            }

            StartNewRun();
        }

        private void Update()
        {
            if (gameplayRuntime == null)
            {
                return;
            }

            if (gameplayRuntime.IsGameOver)
            {
                if (inputRouter != null && inputRouter.TryReadSnapshot(out var gameOverSnapshot) && gameOverSnapshot.HasAnyAction)
                {
                    StartNewRun();
                }

                return;
            }

            if (isPaused)
            {
                ProcessPauseInput();
                UpdateLineClearOverlay();
                Render();
                return;
            }

            var changed = ProcessInputStep();
            changed |= ProcessGravityStep();
            UpdateLineClearOverlay();

            if (changed)
            {
                Render();
            }
        }

        private void OnDestroy()
        {
            if (gameplayRuntime == null)
            {
                return;
            }

            gameplayRuntime.LinesClearedFeedbackRequested -= HandleLinesCleared;
            gameplayRuntime.GameOver -= HandleGameOver;
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

            if (holdPiecePreviewAnchor == null)
            {
                holdPiecePreviewAnchor = FindFirstObjectByType<HoldPiecePreviewAnchor>();
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

            if (snapshot.PauseRequested)
            {
                if (isPaused)
                {
                    ResumeRun();
                }
                else
                {
                    PauseRun();
                }

                return true;
            }

            var changed = false;

            if (snapshot.HoldRequested && snapshot.GestureKind == GestureKind.SwipeDown)
            {
                gameplayRuntime.HardDropAndLock();
                gravityTimer = 0f;
                lockDelayTimer = 0f;
                return true;
            }

            if (snapshot.HoldRequested)
            {
                changed |= gameplayRuntime.TryHoldPiece();
                lockDelayTimer = 0f;
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
                changed |= gameplayRuntime.TryStepDown();
                gravityTimer = 0f;
            }

            if (changed)
            {
                RefreshLockDelayAfterAction();
            }

            return changed;
        }

        private bool ProcessGravityStep()
        {
            gravityTimer += Time.deltaTime;
            lockDelayTimer += Time.deltaTime;

            var changed = false;
            if (gravityTimer >= GetGravityIntervalForLevel(gameplayRuntime.Level))
            {
                gravityTimer = 0f;
                changed |= gameplayRuntime.TryStepDown();
            }

            if (gameplayRuntime.IsActivePieceGrounded() && lockDelayTimer >= lockDelaySeconds)
            {
                gameplayRuntime.LockActivePieceNow();
                lockDelayTimer = 0f;
                changed = true;
            }

            if (changed)
            {
                RefreshLockDelayAfterAction();
            }

            return changed;
        }

        private void RefreshLockDelayAfterAction()
        {
            lockDelayTimer = 0f;
        }

        private void StartNewRun()
        {
            gravityTimer = 0f;
            lockDelayTimer = 0f;
            isPaused = false;
            lineClearOverlayTimer = 0f;
            if (lineClearOverlay != null)
            {
                lineClearOverlay.color = new Color(0.3f, 0.92f, 1f, 0f);
            }
            gameplayRuntime.Start();
            Render();
        }

        private float GetGravityIntervalForLevel(int level)
        {
            var scaled = baseGravitySeconds * Mathf.Pow(gravityLevelMultiplier, Mathf.Max(0, level - 1));
            return Mathf.Max(minGravitySeconds, scaled);
        }

        private void HandleLinesCleared(IReadOnlyList<int> rows)
        {
            lineClearOverlayTimer = reducedEffects ? 0.08f : 0.18f;
            if (lineClearOverlay != null)
            {
                lineClearOverlay.color = new Color(0.3f, 0.92f, 1f, reducedEffects ? 0.10f : 0.22f);
            }

            if (boardRenderer != null)
            {
                boardRenderer.TriggerLineClearPulse(reducedEffects ? 0.35f : 0.8f);
            }

            Debug.Log($"Lines cleared: {rows.Count} (total {gameplayRuntime.LinesCleared})");
        }

        private void HandleGameOver()
        {
            Debug.Log("Game over reached. Tap to restart run.");
        }

        private void ApplyBackdropChrome()
        {
            var boardRoot = boardAnchor.GetComponent<RectTransform>().parent as RectTransform;
            if (boardRoot == null)
            {
                return;
            }

            EnsureBackdrop(boardRoot, "GameplayDarkBackdrop", new Color(0.01f, 0.015f, 0.035f, 1f), 0);
            EnsureBackdrop(boardRoot, "GameplayNeonWash", new Color(0.08f, 0.16f, 0.28f, 0.15f), 1);
            lineClearOverlay = EnsureBackdrop(boardRoot, "LineClearOverlay", new Color(0.3f, 0.92f, 1f, 0f), 2);
        }

        private static Image EnsureBackdrop(RectTransform parent, string name, Color color, int siblingIndex)
        {
            var existing = parent.Find(name) as RectTransform;
            if (existing == null)
            {
                existing = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                existing.SetParent(parent, false);
                existing.anchorMin = Vector2.zero;
                existing.anchorMax = Vector2.one;
                existing.offsetMin = Vector2.zero;
                existing.offsetMax = Vector2.zero;
            }

            existing.SetSiblingIndex(siblingIndex);
            var image = existing.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void Render()
        {
            boardRenderer.Render(gameplayRuntime.Board, gameplayRuntime.ActivePiece, gameplayRuntime.GetGhostPiece());
            hudRenderer?.Render(gameplayRuntime, isPaused, reducedEffects);
        }

        private void ProcessPauseInput()
        {
            if (inputRouter == null || !inputRouter.TryReadSnapshot(out var snapshot) || !snapshot.HasAnyAction)
            {
                return;
            }

            if (snapshot.PauseRequested || snapshot.TapRequested)
            {
                ResumeRun();
            }
        }

        private void PauseRun()
        {
            isPaused = true;
        }

        private void ResumeRun()
        {
            isPaused = false;
        }

        private void RestartRun()
        {
            StartNewRun();
        }

        private void ToggleEffectsIntensity()
        {
            reducedEffects = !reducedEffects;
        }

        private void ReturnToMenuIfAvailable()
        {
            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu");
                return;
            }

            Debug.Log("Menu return requested, but MainMenu scene is unavailable. Placeholder hook is active.");
        }

        private void UpdateLineClearOverlay()
        {
            if (lineClearOverlay == null)
            {
                return;
            }

            if (lineClearOverlayTimer <= 0f)
            {
                var c = lineClearOverlay.color;
                c.a = 0f;
                lineClearOverlay.color = c;
                return;
            }

            lineClearOverlayTimer = Mathf.Max(0f, lineClearOverlayTimer - Time.deltaTime);
            var targetAlpha = reducedEffects ? 0.10f : 0.22f;
            var alpha = Mathf.Clamp01(lineClearOverlayTimer / (reducedEffects ? 0.08f : 0.18f)) * targetAlpha;
            var color = lineClearOverlay.color;
            color.a = alpha;
            lineClearOverlay.color = color;
        }
    }
}
