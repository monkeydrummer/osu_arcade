# osu! Arcade / Cabinet Mode – Operator Setup Guide

## Overview

Arcade mode is a stripped-down kiosk experience designed for public cabinet use:

- Players see only a simplified song-select carousel restricted to a named collection.
- No settings, beatmap editor, skin editor, chat, or multiplayer navigation.
- Application cannot be exited from inside the game.
- No login prompts (guest / offline play only).
- Starts fullscreen automatically.

---

## Operator workflow (one-time setup)

### Step 1 – Normal first-run with a regular build

Launch osu! once **without** `--arcade` to complete first-run setup:

1. Go through the first-run wizard (UI scale, import beatmaps, etc.).
2. Import all the beatmaps you want on the cabinet.
3. In the **Collections** panel (F4 in song select), create a collection named exactly
   **`Arcade`** (or whatever name you plan to use).
4. Add every playable beatmap to that collection.
5. Optionally play through some maps to verify they work.
6. Exit normally.

### Step 2 – Create `arcade.json`

Copy the `arcade.json.example` file (next to the executable) to `arcade.json` and
edit it:

```json
{
  "collectionName": "Arcade",
  "ruleset": null
}
```

| Field | Type | Description |
|---|---|---|
| `collectionName` | string | **Required.** Exact name of the in-game collection. Case-sensitive. |
| `ruleset` | string or `null` | Short name of the forced ruleset: `"osu"`, `"taiko"`, `"fruits"`, `"mania"`. Set to `null` (or omit) to use the player's last-selected ruleset. |

#### Custom config path

If you prefer to store the config elsewhere, pass `--arcade-config=C:\path\to\arcade.json`.

### Step 3 – Launch in arcade mode

```
osu! --arcade
```

or with a custom config path:

```
osu! --arcade --arcade-config=C:\arcade\config.json
```

You can add this to a Windows shortcut, service wrapper, or auto-start entry.

---

## What is locked down in arcade mode

| Feature | Status |
|---|---|
| Settings overlay (Ctrl+O) | Blocked (OverlayActivation.Disabled) |
| Toolbar / rule-set switcher | Always hidden |
| Beatmap editor | Button removed from menu |
| Skin editor | Button removed from menu |
| Multiplayer / Playlists | Buttons removed from menu |
| Beatmap listing browser | Button removed from menu |
| Exit button / hold-to-exit | Removed; game cannot exit via UI |
| Alt+F4 / OS close | Calls `AttemptExit()` which is a no-op; OS-level kill still works |
| Login overlay | Blocked (OverlayActivation.Disabled) |
| First-run setup wizard | Suppressed automatically |
| Song select context menu | Reduced to "Play" only |
| Song select: all beatmaps | Restricted to collection hashes |
| Song select: ruleset tabs | Hidden (toolbar hidden) |

---

## Missing collection behavior

If `collectionName` does not match any existing collection at startup, **all beatmaps
are shown** (no filter applied) and a warning is logged:

```
[Arcade] Collection 'Arcade' not found. All beatmaps will be shown.
```

This is intentional – the cabinet is still playable while the operator investigates.
Logs are written to the standard osu! log folder.

---

## Updating beatmaps / the collection

1. Restart in normal mode (without `--arcade`).
2. Import new beatmaps and add them to the collection.
3. Restart with `--arcade`.

## Allowing staff to exit

Since `AttemptExit()` is a no-op in arcade mode, normal exit paths do not work.
Staff can:
- Use **Task Manager → End Task** (Windows).
- Send `SIGTERM` / `SIGKILL` from a service manager.
- Physically power-cycle the cabinet.

If a keyboard-chord exit is required, add a hidden `OsuGameArcade.OnPressed` handler
that listens for a specific `GlobalAction` combination and calls `base.Exit()`.

---

## Physical / OS-level lockdown (out of scope)

The following are **not** handled by this in-game mode and must be configured at the
OS/hardware level:

- Sticky Keys / Accessibility shortcuts (Windows Ease of Access)
- Task Manager access (Group Policy: `DisableTaskMgr`)
- Desktop / Explorer access (shell replacement or kiosk mode policy)
- Physical keyboard removal / lockdown
- Network access restrictions
