# Gameplay Foundation (Pass 2)

## Domain Structure
Gameplay code is split into three layers under `Assets/Scripts/Gameplay`:
- `Domain`: board and piece rules, no Unity scene dependencies.
- `Runtime`: active piece state, spawn/lock flow, progression stats, and piece generation.
- `Rendering`: Unity-only board + HUD visualization from runtime/domain state.

## Board Model
`BoardModel` is the gameplay grid source of truth.
- Configurable `width` and `height`.
- Tracks occupancy per cell (`PieceId?`).
- Provides bounds checks and occupancy queries.
- Validates whether a piece can be placed at an origin/rotation.
- Locks settled pieces into board state.
- Clears full lines and returns cleared row indices.

## Runtime Flow
`GameplayRuntime` owns match-level gameplay state transitions.
- Starts by queuing `NextPiece` from generator.
- Spawns active piece near top center.
- Supports move/rotate validation through `BoardModel.CanPlace`.
- Supports hard-drop distance calculation and lock.
- Exposes a ghost projection that tracks the active piece landing row.
- Supports hold/swap with a one-hold-per-piece-cycle rule.
- Uses a simple lock-delay timer (piece locks shortly after touching stack/floor unless moved/rotated away).
- Clears lines, updates progression values, then spawns next piece.
- Sets game over if spawn cannot fit.

### Progression Values
- **Score:** line clear values are `100/300/500/800` for `1/2/3/4` lines cleared, multiplied by current level.
- **LinesCleared:** total lines cleared for current run.
- **Level:** `1 + (LinesCleared / 10)`.

### Runtime Feedback Hooks
- `LinesClearedFeedbackRequested`: raised when one or more lines are cleared.
- `GameOver`: raised when spawn fails and the run ends.

## Unity Integration
`GameplayRootController` is the runtime scene entry component.
- builds board + runtime on `Awake`
- creates/uses `GameplayBoardRenderer` on `BoardLayoutAnchor`
- creates a lightweight HUD renderer for score/lines/level, next + hold previews, and game-over text
- applies gravity tick in `Update` with level-based speed scaling and a minimum clamp
- routes input through `GameplayInputRouter` with mobile touch source
- allows tap-to-restart after game over
- keeps optional keyboard controls only for editor/development debugging

`GameplayBoardRenderer` and HUD rendering now apply a darker neon/sci-fi presentation using Unity-native `Image`/`Text` elements only (no imported art).

## Gameplay Presentation Pass (Preview/Grid/Pause)
- **Next-piece preview correctness:** preview rendering now uses the same piece rotation state and positive-Y orientation convention as gameplay spawn, so previewed shapes match spawned pieces exactly.
- **Board grid:** board rendering now draws a subtle code-driven cell grid behind active/locked cells, aligned to board metrics and sized from the same cell step used for gameplay blocks.
- **Line-clear feedback:** a lightweight line-clear pulse now triggers a short board flash/pulse plus a brief gameplay-area neon overlay accent.
- **Pause flow:** gameplay now supports a runtime pause state. While paused, gravity/input lock progression stops and a pause/settings overlay is shown.
- **Minimal in-game settings:** pause overlay includes resume, restart run, effects intensity toggle (full/reduced), and a menu placeholder button that only loads `MainMenu` if that scene is available.

## Setup Workflow Compatibility
Setup includes a `HoldPiecePreviewAnchor` and a compact top HUD anchor split (`score`, `hold`, `next`). Runtime-created presentation objects (grid lines, pause/settings overlay, line-clear overlay) are built idempotently from existing anchors.

**Setup regeneration required after this PR?** No. Existing setup-generated scenes remain compatible; re-running setup is optional and safe/idempotent.

## Mobile Control Mapping (Pass 2)
- Tap left half of screen (outside board): move left.
- Tap right half of screen (outside board): move right.
- Tap board / upper area: rotate clockwise.
- Swipe down: soft drop one row.
- Strong quick downward swipe: hard drop and lock.
- Swipe up: hold/swap current piece (once per spawned piece).
- When game over is shown, any gameplay action restarts the run.
