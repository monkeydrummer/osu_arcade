// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace osu.Game.Rulesets.PacketRun.Charts
{
    public class PacketRunChartFile
    {
        [JsonPropertyName("format_version")]
        public int FormatVersion { get; set; } = 1;

        [JsonPropertyName("metadata")]
        public PacketRunChartMetadata Metadata { get; set; } = new PacketRunChartMetadata();

        [JsonPropertyName("timing")]
        public List<PacketRunChartTimingPoint> Timing { get; set; } = new List<PacketRunChartTimingPoint>();

        [JsonPropertyName("sections")]
        public List<PacketRunChartSection> Sections { get; set; } = new List<PacketRunChartSection>();

        [JsonPropertyName("packets")]
        public List<PacketRunChartPacket> Packets { get; set; } = new List<PacketRunChartPacket>();
    }

    public class PacketRunChartMetadata
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("artist")]
        public string Artist { get; set; } = string.Empty;

        [JsonPropertyName("audio_file")]
        public string AudioFile { get; set; } = string.Empty;

        [JsonPropertyName("offset_ms")]
        public double OffsetMs { get; set; }

        [JsonPropertyName("default_mode")]
        public string DefaultMode { get; set; } = "queue";

        [JsonPropertyName("default_layout")]
        public string DefaultLayout { get; set; } = "horizontal";

        [JsonPropertyName("story_chapter")]
        public int? StoryChapter { get; set; }

        [JsonPropertyName("story_level")]
        public int? StoryLevel { get; set; }
    }

    public class PacketRunChartTimingPoint
    {
        [JsonPropertyName("time_ms")]
        public double TimeMs { get; set; }

        [JsonPropertyName("bpm")]
        public double Bpm { get; set; } = 120;

        [JsonPropertyName("meter")]
        public int Meter { get; set; } = 4;

        [JsonPropertyName("meter_denominator")]
        public int MeterDenominator { get; set; } = 4;
    }

    public class PacketRunChartSection
    {
        [JsonPropertyName("start_time_ms")]
        public double StartTimeMs { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; } = "queue";

        [JsonPropertyName("layout")]
        public string? Layout { get; set; }
    }

    public class PacketRunChartPacket
    {
        [JsonPropertyName("time_ms")]
        public double TimeMs { get; set; }

        [JsonPropertyName("digits")]
        public int[] Digits { get; set; } = System.Array.Empty<int>();

        [JsonPropertyName("variant")]
        public string Variant { get; set; } = "custom";

        [JsonPropertyName("mode")]
        public string? Mode { get; set; }
    }
}
