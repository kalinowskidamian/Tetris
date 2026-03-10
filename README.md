# Neon Tetris Foundation (Unity 6 URP)

## Project Purpose
This repository contains the production foundation for an Android mobile game inspired by Tetris with a neon / sci-fi visual direction. The current implementation includes the first gameplay domain/runtime pass: board occupancy, tetromino definitions, 7-bag piece generation, active piece runtime flow, and a minimal board renderer for in-scene inspection.

## Unity Version
- **Unity Editor:** `6000.3.10f1`
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Target platform:** Android (Google Play release path)

## Current Structure
Top-level production folders under `Assets/`:
- `Art`
- `Audio`
- `Materials`
- `Prefabs`
- `Resources`
- `Scenes`
- `Scripts`
- `Settings`
- `UI`
- `VFX`
- `Editor`

Script domains under `Assets/Scripts/`:
- `Core`
- `Bootstrap`
- `Gameplay`
- `Input`
- `UI`
- `Audio`
- `VFX`
- `Data`
- `Utils`


## Gameplay Foundation (Current)
- Board domain model (`BoardModel`) with bounds checks, occupancy, fit validation, locking, and line clears.
- Data-driven tetromino definitions (`PieceDefinition`) with classic 7-piece library and rotation states.
- Runtime service (`GameplayRuntime`) for spawn, move, rotate, step-down lock, hard drop, next-piece flow, and game-over detection.
- Core progression values now included in runtime: score, cleared lines, level, and line-clear feedback hooks.
- Isolated 7-bag randomizer (`BagPieceGenerator`).
- Unity gameplay integration via `GameplayRootController` + `GameplayBoardRenderer` with HUD/preview rendering on gameplay anchors.

Mobile gameplay controls (portrait pass 1):
- tap left / right side for movement
- tap board/upper area to rotate
- swipe down for soft drop
- strong quick downward swipe for hard drop

Editor/debug keyboard controls are available only in `UNITY_EDITOR` or `DEVELOPMENT_BUILD` builds for inspection.


## Gameplay Progression Rules (Pass 2)
- **Scoring:** line clears use classic readable values (single=100, double=300, triple=500, four=800) multiplied by current level.
- **Lines:** total cleared lines accumulate for the run.
- **Level:** `1 + (totalLines / 10)`.
- **Gravity scaling:** gravity starts from a base interval and scales down each level with a clamp to a minimum interval for readability and fairness on mobile.

## Preview + Run Loop
- **Next piece preview:** HUD now renders a simple code-driven mini-grid preview in portrait layout from `NextPiecePreviewAnchor`.
- **Game over and restart:** if a piece cannot spawn, a game-over message is shown; any new gameplay input restarts a fresh run immediately.
- **Delayed line clear flow:** full rows now enter a short (~0.5s) resolving window where only the soon-to-be-cleared row(s) flash, then collapse/score progression happens after the effect completes.

## Initial Setup Tool
Use the menu command below to create/update all foundational assets in an idempotent way:

- **`Tools/Tetris/Apply Initial Project Setup`**

What this menu action ensures:

> For this PR, rerunning setup is **not required** because new HUD/visual elements are created idempotently at runtime on existing generated anchors.

- required folder structure exists
- config assets exist in `Assets/Resources/Configs`
- foundational scenes are generated/updated:
  - `Assets/Scenes/Bootstrap.unity`
  - `Assets/Scenes/MainMenu.unity`
  - `Assets/Scenes/Gameplay.unity`

## Scene Purpose
- **Bootstrap**: startup entry scene with only essential boot logic (`AppBootstrap`)
- **MainMenu**: menu-oriented scene root with scalable mobile canvas and safe-area structure
- **Gameplay**: portrait-first gameplay scene with explicit roots for background, board, HUD, controls, and feedback

## Foundation Systems Included
- `AppBootstrap` startup flow
- `GameConfig`, `VisualThemeConfig`, `UIThemeConfig`, and registry asset pipeline
- reusable mobile `SafeAreaFitter`
- canvas scaling binder driven by theme config
- minimal VFX feedback architecture (`VFXFeedbackConfig`, `ScreenFeedbackController`)
- gameplay input foundation (`GameplayInputRouter`, `IGameplayInputSource`, gesture snapshot types)

## Documentation
- [Visual Direction](Docs/VisualDirection.md)
- [Scene Architecture](Docs/SceneArchitecture.md)
- [Portrait Gameplay Layout](Docs/PortraitGameplayLayout.md)
- [Setup Workflow](Docs/SetupWorkflow.md)
- [Game Vision](Docs/GameVision.md)
- [Architecture](Docs/Architecture.md)
- [Gameplay Foundation (Pass 1)](Docs/GameplayFoundation.md)
- [Roadmap](Docs/Roadmap.md)
