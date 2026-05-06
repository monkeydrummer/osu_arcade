// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Screens;

namespace osu.Desktop.Arcade
{
    /// <summary>
    /// The arcade / kiosk variant of the desktop game.
    /// Inherits all desktop infrastructure (updater, IPC, stable import) from
    /// <see cref="OsuGameDesktop"/> while disabling UI affordances that are
    /// inappropriate for a public arcade cabinet:
    /// <list type="bullet">
    ///   <item>Replaces the standard loader/main-menu with arcade-only counterparts.</item>
    ///   <item>Blocks all in-game exit paths.</item>
    ///   <item>Suppresses the first-run setup overlay.</item>
    ///   <item>Forces fullscreen mode on startup.</item>
    /// </list>
    /// </summary>
    internal partial class OsuGameArcade : OsuGameDesktop
    {
        private readonly ArcadeConfiguration arcadeConfig;

        // FrameworkConfigManager is registered by the framework host and resolvable via [Resolved].
        [Resolved]
        private FrameworkConfigManager frameworkConfig { get; set; } = null!;

        public OsuGameArcade(ArcadeConfiguration arcadeConfig, string[]? args = null)
            : base(args)
        {
            this.arcadeConfig = arcadeConfig;
        }

        protected override Loader CreateLoader() => new ArcadeLoader(arcadeConfig);

        /// <summary>
        /// Block all exit paths.  The application can only be terminated at the OS level.
        /// </summary>
        public override void AttemptExit() { }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Suppress the first-run setup wizard – operators complete it once with a normal build.
            // LocalConfig is the protected OsuConfigManager property from OsuGameBase,
            // available after SetHost() runs (before LoadComplete).
            LocalConfig.SetValue(OsuSetting.ShowFirstRunSetup, false);

            // Force fullscreen so the game fills the cabinet screen.
            var windowMode = frameworkConfig.GetBindable<WindowMode>(FrameworkSetting.WindowMode);
            windowMode.Value = WindowMode.Fullscreen;
        }

        protected override void ScreenChanged([CanBeNull] IOsuScreen current, [CanBeNull] IOsuScreen newScreen)
        {
            base.ScreenChanged(current, newScreen);

            // Always hide the toolbar regardless of the incoming screen's preference.
            Toolbar?.Hide();
        }
    }
}
