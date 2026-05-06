// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Desktop.Arcade
{
    /// <summary>
    /// A stripped-down <see cref="ButtonSystem"/> for arcade / kiosk mode.
    /// Only exposes the Play ➜ Solo navigation path; all other buttons
    /// (Edit, Browse, Multiplayer, Exit) are omitted.
    /// </summary>
    public partial class ArcadeButtonSystem : ButtonSystem
    {
        // Hide the Settings gear button to the left of the logo in regular arcade mode.
        protected override bool ShowSettingsButton => false;

        protected override void PopulateButtons(GameHost host)
        {
            // Play sub-menu: Solo only
            buttonsPlay.Add(new MainMenuButton(
                ButtonSystemStrings.Solo,
                @"button-default-select",
                OsuIcon.Player,
                new Color4(102, 68, 204, 255),
                (_, _) => OnSolo?.Invoke())
            {
                Padding = new MarginPadding { Left = WEDGE_WIDTH },
            });
            buttonsPlay.ForEach(b => b.VisibleState = ButtonSystemState.Play);

            // Top-level: Play only (no Edit, Browse, or Exit)
            buttonsTopLevel.Add(new MainMenuButton(
                ButtonSystemStrings.Play,
                @"button-play-select",
                OsuIcon.Logo,
                new Color4(102, 68, 204, 255),
                (_, _) => State = ButtonSystemState.Play)
            {
                Padding = new MarginPadding { Left = WEDGE_WIDTH },
            });

            // buttonsMulti and buttonsEdit intentionally left empty.
        }
    }
}
