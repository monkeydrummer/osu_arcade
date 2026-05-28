// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.PacketRun.Charts;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Rulesets.PacketRun.Packets;
using osu.Game.Rulesets.PacketRun.Screens.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Screens
{
    public partial class RandomArcadeScreen : PacketRunScreen
    {
        public override string Title => "Random Arcade";

        private readonly Bindable<PacketRunSongEntry?> selectedSong = new Bindable<PacketRunSongEntry?>();
        private readonly Bindable<PacketModFlags> selectedMods = new Bindable<PacketModFlags>(PacketModFlags.Descending | PacketModFlags.Ascending);
        private readonly Bindable<PacketGameplayMode> selectedMode = new Bindable<PacketGameplayMode>(PacketGameplayMode.Rhythm);

        private PacketRunSongLibrary songLibrary = null!;
        private FillFlowContainer modContainer = null!;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            songLibrary = new PacketRunSongLibrary();
            songLibrary.Scan(GetContentRoot());

            if (songLibrary.Songs.Count > 0)
            {
                selectedSong.Value = songLibrary.Songs[0];
            }

            InternalChildren = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(6, 6, 14, 255) },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(40),
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 12),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "Random Arcade",
                            Font = OsuFont.Torus.With(size: 36, weight: FontWeight.Bold),
                            Colour = new Color4(80, 200, 255, 255),
                        },
                        createSongSelector(),
                        modContainer = createModToggles(),
                        new PacketRunMenuButton("Start", () => startRandom(audio)),
                    },
                },
            };
        }

        private Drawable createSongSelector()
        {
            var flow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 6),
            };

            foreach (var song in songLibrary.Songs)
            {
                var captured = song;
                var button = new PacketRunMenuButton($"{captured.Title} — {captured.Artist}", () => selectedSong.Value = captured)
                {
                    Size = new Vector2(420, 40),
                };

                selectedSong.BindValueChanged(e => button.Selected = e.NewValue == captured, true);
                flow.Add(button);
            }

            return flow;
        }

        private FillFlowContainer createModToggles()
        {
            var flow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 6),
            };

            addModToggle(flow, "Descending", PacketModFlags.Descending);
            addModToggle(flow, "Ascending", PacketModFlags.Ascending);
            addModToggle(flow, "Oreo", PacketModFlags.Oreo);
            addModToggle(flow, "Repeat", PacketModFlags.Repeat);
            addModToggle(flow, "Diagonals", PacketModFlags.Diagonals);
            addModToggle(flow, "Columns", PacketModFlags.Columns);
            addModToggle(flow, "PrePost0", PacketModFlags.PrePost0);
            addModToggle(flow, "Rhythm Mode", PacketGameplayMode.Rhythm, isMode: true);

            return flow;
        }

        private void addModToggle(FillFlowContainer flow, string label, PacketModFlags flag)
        {
            var button = new PacketRunMenuButton(label, () =>
            {
                if ((selectedMods.Value & flag) != 0)
                {
                    selectedMods.Value &= ~flag;
                }
                else
                {
                    selectedMods.Value |= flag;
                }
            })
            {
                Size = new Vector2(420, 36),
            };

            selectedMods.BindValueChanged(e => button.Selected = (e.NewValue & flag) != 0, true);
            flow.Add(button);
        }

        private void addModToggle(FillFlowContainer flow, string label, PacketGameplayMode mode, bool isMode)
        {
            var button = new PacketRunMenuButton(label, () =>
            {
                selectedMode.Value = selectedMode.Value == mode ? PacketGameplayMode.Queue : mode;
            })
            {
                Size = new Vector2(420, 36),
            };

            selectedMode.BindValueChanged(e => button.Selected = e.NewValue == mode, true);
            flow.Add(button);
        }

        private void startRandom(AudioManager audio)
        {
            if (selectedSong.Value == null)
            {
                return;
            }

            var chart = selectedSong.Value.Chart!;
            var generator = new PacketRunBeatmapGenerator();
            var metadata = new BeatmapMetadata
            {
                Title = chart.Metadata.Title,
                Artist = chart.Metadata.Artist,
                AudioFile = chart.Metadata.AudioFile,
            };

            var timing = chart.Timing.Select(t => (t.TimeMs, t.Bpm));
            int seed = System.Environment.TickCount;

            double songLengthMs = PacketRunWorkingBeatmap.GetAudioLengthMs(
                selectedSong.Value.Directory,
                chart.Metadata.AudioFile,
                audio,
                selectedSong.Value.ChartPath);

            int packetCount = PacketRunBeatmapGenerator.CalculatePacketCountForSongLength(songLengthMs, timing, selectedMode.Value);

            var beatmap = generator.Generate(metadata, timing, selectedMods.Value, selectedMode.Value, packetCount: packetCount, seed: seed);

            var working = new PacketRunWorkingBeatmap(beatmap, selectedSong.Value.Directory, chart.Metadata.AudioFile, audio);
            StartGameplay(working);
        }
    }
}
