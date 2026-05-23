# BaseProjectPackageInstaller

A small Unity tool that makes installing my [BaseProjectPackages](https://github.com/JonathanAlber/BaseProjectPackages) quick and painless. No need to copy-paste Git URLs one by one, just click the menu item and you're set.

## Installation

1. Open your project in Unity
2. Open the **Package Manager**
3. Click **+** and select **Install package from git URL**
4. Paste:
   ```
   https://github.com/JonathanAlber/BasePackageInstaller.git
   ```
5. Hit **Enter**
6. Enjoy

## What it does

- Adds two **menu items** under `Tools/Base Package Installer`:
   - **Installer**: Adds any of my base packages with one click
   - **Updater**: Re-imports installed packages to pull the latest remote versions
- Lets you tick exactly which packages to install or update
- Auto-generates a preferred `ProjectInputService` and `PlayerInputActions` action asset
- Generates the matching auto-generated C# class so the input system is ready to use

## Installing packages

1. Open `Tools > Base Package Installer > Installer`
2. Tick the packages you want (or use **Select All**)
3. Click **Install Selected**

## Updating packages

1. Open `Tools > Base Package Installer > Updater` (or click **Open Updater** in the installer)
2. Tick the packages you want to refresh
3. Click **Update Selected**

The updater re-resolves each Git package and tells you what actually happened.

## Logging and status

Both windows now report clearly what is going on:

- A live status line shows which package is being processed.
- Each package logs its result to the Console with the resolved name and version, for example:
   - `Installed Tools 1.2.0.`
   - `Updated UI 1.1.0 → 1.2.0.`
   - `Systems already up to date (1.0.4).`
- If a package runs into a problem, the run **no longer stops**. Remaining packages are still processed, the failure is logged as a warning and the final status box shows a short summary like `Done. 5 ok, 1 failed.` followed by a per-package breakdown.

This fixes the old behavior where a single minor issue could silently stall the updater without telling you.

## Included packages

The installer can pull in any of the following:

| Package | Description |
|---|---|
| `Tools` | General-purpose tools |
| `Attributes` | Custom attributes for the inspector and more |
| `Systems` | Core systems (ServiceLocator, EventBus, etc.) |
| `UI` | UI helpers and menu management |
| `Utility` | Common utilities |
| `ScreenShake` | Screen shake effects |
| `Save System` | Saving and loading game data |

All of these live in the [BaseProjectPackages](https://github.com/JonathanAlber/BaseProjectPackages) repo.

## Why?

I built this so I can spin up new Unity projects with my full stack in seconds. Feel free to use it for your own projects too.