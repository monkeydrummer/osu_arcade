// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.

// See the LICENCE file in the repository root for full licence text.



using osu.Framework.Allocation;

using osu.Framework.Audio;

using osu.Framework.Graphics;

using osu.Framework.Graphics.Containers;

using osu.Game.Rulesets.PacketRun.Charts;

using osu.Game.Rulesets.PacketRun.Editor;

using osu.Game.Screens.Edit;



namespace osu.Game.Rulesets.PacketRun.Editor.Components

{

    // Hosts chart audio playback at screen level so the editor clock keeps updating.

    public partial class EditorPlaybackHost : CompositeComponent

    {

        public EditorClock EditorClock { get; private set; } = null!;



        public PacketRunWorkingBeatmap WorkingBeatmap { get; private set; } = null!;



        public bool HasRealAudio => WorkingBeatmap?.HasRealAudio == true;



        private readonly PacketRunChartDocument document;



        public EditorPlaybackHost(PacketRunChartDocument document)

        {

            this.document = document;

        }



        [BackgroundDependencyLoader]

        private void load(AudioManager audio)

        {

            WorkingBeatmap = new PacketRunWorkingBeatmap(document.Chart, document.SongDirectory, audio, document.FilePath);

            WorkingBeatmap.LoadTrack();



            EditorClock = new EditorClock(WorkingBeatmap.Beatmap);

            AddInternal(EditorClock);

            EditorClock.ChangeSource(WorkingBeatmap.Track);

        }



        protected override void LoadComplete()

        {

            base.LoadComplete();

            waitForTrackLoad();

        }



        private void waitForTrackLoad()

        {

            if (!WorkingBeatmap.Track.IsLoaded)

            {

                Scheduler.AddDelayed(waitForTrackLoad, 50);

                return;

            }



            EditorClock.ChangeSource(WorkingBeatmap.Track);

        }



        public void StopPlayback()

        {

            EditorClock.Stop();



            if (WorkingBeatmap.TrackLoaded)

            {

                WorkingBeatmap.Track.Stop();

            }

        }



        protected override void Dispose(bool isDisposing)

        {

            if (isDisposing)

            {

                StopPlayback();

            }



            base.Dispose(isDisposing);

        }

    }

}


