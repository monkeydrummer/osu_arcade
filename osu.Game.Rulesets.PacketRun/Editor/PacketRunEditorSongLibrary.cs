// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Game.Rulesets.PacketRun.Charts;

namespace osu.Game.Rulesets.PacketRun.Editor
{
    public class PacketRunEditorSongEntry
    {
        public string Id { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Artist { get; init; } = string.Empty;

        public string Directory { get; init; } = string.Empty;

        public string? ChartPath { get; init; }

        public string? AudioPath { get; init; }

        public bool HasChart { get; init; }

        public PacketRunChartFile? Chart { get; init; }
    }

    public class PacketRunEditorSongLibrary
    {
        private static readonly string[] audio_extensions = { ".ogg", ".mp3", ".wav", ".flac" };

        public IReadOnlyList<PacketRunEditorSongEntry> Songs => songs;

        private readonly List<PacketRunEditorSongEntry> songs = new List<PacketRunEditorSongEntry>();

        public void Scan(string contentRoot)
        {
            songs.Clear();

            if (!Directory.Exists(contentRoot))
            {
                return;
            }

            foreach (var songDir in Directory.GetDirectories(contentRoot))
            {
                string? chartPath = Directory.GetFiles(songDir, "*.packet.json").FirstOrDefault();
                string? audioPath = PacketRunPathUtils.EnumerateFiles(songDir, "*.*")
                    .FirstOrDefault(f => audio_extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));

                if (chartPath == null && audioPath == null)
                {
                    continue;
                }

                PacketRunChartFile? chart = null;
                string title = Path.GetFileName(songDir);
                string artist = string.Empty;

                if (chartPath != null)
                {
                    chart = PacketRunChartDecoder.DecodeFile(chartPath);
                    title = chart.Metadata.Title;
                    artist = chart.Metadata.Artist;

                    if (audioPath == null && !string.IsNullOrEmpty(chart.Metadata.AudioFile))
                    {
                        string referenced = Path.Combine(songDir, chart.Metadata.AudioFile);

                        if (PacketRunPathUtils.FileExists(referenced))
                        {
                            audioPath = PacketRunPathUtils.ToFullPath(referenced);
                        }
                        else
                        {
                            audioPath = PacketRunPathUtils.FindFileByName(songDir, chart.Metadata.AudioFile);
                        }
                    }
                }
                else if (audioPath != null)
                {
                    title = Path.GetFileNameWithoutExtension(audioPath);
                }

                songs.Add(new PacketRunEditorSongEntry
                {
                    Id = Path.GetFileName(songDir),
                    Title = title,
                    Artist = artist,
                    Directory = songDir,
                    ChartPath = chartPath,
                    AudioPath = audioPath,
                    HasChart = chartPath != null,
                    Chart = chart,
                });
            }

            songs.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));
        }
    }
}
