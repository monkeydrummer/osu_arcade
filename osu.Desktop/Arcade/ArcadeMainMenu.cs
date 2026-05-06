// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Screens.Menu;

namespace osu.Desktop.Arcade
{
    /// <summary>
    /// A simplified <see cref="MainMenu"/> for arcade / kiosk mode.
    /// <list type="bullet">
    ///   <item>Uses <see cref="ArcadeButtonSystem"/> – Play ➜ Solo path only.</item>
    ///   <item>Keeps toolbar and all overlays disabled at all times.</item>
    ///   <item>Blocks all attempts to exit the application.</item>
    ///   <item>Suppresses login prompts (overlays are blocked by <see cref="OverlayActivation.Disabled"/>).</item>
    /// </list>
    /// </summary>
    public partial class ArcadeMainMenu : MainMenu
    {
        private readonly ArcadeConfiguration arcadeConfig;

        public ArcadeMainMenu(ArcadeConfiguration arcadeConfig)
        {
            this.arcadeConfig = arcadeConfig;
        }

        // Always report overlays as hidden so the toolbar never shows and
        // OverlayActivationMode stays Disabled.
        public override bool HideOverlaysOnEnter => true;

        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.Disabled;

        // Swap in the kiosk-only button system before parent BDL assigns callbacks.
        protected override ButtonSystem CreateButtonSystem() => new ArcadeButtonSystem();

        [BackgroundDependencyLoader]
        private void load()
        {
            // Override Solo to push the arcade song select.
            Buttons.OnSolo = () => this.Push(new ArcadeSongSelect(arcadeConfig));

            // Settings and beatmap listing are blocked by OverlayActivation anyway,
            // but null out the callbacks explicitly so there's no risk.
            Buttons.OnSettings = null;
            Buttons.OnBeatmapListing = null;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Remove any "hold to exit" overlay that the parent may have added
            // (added when host.CanExit == true on desktop).
            foreach (var overlay in InternalChildren.OfType<HoldToExitGameOverlay>().ToList())
                RemoveInternal(overlay, true);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            // Never allow the main menu to exit in arcade mode – doing so would close the app.
            return true;
        }

        // Prevent Back key from suspending to background.
        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e) => false;

        public override void OnReleased(KeyBindingReleaseEvent<GlobalAction> e) { }
    }
}
