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
- Runtime service (`GameplayRuntime`) for spawn, move, rotate, step-down lock, hard drop, and next piece flow.
- Isolated 7-bag randomizer (`BagPieceGenerator`).
- Unity gameplay integration via `GameplayRootController` + `GameplayBoardRenderer` attached to `BoardLayoutAnchor`.

Mobile gameplay controls (portrait pass 1):
- tap left / right side for movement
- tap board/upper area to rotate
- swipe down for soft drop
- strong quick downward swipe for hard drop

Editor/debug keyboard controls are available only in `UNITY_EDITOR` or `DEVELOPMENT_BUILD` builds for inspection.

## Initial Setup Tool
Use the menu command below to create/update all foundational assets in an idempotent way:

- **`Tools/Tetris/Apply Initial Project Setup`**

What this menu action ensures:

> If gameplay scene structure or generated anchors change in a PR, rerun this setup action to regenerate `Gameplay` scene content.

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
