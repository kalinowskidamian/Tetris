# Architecture

## Repository-Aligned Folder Structure

```text
Assets/
  Art/
  Audio/
  Editor/
  Materials/
  Prefabs/
  Resources/
    Configs/
  Scenes/
  Scripts/
    Audio/
    Bootstrap/
    Core/
    Data/
    Gameplay/
    Input/
    UI/
    Utils/
    VFX/
  Settings/
  UI/
  VFX/
Packages/
ProjectSettings/
Docs/
```

Principles:
- Keep production content organized by domain under `Assets/` (no `_Project` indirection in this repo).
- Keep startup, configuration, UI, VFX, and gameplay foundations decoupled.
- Prefer ScriptableObject-driven configuration for runtime tuning and mobile iteration.

## Config-Driven Foundation
- `ProjectConfigRegistry` is the central reference for `GameConfig`, `VisualThemeConfig`, `UIThemeConfig`, and `VFXFeedbackConfig`.
- Startup and runtime helpers resolve config from the registry when local references are absent.
- Initial setup tooling owns baseline config asset creation and assignment.

## Scene Architecture Baseline
- `Bootstrap`: entry-only startup scene.
- `MainMenu`: safe-area-aware menu scene.
- `Gameplay`: portrait-first layout scene with dedicated roots for board, HUD, controls, and feedback.

## Mobile Input Foundation (Planned Behavior)
- Input is routed through a dedicated gameplay input service layer (`GameplayInputRouter` + `IGameplayInputSource`).
- Gesture vocabulary is prepared for tap / swipe / hold without binding final gameplay behavior yet.
- Gameplay systems should consume input snapshots, not raw touch APIs directly.

## Gameplay Foundation (Pass 1)
- Domain-first gameplay model under `Assets/Scripts/Gameplay/Domain` (`BoardModel`, `PieceDefinition`, classic tetromino library).
- Runtime orchestration under `Assets/Scripts/Gameplay/Runtime` (`GameplayRuntime`, `BagPieceGenerator`).
- Unity integration under `Assets/Scripts/Gameplay` + `Rendering` (`GameplayRootController`, `GameplayBoardRenderer`).
- Current scope intentionally excludes scoring/progression/meta and final audiovisual polish.
