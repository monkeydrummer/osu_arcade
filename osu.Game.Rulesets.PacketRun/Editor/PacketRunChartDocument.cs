// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Game.Rulesets.PacketRun.Charts;
using osu.Game.Rulesets.PacketRun.Objects;

namespace osu.Game.Rulesets.PacketRun.Editor
{
    public class PacketRunChartSetupSettings
    {
        public int DigitsPerPacket { get; set; } = 3;

        public PacketGameplayMode DefaultMode { get; set; } = PacketGameplayMode.Queue;

        public PacketLayout DefaultLayout { get; set; } = PacketLayout.Horizontal;

        public PacketRunTimeSignature TimeSignature { get; set; } = PacketRunTimeSignature.CommonFourFour;
    }

    public class PacketRunEditorViewSettings
    {
        public bool ShowBeatLines { get; set; } = true;

        public bool ShowMinorBeatLines { get; set; } = true;

        public bool ShowVerticalDigits { get; set; }

        public bool SnapEnabled { get; set; } = true;

        public int SnapDivisor { get; set; } = 1;
    }

    public class PacketRunChartDocument
    {
        public PacketRunChartFile Chart { get; private set; } = new PacketRunChartFile();

        public string? FilePath { get; private set; }

        public string SongDirectory { get; private set; } = string.Empty;

        public bool IsDirty { get; private set; }

        public PacketRunChartSetupSettings SetupSettings { get; } = new PacketRunChartSetupSettings();

        public PacketRunEditorViewSettings ViewSettings { get; } = new PacketRunEditorViewSettings();

        public void Load(string chartPath)
        {
            Chart = PacketRunChartDecoder.DecodeFile(chartPath);
            FilePath = chartPath;
            SongDirectory = Path.GetDirectoryName(chartPath)!;
            IsDirty = false;

            if (Chart.Timing.Count > 0)
            {
                SetupSettings.TimeSignature = PacketRunTimeSignature.FromTimingPoint(Chart.Timing[0]);
            }

            syncSetupFromMetadata();
        }

        public void NewBlank(string songDirectory, string audioFileName, string title, string artist)
        {
            Chart = createBlankChart(audioFileName, title, artist);
            SongDirectory = songDirectory;
            FilePath = Path.Combine(songDirectory, "chart.packet.json");
            IsDirty = true;
            syncSetupFromMetadata();
        }

        public static PacketRunChartDocument FromImport(string sourceAudioPath, string contentRoot)
        {
            string fileName = Path.GetFileName(sourceAudioPath);
            string slug = slugify(Path.GetFileNameWithoutExtension(fileName));
            string songDirectory = Path.Combine(contentRoot, slug);

            int suffix = 1;

            while (Directory.Exists(songDirectory))
            {
                songDirectory = Path.Combine(contentRoot, $"{slug}_{suffix++}");
            }

            Directory.CreateDirectory(songDirectory);
            string destAudioPath = Path.Combine(songDirectory, fileName);
            File.Copy(sourceAudioPath, destAudioPath);

            var document = new PacketRunChartDocument();
            string title = Path.GetFileNameWithoutExtension(fileName);
            document.NewBlank(songDirectory, fileName, title, string.Empty);
            return document;
        }

        public PacketRunChartTimingPoint GetPrimaryTiming()
        {
            if (Chart.Timing.Count == 0)
            {
                Chart.Timing.Add(new PacketRunChartTimingPoint());
            }

            return Chart.Timing[0];
        }

        public void ApplyTimingResult(double bpm, double offsetMs, PacketRunTimeSignature timeSignature)
        {
            Chart.Metadata.OffsetMs = offsetMs;
            var timing = GetPrimaryTiming();
            timing.TimeMs = 0;
            timing.Bpm = bpm;
            timeSignature.ApplyTo(timing);
            SetupSettings.TimeSignature = timeSignature;
            MarkDirty();
        }

        public void SyncMetadataFromSetup()
        {
            Chart.Metadata.Title = Chart.Metadata.Title ?? string.Empty;
            Chart.Metadata.DefaultMode = SetupSettings.DefaultMode == PacketGameplayMode.Rhythm ? "rhythm" : "queue";
            Chart.Metadata.DefaultLayout = SetupSettings.DefaultLayout == PacketLayout.Vertical ? "vertical" : "horizontal";
            SetupSettings.TimeSignature.ApplyTo(GetPrimaryTiming());

            if (Chart.Sections.Count == 0)
            {
                Chart.Sections.Add(new PacketRunChartSection());
            }

            Chart.Sections[0].StartTimeMs = 0;
            Chart.Sections[0].Mode = Chart.Metadata.DefaultMode;
            Chart.Sections[0].Layout = Chart.Metadata.DefaultLayout;
            MarkDirty();
        }

