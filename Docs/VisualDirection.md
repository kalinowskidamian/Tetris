# Visual Direction - Neon / Sci-Fi

## Intent
Build a premium arcade presentation for mobile with a dark futuristic base and bright, high-contrast glow accents.

## Core Pillars
- Dark, layered backgrounds that support readability
- Neon accent palette with controlled saturation
- Strong feedback space for future line-clear flashes and pulses
- Clean UI silhouette and legibility on small Android screens

## Baseline Configuration
Current config assets include fields for:
- background tone controls
- glow intensity controls
- UI animation timing controls
- safe-area and scaling behavior

These settings are centralized so art direction can be tuned without refactoring scene code.

## Line-Clear and Board Neon Notes
- Removed the board-wide `LineClearOverlay` flash so board background luminance remains stable during clears.
- Line-clear feedback is now localized to cleared rows only (no full-board brightening).
- Increased neon surround readability with stronger rail/segment alpha and clearer pulsing + segmented motion.
- Setup tooling changes are **not** required for this visual update; no setup regeneration is needed after pull.
