// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.PacketRun.Judgements;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Rulesets.PacketRun.Scoring;

namespace osu.Game.Rulesets.PacketRun.Objects
{
    public class PacketHitObject : HitObject
    {
        public int[] Digits { get; set; } = System.Array.Empty<int>();

        public PacketVariant Variant { get; set; } = PacketVariant.Custom;

        public PacketGameplayMode? ModeOverride { get; set; }

        public override Judgement CreateJudgement() => new PacketJudgement();

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);
            HitWindows = new PacketRunHitWindows();
        }
    }

    public class PacketRunModeSection
    {
        public double StartTime { get; set; }

        public PacketGameplayMode Mode { get; set; }

        public PacketLayout? Layout { get; set; }
    }

    public class PacketRunBeatmap : Beatmap<PacketHitObject>
    {
        public PacketGameplayMode DefaultMode { get; set; } = PacketGameplayMode.Queue;

        public PacketLayout DefaultLayout { get; set; } = PacketLayout.Horizontal;

        public List<PacketRunModeSection> ModeSections { get; set; } = new List<PacketRunModeSection>();

        public PacketGameplayMode GetModeAt(double time)
        {
            PacketGameplayMode mode = DefaultMode;

            foreach (var section in ModeSections)
            {
                if (section.StartTime <= time)
                {
                    mode = section.Mode;
                }
                else
                {
                    break;
                }
            }

            return mode;
        }

        public PacketLayout GetLayoutAt(double time)
        {
            PacketLayout layout = DefaultLayout;

            foreach (var section in ModeSections)
            {
                if (section.StartTime <= time)
                {
                    if (section.Layout.HasValue)
                    {
                        layout = section.Layout.Value;
                    }
                }
                else
                {
                    break;
                }
            }

            return layout;
        }

        public static PacketRunBeatmap From(IBeatmap beatmap)
        {
            if (beatmap is PacketRunBeatmap packetRunBeatmap)
            {
                return packetRunBeatmap;
            }

            var fallback = new PacketRunBeatmap
            {
                BeatmapInfo = beatmap.BeatmapInfo,
                ControlPointInfo = beatmap.ControlPointInfo,
                HitObjects = beatmap.HitObjects.Cast<PacketHitObject>().ToList(),
            };

            fallback.AudioLeadIn = beatmap.AudioLeadIn;
            fallback.StackLeniency = beatmap.StackLeniency;
            fallback.SpecialStyle = beatmap.SpecialStyle;
            fallback.LetterboxInBreaks = beatmap.LetterboxInBreaks;
            fallback.WidescreenStoryboard = beatmap.WidescreenStoryboard;
            fallback.EpilepsyWarning = beatmap.EpilepsyWarning;
            fallback.SamplesMatchPlaybackRate = beatmap.SamplesMatchPlaybackRate;
            fallback.DistanceSpacing = beatmap.DistanceSpacing;
            fallback.GridSize = beatmap.GridSize;
            fallback.TimelineZoom = beatmap.TimelineZoom;
            fallback.Countdown = beatmap.Countdown;
            fallback.CountdownOffset = beatmap.CountdownOffset;
            fallback.Bookmarks = beatmap.Bookmarks;
            fallback.BeatmapVersion = beatmap.BeatmapVersion;
            fallback.Breaks = beatmap.Breaks;

            return fallback;
        }
    }
}
