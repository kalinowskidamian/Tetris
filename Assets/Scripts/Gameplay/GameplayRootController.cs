using Tetris.Gameplay.Domain;
using Tetris.Gameplay.Rendering;
using Tetris.Gameplay.Runtime;
using UnityInput = UnityEngine.Input;
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

        private GameplayRuntime gameplayRuntime;
        private GameplayBoardRenderer boardRenderer;
        private float gravityTimer;

        private void Awake()
        {
            if (boardAnchor == null)
            {
                boardAnchor = FindFirstObjectByType<BoardLayoutAnchor>();
            }

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

            HandleDebugInput();

            gravityTimer += Time.deltaTime;
            if (gravityTimer >= gravitySeconds)
            {
                gravityTimer = 0f;
                gameplayRuntime.StepDownAndLockIfNeeded();
                Render();
            }
        }

        private void HandleDebugInput()
        {
            var changed = false;

            if (UnityInput.GetKeyDown(KeyCode.LeftArrow))
            {
                changed |= gameplayRuntime.TryMove(-1, 0);
            }

            if (UnityInput.GetKeyDown(KeyCode.RightArrow))
            {
                changed |= gameplayRuntime.TryMove(1, 0);
            }

            if (UnityInput.GetKeyDown(KeyCode.DownArrow))
            {
                gameplayRuntime.StepDownAndLockIfNeeded();
                changed = true;
            }

            if (UnityInput.GetKeyDown(KeyCode.UpArrow) || UnityInput.GetKeyDown(KeyCode.X))
            {
                changed |= gameplayRuntime.TryRotate(1);
            }

            if (UnityInput.GetKeyDown(KeyCode.Z))
            {
                changed |= gameplayRuntime.TryRotate(-1);
            }

            if (UnityInput.GetKeyDown(KeyCode.Space))
            {
                gameplayRuntime.HardDropAndLock();
                changed = true;
            }

            if (changed)
            {
                Render();
            }
        }

        private void Render()
        {
            boardRenderer.Render(gameplayRuntime.Board, gameplayRuntime.ActivePiece);
        }
    }
}
