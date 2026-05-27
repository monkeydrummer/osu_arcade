// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game;
using osu.Game.Database;
using osu.Game.Rulesets.PacketRun.Editor;
using osu.Game.Rulesets.PacketRun.Editor.Screens;
using osu.Game.Screens;

namespace osu.Desktop.Editor
{
    public partial class PacketRunEditorLoader : Loader
    {
        protected override OsuScreen CreateLoadableScreen() => new EditorMainMenu();
    }

    public partial class PacketRunEditorBackgroundDataStoreProcessor : BackgroundDataStoreProcessor
    {
        protected override void LoadComplete()
        {
        }
    }

    public partial class PacketRunEditorGame : OsuGame
    {
        private DependencyContainer editorDependencies = null!;

        public PacketRunEditorGame(string[]? args = null)
            : base(args)
        {
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            editorDependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            return editorDependencies;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            editorDependencies.CacheAs(new PacketRunEditorSession());
        }

        protected override Loader CreateLoader() => new PacketRunEditorLoader();

        protected override BackgroundDataStoreProcessor CreateBackgroundDataStoreProcessor() => new PacketRunEditorBackgroundDataStoreProcessor();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Toolbar.Hide();
        }

        public override void AttemptExit()
        {
            PerformFromScreen(menu => menu.Exit(), new[] { typeof(EditorMainMenu) });
        }

        protected override void ScreenChanged(IOsuScreen? current, IOsuScreen? newScreen)
        {
            base.ScreenChanged(current, newScreen);
            Toolbar.Hide();
        }
    }
}
