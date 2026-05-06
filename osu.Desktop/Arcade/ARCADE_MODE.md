# Arcade Mode – Technical Reference

> **Audience:** Cursor agents and developers working on the arcade feature.
> For the operator setup guide (collection setup, config file, launch flags) see `SETUP.md`.

---

## What it is

Arcade mode is a kiosk / cabinet variant of the standard osu! desktop game launched via
`--arcade`.  It shares all desktop infrastructure (Realm database, IPC, updater, audio)
but replaces the UI shell with a stripped-down flow that exposes only song select and
gameplay, with no way for a player to leave the application.

---

## Entry point

`osu.Desktop/Program.cs` parses `--arcade` (and the optional `--arcade-config=<path>`
flag) and calls:

```csharp
host.Run(new OsuGameArcade(arcadeConfig, args));
```

This is exactly parallel to the `--tournament` → `TournamentGame` pattern.

---

## File map

All arcade-specific code lives in **`osu.Desktop/Arcade/`**.

| File | Role |
|---|---|
| `ArcadeConfiguration.cs` | JSON config POCO + `LoadFromFile` / `DefaultConfigPath` helpers |
| `OsuGameArcade.cs` | Root game class; wires up the custom loader, blocks exit, forces fullscreen, suppresses first-run |
| `ArcadeLoader.cs` | Thin `Loader` subclass – overrides `CreateMainMenu()` to return `ArcadeMainMenu` |
| `ArcadeMainMenu.cs` | `MainMenu` subclass – uses `ArcadeButtonSystem`, locks overlays, blocks exit/back |
| `ArcadeButtonSystem.cs` | `ButtonSystem` subclass – only adds Play→Solo buttons |
| `ArcadeSongSelect.cs` | `SoloSongSelect` subclass – applies collection filter + optional ruleset lock, strips context menu |
| `arcade.json.example` | Sample config for operators to copy |
| `SETUP.md` | Operator workflow guide (collection creation, config fields, updating maps) |
| `ARCADE_MODE.md` | This file |

---

## Class hierarchy and call chain

```
Program.cs
  └─ OsuGameArcade (: OsuGameDesktop : OsuGame : OsuGameBase)
       └─ CreateLoader() → ArcadeLoader (: Loader)
            └─ CreateMainMenu() → ArcadeMainMenu (: MainMenu)
                 ├─ CreateButtonSystem() → ArcadeButtonSystem (: ButtonSystem)
                 └─ Buttons.OnSolo → ArcadeSongSelect (: SoloSongSelect : SongSelect)
```

At runtime the screen stack is:

```
ArcadeLoader
  → (intro sequence)
    → ArcadeMainMenu          ← permanent root; OnExiting returns true (blocks pop)
        → ArcadeSongSelect
            → PlayerLoader
                → Player
                    → ResultsScreen
                        (back-pops return to ArcadeSongSelect)
```

---

## Overlay / toolbar lockdown mechanism

The key lever is **`OverlayActivation.Disabled`**, an enum value that the arcade screens
set via `InitialOverlayActivationMode`.  Here is how each lock works:

| Mechanism | Where set | Effect |
|---|---|---|
| `InitialOverlayActivationMode => Disabled` | `ArcadeMainMenu`, `ArcadeSongSelect` | `OsuGame.ScreenChanged` binds `OverlayActivationMode` to this; fires `CloseAllOverlays()` |
| `HideOverlaysOnEnter => true` | `ArcadeMainMenu`, `ArcadeSongSelect` | `OsuGame.ScreenChanged` calls `CloseAllOverlays()` including `Toolbar.Hide()` |
| `Toolbar.PropagateNonPositionalInputSubTree` | `Toolbar.cs` (existing code) | Returns `false` when activation is `Disabled`; all toolbar hotkeys (Ctrl+O, Ctrl+T, etc.) are silently swallowed |
| `OsuFocusedOverlayContainer.UpdateState` | Framework overlay base (existing code) | Any overlay that tries to `Show()` while activation is `Disabled` immediately self-hides |
| `OsuGameArcade.ScreenChanged` | `OsuGameArcade.cs` | Belt-and-suspenders `Toolbar?.Hide()` on every screen transition |

The result: settings, chat, notifications, login, beatmap listing, skin editor, and every
other `OsuFocusedOverlayContainer` are silently blocked — no new code required in those
classes.

---

## Exit lockdown mechanism