        public void AddPacket(PacketRunChartPacket packet)
        {
            Chart.Packets.Add(packet);
            SortPackets();
            MarkDirty();
        }

        public void AddPackets(IEnumerable<PacketRunChartPacket> packets)
        {
            Chart.Packets.AddRange(packets);
            SortPackets();
            MarkDirty();
        }

        public void UpdatePacket(PacketRunChartPacket packet, double timeMs, int[] digits, string variant, string? mode)
        {
            packet.TimeMs = timeMs;
            packet.Digits = digits;
            packet.Variant = variant;
            packet.Mode = mode;
            SortPackets();
            MarkDirty();
        }

        public void RemovePackets(IEnumerable<PacketRunChartPacket> packets)
        {
            foreach (var packet in packets.ToList())
            {
                Chart.Packets.Remove(packet);
            }

            MarkDirty();
        }

        public void SortPackets() => Chart.Packets.Sort((a, b) => a.TimeMs.CompareTo(b.TimeMs));

        public double SnapTime(double timeMs)
        {
            if (!ViewSettings.SnapEnabled)
            {
                return timeMs;
            }

            return new PacketRunBeatGrid(Chart).SnapToBeat(timeMs, ViewSettings.SnapDivisor);
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                throw new InvalidOperationException("No file path set. Use SaveAs.");
            }

            SaveAs(FilePath);
        }

        public void SaveAs(string path)
        {
            SortPackets();
            validate();
            PacketRunChartEncoder.EncodeFile(Chart, path);
            FilePath = path;
            SongDirectory = Path.GetDirectoryName(path)!;
            IsDirty = false;
        }

        public void MarkDirty() => IsDirty = true;

        public PacketRunBeatGrid CreateBeatGrid() => new PacketRunBeatGrid(Chart);

        private void syncSetupFromMetadata()
        {
            SetupSettings.DefaultMode = Chart.Metadata.DefaultMode.Equals("rhythm", StringComparison.OrdinalIgnoreCase)
                ? PacketGameplayMode.Rhythm
                : PacketGameplayMode.Queue;
            SetupSettings.DefaultLayout = Chart.Metadata.DefaultLayout.Equals("vertical", StringComparison.OrdinalIgnoreCase)
                ? PacketLayout.Vertical
                : PacketLayout.Horizontal;

            if (Chart.Timing.Count > 0)
            {
                SetupSettings.TimeSignature = PacketRunTimeSignature.FromTimingPoint(Chart.Timing[0]);
            }
        }

        private static PacketRunChartFile createBlankChart(string audioFileName, string title, string artist)
        {
            var chart = new PacketRunChartFile();
            chart.Metadata.Title = title;
            chart.Metadata.Artist = artist;
            chart.Metadata.AudioFile = audioFileName;
            chart.Metadata.OffsetMs = 0;
            chart.Metadata.DefaultMode = "queue";
            chart.Metadata.DefaultLayout = "horizontal";
            chart.Timing.Add(new PacketRunChartTimingPoint { TimeMs = 0, Bpm = 120, Meter = 4, MeterDenominator = 4 });
            chart.Sections.Add(new PacketRunChartSection { StartTimeMs = 0, Mode = "queue", Layout = "horizontal" });
            return chart;
        }

        private void validate()
        {
            foreach (var packet in Chart.Packets)
            {
                if (packet.Digits.Any(d => d < 0 || d > 9))
                {
                    throw new InvalidDataException("Packet digits must be between 0 and 9.");
                }
            }

            if (PacketRunAudioPathResolver.TryResolve(Chart.Metadata.AudioFile, SongDirectory, FilePath, out string audioPath) != PacketRunAudioResolveResult.FoundOnDisk)
            {
                throw new FileNotFoundException($"Audio file not found: {audioPath}");
            }
        }

        private static string slugify(string name)
        {
            string slug = Regex.Replace(name.ToLowerInvariant(), @"[^a-z0-9]+", "_").Trim('_');

            if (string.IsNullOrEmpty(slug))
            {
                slug = "song";
            }

            return slug;
        }
    }
}
