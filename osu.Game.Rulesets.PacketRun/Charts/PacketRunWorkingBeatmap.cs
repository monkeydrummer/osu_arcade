// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.PacketRun.Charts
{
    public class PacketRunWorkingBeatmap : WorkingBeatmap
    {
        private readonly PacketRunBeatmap beatmap;
        private readonly string songDirectory;
        private readonly string audioFileName;
        private readonly AudioManager audio;

        public PacketRunChartFile? ChartFile { get; }

        public PacketRunWorkingBeatmap(PacketRunBeatmap beatmap, string songDirectory, string audioFileName, AudioManager audioManager)
            : base(beatmap.BeatmapInfo, audioManager)
        {
            this.beatmap = beatmap;
            this.songDirectory = songDirectory;
            this.audioFileName = audioFileName;
            audio = audioManager;
        }

        public PacketRunWorkingBeatmap(PacketRunChartFile chart, string songDirectory, AudioManager audioManager)
            : this(
                PacketRunChartDecoder.Decode(System.Text.Json.JsonSerializer.Serialize(chart)),
                songDirectory,
                chart.Metadata.AudioFile,
                audioManager)
        {
            ChartFile = chart;
        }

        protected override IBeatmap GetBeatmap() => beatmap;

        public override Texture? GetBackground() => null;

        protected override Track GetBeatmapTrack()
        {
            string path = Path.Combine(songDirectory, audioFileName);

            if (!File.Exists(path))
            {
                return GetVirtualTrack(120000);
            }

            return audio.Tracks.Get(path) ?? GetVirtualTrack(120000);
        }

        protected internal override ISkin? GetSkin() => null;

        public override Stream GetStream(string storagePath) => File.OpenRead(Path.Combine(songDirectory, storagePath));
    }
}
