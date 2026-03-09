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
- Locks active piece when down movement fails.
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
- creates a lightweight HUD renderer for score/lines/level, next preview, and game-over text
- applies gravity tick in `Update` with level-based speed scaling and a minimum clamp
- routes input through `GameplayInputRouter` with mobile touch source
- allows tap-to-restart after game over
- keeps optional keyboard controls only for editor/development debugging

`GameplayBoardRenderer` and HUD rendering now apply a darker neon/sci-fi presentation using Unity-native `Image`/`Text` elements only (no imported art).

## Setup Workflow Compatibility
No additional generated scene objects are required for this pass. Existing setup-generated anchors are still the integration points, and runtime creates missing visual children idempotently.

## Mobile Control Mapping (Pass 2)
- Tap left half of screen (outside board): move left.
- Tap right half of screen (outside board): move right.
- Tap board / upper area: rotate clockwise.
- Swipe down: soft drop one row.
- Strong quick downward swipe: hard drop and lock.
- When game over is shown, any gameplay action restarts the run.
