# Portrait Gameplay Layout Guide

## Intent
The Gameplay scene is portrait-first and split into explicit layout zones so future systems can attach cleanly without hierarchy rewrites.

## Layout Zones
- `BackgroundRoot`: full-screen presentation layer behind gameplay.
- `BoardRoot`: central gameplay area intended for the falling-block board.
- `HUDRoot`: top-focused information zone for score/level/combo/next/hold.
- `ControlsRoot`: bottom touch zone for buttons and gesture surfaces.
- `FeedbackRoot`: full overlay layer for flashes, combo banners, warning overlays, and screen feedback.

## Anchor Components
Attach systems to marker components instead of searching by name:
- `GameplayLayoutRoot`
- `BoardLayoutAnchor`
- `HUDLayoutAnchor`
- `ControlsLayoutAnchor`
- `FeedbackLayoutAnchor`
- `ScoreInfoAnchor`: lightweight HUD slot for score/lines/level text presenters.
- `NextPiecePreviewAnchor`: lightweight HUD slot for next-piece preview presenter.

This keeps gameplay presenters and UI binders robust if hierarchy names evolve.

## Integration Guidance for Future Systems
- Board rendering/spawn systems should parent under `BoardLayoutAnchor`.
- HUD presenters/widgets should parent under `HUDLayoutAnchor`.
- Touch controls and gesture surfaces should mount under `ControlsLayoutAnchor`.
- VFX/UI feedback overlays should render under `FeedbackLayoutAnchor`.
- Keep all gameplay feature entry points scene-agnostic by resolving these anchors at runtime.

## Current Portrait Proportion Targets
- HUD is intentionally compact so the board remains the dominant visual zone.
- `BoardRoot` and `HUDRoot` anchors are tuned together to keep clear separation between active gameplay and top panels.
- Next-piece preview is rendered inside a dedicated content area within `NextPiecePreviewAnchor` so pieces stay centered and fully contained.
- Layout updates are generated through `Tools/Tetris/Apply Initial Project Setup`; rerun setup after pulling layout changes.


## Gameplay Presentation Updates (This PR)
- HUD band is intentionally reduced to a compact strip to keep score/hold/next readable while giving the board most of the portrait space.
- Board anchor is expanded upward so spawn presentation starts clearly below the HUD visual region.
- Grid rendering is strengthened with higher-opacity neon lines and slightly thicker strokes for better cell readability.
- Line clear feedback now uses a brief, energetic per-cell pulse: the actual cleared-row cells flash/blink with a bright energized tint before disappearing, while non-cleared rows/background remain unchanged.
- Neon side ambience is now owned by a dedicated `GameplaySideNeonRenderer` attached to `GameplayLayoutRoot`, instead of `GameplayBoardRenderer`.
- The side renderer computes left/right decoration zones from the full gameplay rect minus the live board rect, then renders rails and segmented animated accents strictly in those side bars.
- Neon animation runs continuously during gameplay (unscaled time), stays visible-but-subtle, and never draws over board cells, ghost piece, grid, or HUD.
- This is runtime-only wiring through `GameplayRootController`; setup regeneration is not required after pull.
