// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Game.Rulesets.PacketRun.Charts;
using osu.Game.Rulesets.PacketRun.Database;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Rulesets.PacketRun.Screens
{
    public partial class PacketRunSoloPlayer : SoloPlayer
    {
        [Resolved]
        private PacketRunSaveStore saveStore { get; set; } = null!;

        protected override ResultsScreen CreateResults(ScoreInfo score)
        {
            persistLocalScore(score);
            return base.CreateResults(score);
        }

        private void persistLocalScore(ScoreInfo score)
        {
            var workingBeatmap = Beatmap.Value as PacketRunWorkingBeatmap;
            var chart = workingBeatmap?.ChartFile;

            string songId = chart?.Metadata.Title ?? score.BeatmapInfo?.Metadata.Title ?? "unknown";
            string mode = chart?.Metadata.DefaultMode ?? "queue";

            saveStore.AddHighScore(new PacketRunHighScoreEntry
            {
                SongId = songId,
                Mode = mode,
                Score = score.TotalScore,
                Accuracy = score.Accuracy,
                Date = DateTimeOffset.Now,
            });

            if (chart?.Metadata.StoryLevel != null && score.Rank != ScoreRank.F)
            {
                saveStore.MarkStoryLevelComplete(saveStore.ActiveProfileSlot, chart.Metadata.StoryLevel.Value);
            }
        }
    }
}
