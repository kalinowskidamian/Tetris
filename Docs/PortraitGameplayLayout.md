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
