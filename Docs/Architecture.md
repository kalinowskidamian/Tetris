# Architecture

## Proposed Folder Structure

```text
Assets/
  _Project/
    Art/
    Audio/
    Data/
    Prefabs/
    Scenes/
    Scripts/
      Core/
      Gameplay/
      UI/
      Effects/
      Progression/
      Platform/
      Services/
    UI/
    VFX/
Packages/
ProjectSettings/
Docs/
```

Principles:
- Keep project-owned content under `Assets/_Project`.
- Separate gameplay rules, presentation, and platform/services code.
- Prefer ScriptableObject/data-driven configuration for balance and tuning.

## Gameplay Systems (Planned)
- Board/grid simulation with deterministic rules.
- Piece generation/spawn policy and rotation/lock rules.
- Input abstraction for touch gestures and optional external devices.
- Scoring, combo, speed progression, and fail-state handling.
- Game state orchestration (menu, run, pause, game over).

## UI System
- Layered UI architecture (HUD, overlays, menus, meta screens).
- Event-driven view updates from gameplay/domain state.
- Reusable UI components with centralized style/theme tokens.
- Localization-ready text and formatting paths.

## Effects / Juice System
- Centralized feedback triggers (line clear, hard drop, level-up, streak).
- Coordinated animation, VFX, SFX, camera shake, and haptics.
- Intensity profiles for accessibility/performance tiers.
- Pooling/performance-conscious implementation for mobile hardware.

## Save / Progression Foundation
- Versioned save schema for profile, settings, progression, and stats.
- Explicit separation between run-state and persistent profile data.
- Data integrity checks and safe migration path for updates.
- Analytics-ready event boundaries (without hard vendor lock-in).

## Android / Play Store Readiness
- Target Android performance budgets from day one (CPU/GPU/memory).
- Build configuration separation: development, staging, production.
- Compliance-conscious architecture for privacy, consent, and identifiers.
- Prepare integration seams for Play Services, IAP, ads, and remote config.
- Crash/telemetry hooks and release pipeline readiness.