| Layer | Code | What it prevents |
|---|---|---|
| `OsuGameArcade.AttemptExit()` | Overridden to no-op | OS close button / Alt+F4 calling `AttemptExit` |
| `ArcadeMainMenu.OnExiting(e)` | Returns `true` | Screen-stack pop of the main menu (which would trigger game exit) |
| `ArcadeMainMenu.OnPressed(GlobalAction.Back)` | Returns `false` | Back key trying to `SuspendToBackground` |
| `HoldToExitGameOverlay` removed in `LoadComplete` | `RemoveInternal` | Visual hold-to-exit affordance is removed entirely |
| `ArcadeButtonSystem` omits Exit button | `PopulateButtons` | Exit button never rendered |

The application can still be killed at the OS level (Task Manager, SIGKILL, power off).

---

## Beatmap restriction

`ArcadeSongSelect` restricts the carousel to a single collection:

1. On `[BackgroundDependencyLoader]`, queries `RealmAccess` for a `BeatmapCollection`
   whose `Name` matches `ArcadeConfiguration.CollectionName`.
2. Materialises `BeatmapMD5Hashes` into an `ImmutableHashSet<string>` (detached from Realm).
3. Assigns `FilterControl.ApplyRequiredCriteria`:
   ```csharp
   criteria.CollectionBeatmapMD5Hashes = collectionHashes;  // null → show all
   criteria.Ruleset = lockedRuleset;                         // null → no lock
   ```
4. `BeatmapCarouselFilterMatching.CheckCriteriaMatch` (existing code) applies the filter
   on every carousel refresh.

If the collection is not found, a warning is logged and all beatmaps are shown (fail-open
so the cabinet remains playable).

---

## Changes made to `osu.Game` files

These are the only changes outside `osu.Desktop/Arcade/`.  They are minimal, additive,
and non-breaking:

### `osu.Game/Screens/Loader.cs`
- `createMainMenu` local function promoted to `protected virtual MainMenu CreateMainMenu()`.
- All intro constructors now call `CreateMainMenu` instead of the local function.
- Enables `ArcadeLoader` to inject `ArcadeMainMenu` without duplicating intro selection logic.

### `osu.Game/Screens/Menu/MainMenu.cs`
- `protected virtual ButtonSystem CreateButtonSystem()` factory added.
- `Buttons = new ButtonSystem { ... }` refactored to `Buttons = CreateButtonSystem()` then
  individual callback assignments.  No behaviour change.
- `OnPressed` and `OnReleased` marked `virtual` to allow `ArcadeMainMenu` to override them.

### `osu.Game/Screens/Menu/ButtonSystem.cs`
- `buttonsTopLevel`, `buttonsPlay`, `buttonsMulti`, `buttonsEdit` changed from `private`
  to `protected` so `ArcadeButtonSystem` can populate them.
- Button-creation code extracted from `load()` into `protected virtual void PopulateButtons(GameHost host)`.
  `load()` now just calls `PopulateButtons` then adds the lists to `buttonArea`.

### `osu.Desktop/Properties/launchSettings.json`
- Added **"osu! Arcade"** launch profile with `--arcade` command-line argument.

---

## Configuration (`arcade.json`)

Loaded by `ArcadeConfiguration.LoadFromFile(path)` using Newtonsoft.Json.
Default path: `arcade.json` next to the executable.
Override path: `--arcade-config=<path>`.

```json
{
  "collectionName": "Arcade",   // required; exact name, case-sensitive
  "ruleset": null               // optional; "osu" | "taiko" | "fruits" | "mania"
}
```

`ArcadeConfiguration` is passed down the constructor chain:
`OsuGameArcade` → `ArcadeLoader` → `ArcadeMainMenu` → `ArcadeSongSelect`.
It is not cached in the DI container; each arcade class holds it as a constructor parameter.

---

## Dependency injection notes

- `OsuConfigManager` is **not** injectable as a BDL parameter in `OsuGameArcade` itself —
  it is registered *for children*, not for the game root's own BDL.
  Use `LocalConfig` (the `protected OsuConfigManager` property from `OsuGameBase`) directly,
  or do the work in `LoadComplete` where the full DI graph is ready.
- `FrameworkConfigManager` is registered by the framework host and resolved via `[Resolved]`.
- `RealmAccess` and `IRulesetStore` are registered by `OsuGameBase.CreateChildDependencies`
  and are injectable in `ArcadeSongSelect`'s BDL as normal.

---

## How to extend

**Add a new restricted screen:** subclass an existing screen, set
`HideOverlaysOnEnter => true` and `InitialOverlayActivationMode => OverlayActivation.Disabled`,
then push it from wherever in the arcade flow it belongs.

**Add a new config option:** add a property to `ArcadeConfiguration` with a `[JsonProperty]`
attribute and pass it through the constructor chain to wherever it is consumed.

**Add a staff-exit chord:** override `OsuGameArcade.OnPressed(KeyBindingPressEvent<GlobalAction>)`
to listen for a specific `GlobalAction` combination and call `base.Exit()` instead of the
no-op `AttemptExit()`.
