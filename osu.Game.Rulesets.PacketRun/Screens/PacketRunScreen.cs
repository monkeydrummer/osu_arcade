// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.PacketRun.Charts;
using osu.Game.Screens;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.PacketRun.Screens
{
    public abstract partial class PacketRunScreen : OsuScreen
    {
        public override bool ShowFooter => true;

        public override bool AllowUserExit => true;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        protected void StartGameplay(WorkingBeatmap workingBeatmap)
        {
            Beatmap.Value = workingBeatmap;
            Ruleset.Value = rulesets.GetRuleset(PacketRunRuleset.SHORT_NAME)
                            ?? rulesets.AvailableRulesets.First();

            this.Push(new PlayerLoader(() => new PacketRunSoloPlayer()));
        }

        protected static string GetContentRoot()
        {
            string devPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "PacketRun.Content", "songs");

            if (System.IO.Directory.Exists(devPath))
            {
                return devPath;
            }

            return System.IO.Path.Combine(System.AppContext.BaseDirectory, "PacketRun.Content", "songs");
        }
    }
}
