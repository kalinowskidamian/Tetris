# Scene Architecture

## Scene List
1. `Bootstrap`
2. `MainMenu`
3. `Gameplay`

## Bootstrap Scene
Purpose:
- hold only startup responsibilities
- initialize app runtime settings
- load the next scene based on `GameConfig`

Contents:
- `AppBootstrap` GameObject with `AppBootstrap` component

## MainMenu Scene
Purpose:
- host main menu UI hierarchy and safe-area aware root
- provide clean staging area for future polished menu systems

Contents:
- `EnvironmentRoot` + camera
- `MainMenuRoot`
  - `Canvas`
    - `SafeAreaRoot`
      - `MainMenuScreenRoot`
  - `EventSystem`

## Gameplay Scene (Portrait-First)
Purpose:
- separate portrait gameplay zones for board focus, HUD readability, touch controls, and feedback overlays
- provide explicit anchor points for future gameplay and UI systems

Contents:
- `EnvironmentRoot` + camera
- `GameplayRoot`
  - `Canvas`
    - `SafeAreaRoot`
      - `GameplayScreenRoot` (`GameplayLayoutRoot`)
        - `BackgroundRoot`
        - `BoardRoot` (`BoardLayoutAnchor`)
        - `HUDRoot` (`HUDLayoutAnchor`)
        - `ControlsRoot` (`ControlsLayoutAnchor`)
        - `FeedbackRoot` (`FeedbackLayoutAnchor`, `ScreenFeedbackController`)
  - `EventSystem`
  - `GameplayInputRouter`
