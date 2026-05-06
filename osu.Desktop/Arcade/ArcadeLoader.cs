// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens;
using osu.Game.Screens.Menu;

namespace osu.Desktop.Arcade
{
    /// <summary>
    /// A <see cref="Loader"/> variant that injects an <see cref="ArcadeMainMenu"/> as the
    /// post-intro destination.
    /// </summary>
    public partial class ArcadeLoader : Loader
    {
        private readonly ArcadeConfiguration arcadeConfig;

        public ArcadeLoader(ArcadeConfiguration arcadeConfig)
        {
            this.arcadeConfig = arcadeConfig;
        }

        protected override MainMenu CreateMainMenu() => new ArcadeMainMenu(arcadeConfig);
    }
}
