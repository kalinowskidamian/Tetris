# Neon Tetris Foundation (Unity 6 URP)

## Project Purpose
This repository contains the production foundation for an Android mobile game inspired by Tetris with a neon / sci-fi visual direction. The current implementation focuses on scalable project setup, scene architecture, and configuration systems. No gameplay loop is implemented yet.

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

## Initial Setup Tool
Use the menu command below to create/update all foundational assets in an idempotent way:

- **`Tools/Tetris/Apply Initial Project Setup`**

What this menu action ensures:
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
- [Roadmap](Docs/Roadmap.md)
