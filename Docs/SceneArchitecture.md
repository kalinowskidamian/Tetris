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

## Gameplay Scene
Purpose:
- separate board/world and UI layers for future gameplay features
- reserve structure for game-feel and feedback systems

Contents:
- `EnvironmentRoot` + camera
- `BoardRoot`
- `GameplayRoot`
  - `Canvas`
  - `SafeAreaRoot`
    - `GameplayLayer`
    - `OverlayLayer`
    - `GameplayScreenRoot`
  - `EventSystem`
  - `ScreenFeedback`
