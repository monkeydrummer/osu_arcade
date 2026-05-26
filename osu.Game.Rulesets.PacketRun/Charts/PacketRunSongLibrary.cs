// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.Game.Rulesets.PacketRun.Charts
{
    public class PacketRunSongEntry
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Artist { get; init; } = string.Empty;
        public string Directory { get; init; } = string.Empty;
        public string? ChartPath { get; init; }
        public PacketRunChartFile? Chart { get; init; }
    }

    public class PacketRunSongLibrary
    {
        public IReadOnlyList<PacketRunSongEntry> Songs => songs;

        private readonly List<PacketRunSongEntry> songs = new List<PacketRunSongEntry>();

        public void Scan(string contentRoot)
        {
            songs.Clear();

            if (!Directory.Exists(contentRoot))
            {
                return;
            }

            foreach (var songDir in Directory.GetDirectories(contentRoot))
            {
                string chartPath = Directory.GetFiles(songDir, "*.packet.json").FirstOrDefault();

                if (chartPath == null)
                {
                    continue;
                }

                var chart = PacketRunChartDecoder.DecodeFile(chartPath);

                songs.Add(new PacketRunSongEntry
                {
                    Id = Path.GetFileName(songDir),
                    Title = chart.Metadata.Title,
                    Artist = chart.Metadata.Artist,
                    Directory = songDir,
                    ChartPath = chartPath,
                    Chart = chart,
                });
            }
        }
    }
}
