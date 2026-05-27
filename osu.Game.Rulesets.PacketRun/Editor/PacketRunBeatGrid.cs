// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.PacketRun.Charts;

namespace osu.Game.Rulesets.PacketRun.Editor
{
    public class PacketRunBeatGrid
    {
        private readonly double offsetMs;
        private readonly double timingStartMs;
        private readonly double bpm;
        private readonly int meter;
        private readonly double beatIntervalMs;

        public PacketRunBeatGrid(PacketRunChartFile chart)
        {
            offsetMs = chart.Metadata.OffsetMs;
            var timing = getPrimaryTiming(chart);
            timingStartMs = timing.TimeMs;
            bpm = timing.Bpm > 0 ? timing.Bpm : 120;
            meter = timing.Meter > 0 ? timing.Meter : 4;
            beatIntervalMs = 60000.0 / bpm;
        }

        public double BeatIntervalMs => beatIntervalMs;

        public double FirstBeatMs => offsetMs + timingStartMs;

        public IEnumerable<double> GetBeatTimes(double startMs, double endMs, bool majorOnly)
        {
            if (endMs < startMs)
            {
                yield break;
            }

            double firstBeat = FirstBeatMs;
            double gridStart = firstBeat;

            if (gridStart < startMs)
            {
                double beatsBefore = Math.Floor((startMs - gridStart) / beatIntervalMs);
                gridStart += beatsBefore * beatIntervalMs;
            }

            int beatIndex = (int)Math.Round((gridStart - firstBeat) / beatIntervalMs);

            for (double time = gridStart; time <= endMs; time += beatIntervalMs, beatIndex++)
            {
                if (time < startMs - 0.001)
                {
                    continue;
                }

                bool isMajor = beatIndex % meter == 0;

                if (majorOnly && !isMajor)
                {
                    continue;
                }

                yield return time;
            }
        }

        public bool IsMajorBeat(double timeMs)
        {
            double relative = timeMs - FirstBeatMs;
            if (relative < -0.001)
            {
                return false;
            }

            int beatIndex = (int)Math.Round(relative / beatIntervalMs);
            return beatIndex % meter == 0;
        }

        public double SnapToBeat(double timeMs, int divisor)
        {
            if (divisor < 1)
            {
                divisor = 1;
            }

            double interval = beatIntervalMs / divisor;
            double first = FirstBeatMs;
            double relative = timeMs - first;
            double steps = Math.Round(relative / interval);
            return first + steps * interval;
        }

        private static PacketRunChartTimingPoint getPrimaryTiming(PacketRunChartFile chart)
        {
            if (chart.Timing.Count > 0)
            {
                return chart.Timing[0];
            }

            return new PacketRunChartTimingPoint();
        }
    }
}
