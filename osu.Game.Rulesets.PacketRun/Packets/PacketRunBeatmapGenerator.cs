// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.PacketRun.Objects;

namespace osu.Game.Rulesets.PacketRun.Packets
{
    public class PacketRunBeatmapGenerator
    {
        private readonly PacketGeneratorRegistry registry = new PacketGeneratorRegistry();

        public PacketRunBeatmap Generate(
            BeatmapMetadata metadata,
            IEnumerable<(double timeMs, double bpm)> timingPoints,
            PacketModFlags mods,
            PacketGameplayMode mode,
            int packetCount = 32,
            int? seed = null)
        {
            var random = seed.HasValue ? new Random(seed.Value) : new Random();

            double scrollTier = mode == PacketGameplayMode.Rhythm ? 1.5 : 1.0;

            var context = new PacketGenerationContext
            {
                Random = random,
                EnabledMods = mods,
                MinLength = 3,
                MaxLength = 3,
                PreferredLayout = scrollTier >= 1.5 ? PacketLayout.Vertical : PacketLayout.Horizontal,
                ScrollSpeedTier = scrollTier,
            };

            var beatmap = new PacketRunBeatmap
            {
                BeatmapInfo = new BeatmapInfo { Metadata = metadata },
                DefaultMode = mode,
                DefaultLayout = context.PreferredLayout,
            };

            foreach (var (timeMs, bpm) in timingPoints)
            {
                beatmap.ControlPointInfo.Add(timeMs, new TimingControlPoint { BeatLength = 60000 / bpm });
            }

            if (!beatmap.ControlPointInfo.TimingPoints.Any())
            {
                beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });
            }

            double beatLength = beatmap.ControlPointInfo.TimingPoints.First().BeatLength;
            double start = beatLength * 4;
            double spacing = getPacketSpacing(beatLength, mode);

            for (int i = 0; i < packetCount; i++)
            {
                var generated = registry.Generate(context);

                beatmap.HitObjects.Add(new PacketHitObject
                {
                    StartTime = start + i * spacing,
                    Digits = generated.Digits,
                    Variant = generated.Variant,
                });
            }

            return beatmap;
        }

        /// <summary>
        /// Calculates how many packets are required to cover a song of the given length.
        /// </summary>
        public static int CalculatePacketCountForSongLength(
            double songLengthMs,
            IEnumerable<(double timeMs, double bpm)> timingPoints,
            PacketGameplayMode mode)
        {
            if (songLengthMs <= 0)
            {
                return 1;
            }

            double beatLength = resolveBeatLength(timingPoints);
            double start = beatLength * 4;
            double spacing = getPacketSpacing(beatLength, mode);

            if (spacing <= 0 || songLengthMs <= start)
            {
                return 1;
            }

            return Math.Max(1, (int)Math.Floor((songLengthMs - start) / spacing) + 1);
        }

        private static double resolveBeatLength(IEnumerable<(double timeMs, double bpm)> timingPoints)
        {
            var firstTiming = timingPoints.FirstOrDefault();

            if (firstTiming.bpm > 0)
            {
                return 60000 / firstTiming.bpm;
            }

            return 500;
        }

        private static double getPacketSpacing(double beatLength, PacketGameplayMode mode) =>
            mode == PacketGameplayMode.Rhythm ? beatLength * 2 : beatLength;
    }
}
