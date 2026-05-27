// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Rulesets.PacketRun.Screens
{
    public partial class PacketRunResultsScreen : SoloResultsScreen
    {
        public PacketRunResultsScreen(ScoreInfo score)
            : base(score)
        {
        }

        protected override bool ShowCollectionButton => false;
    }
}
