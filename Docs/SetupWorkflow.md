# Setup Workflow

## One-Time Setup After Pulling
1. Open project with Unity `6000.3.10f1`.
2. Wait for domain reload and package import.
3. Run menu action: `Tools/Tetris/Apply Initial Project Setup`.

## What the Setup Action Does
- Creates missing production folders in `Assets/`
- Creates missing script-domain folders in `Assets/Scripts/`
- Creates/updates foundational config assets in `Assets/Resources/Configs`
- Creates/updates foundational scenes in `Assets/Scenes`

## Idempotency
The setup action is designed to be safe to run repeatedly:
- missing assets/folders are created
- existing assets are reused
- scene files are regenerated to keep baseline structure consistent

## Recommended Next Manual Steps
- Add generated scenes to Build Settings in this order:
  1. `Bootstrap`
  2. `MainMenu`
  3. `Gameplay`
- Assign Android player settings, app icon, and package ID per release plan.
