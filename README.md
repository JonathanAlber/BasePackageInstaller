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

- Adds a **menu item button** to install any of my base packages with one click
- Auto-generates a preferred `ProjectInputService` and `PlayerInputActions` action asset
- Generates the matching auto-generated C# class so the input system is ready to use

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

All of these live in the [BaseProjectPackages](https://github.com/JonathanAlber/BaseProjectPackages) repo.

## Why?

I built this so I can spin up new Unity projects with my full stack in seconds. Feel free to use it for your own projects too.
