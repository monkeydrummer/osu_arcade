// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using osu.Framework.Platform;

namespace osu.Game.Rulesets.PacketRun.Database
{
    public class PacketRunProfile
    {
        public int SlotIndex { get; set; }
        public string Name { get; set; } = "Runner";
        public int? ActiveStoryLevel { get; set; }
    }

    public class PacketRunStoryProgress
    {
        public int ProfileSlot { get; set; }
        public List<int> CompletedLevels { get; set; } = new List<int>();
    }

    public class PacketRunHighScoreEntry
    {
        public string SongId { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public long Score { get; set; }
        public double Accuracy { get; set; }
        public DateTimeOffset Date { get; set; }
    }

    public class PacketRunSaveData
    {
        public List<PacketRunProfile> Profiles { get; set; } = new List<PacketRunProfile>();
        public List<PacketRunStoryProgress> StoryProgress { get; set; } = new List<PacketRunStoryProgress>();
        public List<PacketRunHighScoreEntry> HighScores { get; set; } = new List<PacketRunHighScoreEntry>();
    }

    public class PacketRunSaveStore
    {
        private readonly Storage storage;
        private PacketRunSaveData data = new PacketRunSaveData();

        public PacketRunSaveStore(Storage storage)
        {
            this.storage = storage;
            ensureProfiles();
            load();
        }

        public IReadOnlyList<PacketRunProfile> Profiles => data.Profiles;

        public int ActiveProfileSlot { get; set; }

        public PacketRunSaveData Data => data;

        public void Save()
        {
            using var stream = storage.GetStream("packetrun/save.json", FileAccess.Write, FileMode.Create);
            using var writer = new StreamWriter(stream);
            writer.Write(JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }

        private void load()
        {
            if (!storage.Exists("packetrun/save.json"))
            {
                return;
            }

            using var stream = storage.GetStream("packetrun/save.json");
            using var reader = new StreamReader(stream);
            data = JsonSerializer.Deserialize<PacketRunSaveData>(reader.ReadToEnd()) ?? new PacketRunSaveData();
            ensureProfiles();
        }

        private void ensureProfiles()
        {
            for (int i = 0; i < 3; i++)
            {
                if (data.Profiles.All(p => p.SlotIndex != i))
                {
                    data.Profiles.Add(new PacketRunProfile { SlotIndex = i, Name = $"Runner {i + 1}" });
                }
            }
        }

        public void UpdateProfile(PacketRunProfile profile)
        {
            var existing = data.Profiles.FirstOrDefault(p => p.SlotIndex == profile.SlotIndex);

            if (existing != null)
            {
                existing.Name = profile.Name;
                existing.ActiveStoryLevel = profile.ActiveStoryLevel;
            }

            Save();
        }

        public void MarkStoryLevelComplete(int profileSlot, int level)
        {
            var progress = data.StoryProgress.FirstOrDefault(p => p.ProfileSlot == profileSlot)
                           ?? new PacketRunStoryProgress { ProfileSlot = profileSlot };

            if (!data.StoryProgress.Contains(progress))
            {
                data.StoryProgress.Add(progress);
            }

            if (!progress.CompletedLevels.Contains(level))
            {
                progress.CompletedLevels.Add(level);
            }

            Save();
        }

        public void AddHighScore(PacketRunHighScoreEntry entry)
        {
            data.HighScores.Add(entry);
            data.HighScores = data.HighScores
                                    .OrderByDescending(h => h.Score)
                                    .Take(100)
                                    .ToList();
            Save();
        }
    }
}

