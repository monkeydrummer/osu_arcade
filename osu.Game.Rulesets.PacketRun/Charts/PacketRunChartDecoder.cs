// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Text.Json;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.PacketRun.Objects;

namespace osu.Game.Rulesets.PacketRun.Charts
{
    public static class PacketRunChartDecoder
    {
        public static PacketRunBeatmap Decode(string json, BeatmapMetadata? metadataOverride = null)
        {
            var chart = JsonSerializer.Deserialize<PacketRunChartFile>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            }) ?? throw new InvalidDataException("Invalid Packet Run chart JSON.");

            var metadata = metadataOverride ?? new BeatmapMetadata
            {
                Title = chart.Metadata.Title,
                Artist = chart.Metadata.Artist,
                AudioFile = chart.Metadata.AudioFile,
            };

            var beatmap = new PacketRunBeatmap
            {
                BeatmapInfo = new BeatmapInfo { Metadata = metadata },
                DefaultMode = parseMode(chart.Metadata.DefaultMode),
            };

            beatmap.AudioLeadIn = chart.Metadata.OffsetMs;

            foreach (var timing in chart.Timing)
            {
                beatmap.ControlPointInfo.Add(timing.TimeMs, new TimingControlPoint
                {
                    BeatLength = 60000 / timing.Bpm,
                });
            }

            foreach (var section in chart.Sections)
            {
                beatmap.ModeSections.Add(new PacketRunModeSection
                {
                    StartTime = section.StartTimeMs,
                    Mode = parseMode(section.Mode),
                });
            }

            foreach (var packet in chart.Packets)
            {
                beatmap.HitObjects.Add(new PacketHitObject
                {
                    StartTime = packet.TimeMs,
                    Digits = packet.Digits,
                    Variant = parseVariant(packet.Variant),
                    Layout = parseLayout(packet.Layout),
                    ModeOverride = string.IsNullOrEmpty(packet.Mode) ? null : parseMode(packet.Mode),
                });
            }

            return beatmap;
        }

        public static PacketRunChartFile DecodeFile(string path) =>
            JsonSerializer.Deserialize<PacketRunChartFile>(File.ReadAllText(path), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            }) ?? throw new InvalidDataException("Invalid Packet Run chart JSON.");

        private static PacketGameplayMode parseMode(string mode) =>
            mode.Equals("rhythm", StringComparison.OrdinalIgnoreCase) ? PacketGameplayMode.Rhythm : PacketGameplayMode.Queue;

        private static PacketLayout parseLayout(string layout) =>
            layout.Equals("vertical", StringComparison.OrdinalIgnoreCase) ? PacketLayout.Vertical : PacketLayout.Horizontal;

        private static PacketVariant parseVariant(string variant) =>
            Enum.TryParse<PacketVariant>(variant, true, out var parsed) ? parsed : PacketVariant.Custom;
    }
}
