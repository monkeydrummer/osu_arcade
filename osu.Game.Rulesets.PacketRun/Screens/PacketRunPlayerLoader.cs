// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.PacketRun.Screens
{
    public partial class PacketRunPlayerLoader : PlayerLoader
    {
        public PacketRunPlayerLoader(Func<Player> createPlayer)
            : base(createPlayer)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            PlayerSettings.Expire();
        }

        protected override bool ShowSideSettings => false;

        protected override double PlayerPushDelay => 0;
    }
}
