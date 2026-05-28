// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.

// See the LICENCE file in the repository root for full licence text.



using System.Collections.Generic;

using System.IO;

using System.Threading;

using osu.Framework.Audio;

using osu.Framework.Audio.Track;

using osu.Framework.Graphics.Textures;

using osu.Framework.IO.Stores;

using osu.Framework.Platform;

using osu.Game.Beatmaps;

using osu.Game.Rulesets.Mods;

using osu.Game.Rulesets.PacketRun.Objects;

using osu.Game.Skinning;



namespace osu.Game.Rulesets.PacketRun.Charts

{

    public enum PacketRunAudioLoadStatus

    {

        None,

        Loaded,

        FileNotFound,

        TrackLoadFailed,

    }



    public class PacketRunWorkingBeatmap : WorkingBeatmap

    {

        private readonly PacketRunBeatmap beatmap;

        private readonly string songDirectory;

        private readonly string? chartFilePath;

        private readonly string audioFileName;

        private readonly AudioManager audio;



        private ITrackStore? songTrackStore;



        public PacketRunChartFile? ChartFile { get; }



        public bool HasRealAudio { get; private set; }



        public PacketRunAudioLoadStatus AudioLoadStatus { get; private set; }



        public string? ResolvedAudioPath { get; private set; }



        public PacketRunWorkingBeatmap(PacketRunBeatmap beatmap, string songDirectory, string audioFileName, AudioManager audioManager, string? chartFilePath = null)

            : base(beatmap.BeatmapInfo, audioManager)

        {

            this.beatmap = beatmap;

            this.songDirectory = songDirectory;

            this.chartFilePath = chartFilePath;

            this.audioFileName = audioFileName;

            audio = audioManager;

        }



        public PacketRunWorkingBeatmap(PacketRunChartFile chart, string songDirectory, AudioManager audioManager, string? chartFilePath = null)

            : this(

                PacketRunChartDecoder.Decode(System.Text.Json.JsonSerializer.Serialize(chart)),

                songDirectory,

                chart.Metadata.AudioFile,

                audioManager,

                chartFilePath)

        {

            ChartFile = chart;

        }



        protected override IBeatmap GetBeatmap() => beatmap;



        public override IBeatmap GetPlayableBeatmap(IRulesetInfo ruleset, IReadOnlyList<Mod> mods, CancellationToken token) =>
            base.GetPlayableBeatmap(ruleset, mods, token);



        public override Texture? GetBackground() => null;



        protected override Track GetBeatmapTrack()

        {

            if (PacketRunAudioPathResolver.TryResolve(audioFileName, songDirectory, chartFilePath, out string path) != PacketRunAudioResolveResult.FoundOnDisk)

            {

                ResolvedAudioPath = path;

                AudioLoadStatus = PacketRunAudioLoadStatus.FileNotFound;

                HasRealAudio = false;

                return GetVirtualTrack(120000);

            }



            ResolvedAudioPath = path;

            Track? track = loadTrackFromFile(path);



            if (track != null && !track.IsDummyDevice)

            {

                HasRealAudio = true;

                AudioLoadStatus = PacketRunAudioLoadStatus.Loaded;

                return track;

            }



            AudioLoadStatus = PacketRunAudioLoadStatus.TrackLoadFailed;

            HasRealAudio = false;

            return GetVirtualTrack(120000);

        }



        protected internal override ISkin? GetSkin() => null;



        public override Stream GetStream(string storagePath)

        {

            var storage = new NativeStorage(songDirectory);

            return storage.GetStream(storagePath);

        }



        public static double GetAudioLengthMs(string songDirectory, string audioFileName, AudioManager audioManager, string? chartFilePath = null)
        {
            var beatmap = new PacketRunBeatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata { AudioFile = audioFileName },
                },
            };

            var working = new PacketRunWorkingBeatmap(beatmap, songDirectory, audioFileName, audioManager, chartFilePath);
            working.LoadTrack();
            return working.Track.Length;
        }

        private Track? loadTrackFromFile(string path)

        {

            string? directory = Path.GetDirectoryName(path);

            string fileName = Path.GetFileName(path);



            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))

            {

                return null;

            }



            songTrackStore?.Dispose();

            songTrackStore = audio.GetTrackStore(new StorageBackedResourceStore(new NativeStorage(directory)));

            return songTrackStore.Get(fileName);

        }

    }

}


