# Gameplay Foundation (Pass 1)

## Domain Structure
Gameplay code is split into three layers under `Assets/Scripts/Gameplay`:
- `Domain`: board and piece rules, no Unity scene dependencies.
- `Runtime`: active piece state, spawn/lock flow, and piece generation.
- `Rendering`: Unity-only board visualization from domain state.

## Board Model
`BoardModel` is the gameplay grid source of truth.
- Configurable `width` and `height`.
- Tracks occupancy per cell (`PieceId?`).
- Provides bounds checks and occupancy queries.
- Validates whether a piece can be placed at an origin/rotation.
- Locks settled pieces into board state.
- Clears full lines and returns cleared row indices.

## Piece Definitions
`PieceDefinition` represents tetromino data.
- Piece id (`PieceId`).
- Rotation states as cell coordinate offsets.
- Spawn rotation index.
- Visual token color for future rendering/theme usage.

Classic 7 tetromino definitions are provided by `ClassicTetrominoLibrary`.

## Runtime Flow
`GameplayRuntime` owns match-level gameplay state transitions.
- Starts by queuing `NextPiece` from generator.
- Spawns active piece near top center.
- Supports move/rotate validation through `BoardModel.CanPlace`.
- Supports hard-drop distance calculation and lock.
- Locks active piece when down movement fails.
- Clears lines, then spawns next piece.
- Sets game over if spawn cannot fit.

## Piece Generation
`BagPieceGenerator` implements isolated 7-bag randomization:
- shuffles all piece definitions into a bag
- drains bag one by one
- refills and reshuffles when empty

## Unity Integration
`GameplayRootController` is the runtime scene entry component.
- builds board + runtime on `Awake`
- creates/uses `GameplayBoardRenderer` on `BoardLayoutAnchor`
- applies gravity tick in `Update`
- routes input through `GameplayInputRouter` with mobile touch source
- keeps optional keyboard controls only for editor/development debugging

`GameplayBoardRenderer` draws board + active piece using generated square sprites.

## Implemented vs Not Yet
Implemented now:
- board occupancy and line clear logic
- piece rotation/placement validation
- active piece lifecycle and next piece flow
- hard drop and gravity stepping
- minimal runtime rendering in gameplay scene

Not implemented yet:
- scoring, level progression, hold, ghost piece, finalized preview UI
- wall kick systems (SRS)
- final art, VFX polish, audio, or meta systems

## Mobile Control Mapping (Pass 1)
- Tap left half of screen (outside board): move left.
- Tap right half of screen (outside board): move right.
- Tap board / upper area: rotate clockwise.
- Swipe down: soft drop one row.
- Strong quick downward swipe: hard drop and lock.

## Debug Controls
- Keyboard control path is wired only in `UNITY_EDITOR` or `DEVELOPMENT_BUILD` for inspection.
