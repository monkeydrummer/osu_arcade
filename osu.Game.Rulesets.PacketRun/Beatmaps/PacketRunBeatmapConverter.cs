// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.PacketRun.Objects;

namespace osu.Game.Rulesets.PacketRun.Beatmaps
{
    public class PacketRunBeatmapConverter : BeatmapConverter<PacketHitObject>
    {
        public PacketRunBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
        }

        public override bool CanConvert() => true;

        protected override Beatmap<PacketHitObject> CreateBeatmap() => new PacketRunBeatmap();

        protected override Beatmap<PacketHitObject> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
        {
            var converted = base.ConvertBeatmap(original, cancellationToken);

            if (original is PacketRunBeatmap source && converted is PacketRunBeatmap target)
            {
                target.DefaultMode = source.DefaultMode;
                target.DefaultLayout = source.DefaultLayout;
                target.ModeSections = source.ModeSections.Select(section => new PacketRunModeSection
                {
                    StartTime = section.StartTime,
                    Mode = section.Mode,
                    Layout = section.Layout,
                }).ToList();
            }

            return converted;
        }

        protected override IEnumerable<PacketHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            if (original is PacketHitObject packet)
            {
                yield return packet;
            }
        }
    }
}
