// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.Database;
using osu.Game.Rulesets.PacketRun.Database;
using osu.Game.Rulesets.PacketRun.Screens;
using osu.Game.Screens;

namespace osu.Desktop
{
    public partial class PacketRunLoader : Loader
    {
        protected override OsuScreen CreateLoadableScreen() => new PacketRunMainMenu();
    }

    /// <summary>
    /// Skips osu! lazer's online beatmap metadata maintenance. Packet Run uses local JSON charts only.
    /// </summary>
    public partial class PacketRunBackgroundDataStoreProcessor : BackgroundDataStoreProcessor
    {
        protected override void LoadComplete()
        {
        }
    }

    public partial class PacketRunGame : OsuGame
    {
        private DependencyContainer packetRunDependencies = null!;

        public PacketRunGame(string[]? args = null)
            : base(args)
        {
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            packetRunDependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            return packetRunDependencies;
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            packetRunDependencies.CacheAs(new PacketRunSaveStore(storage));
        }

        protected override Loader CreateLoader() => new PacketRunLoader();

        protected override BackgroundDataStoreProcessor CreateBackgroundDataStoreProcessor() => new PacketRunBackgroundDataStoreProcessor();
    }
}
