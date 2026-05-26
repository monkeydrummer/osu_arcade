// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.PacketRun.Charts;
using osu.Game.Rulesets.PacketRun.Database;
using osu.Game.Rulesets.PacketRun.Screens.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Screens
{
    public partial class StoryModeScreen : PacketRunScreen
    {
        public override string Title => "Story Mode";

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, PacketRunSaveStore saveStore)
        {
            var songLibrary = new PacketRunSongLibrary();
            songLibrary.Scan(GetContentRoot());

            var flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(40),
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 12),
            };

            flow.Add(new OsuSpriteText
            {
                Text = "Story Mode — Select Profile",
                Font = OsuFont.Torus.With(size: 32, weight: FontWeight.Bold),
                Colour = new Color4(255, 80, 180, 255),
            });

            foreach (var profile in saveStore.Profiles.OrderBy(p => p.SlotIndex))
            {
                var captured = profile;
                flow.Add(new PacketRunMenuButton($"Slot {captured.SlotIndex + 1}: {captured.Name}", () =>
                {
                    saveStore.ActiveProfileSlot = captured.SlotIndex;
                    this.Push(new StoryLevelSelectScreen(captured, songLibrary, audio, saveStore));
                })
                {
                    Size = new Vector2(420, 40),
                });
            }

            InternalChildren = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(6, 6, 14, 255) },
                flow,
            };
        }
    }

    public partial class StoryLevelSelectScreen : PacketRunScreen
    {
        private readonly PacketRunProfile profile;
        private readonly PacketRunSongLibrary songLibrary;
        private readonly AudioManager audio;
        private readonly PacketRunSaveStore saveStore;

        public StoryLevelSelectScreen(PacketRunProfile profile, PacketRunSongLibrary songLibrary, AudioManager audio, PacketRunSaveStore saveStore)
        {
            this.profile = profile;
            this.songLibrary = songLibrary;
            this.audio = audio;
            this.saveStore = saveStore;
        }

        public override string Title => "Story Levels";

        [BackgroundDependencyLoader]
        private void load()
        {
            var storySongs = songLibrary.Songs.Where(s => s.Chart?.Metadata.StoryLevel != null).ToList();

            var flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(40),
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 12),
            };

            flow.Add(new OsuSpriteText
            {
                Text = $"Runner: {profile.Name}",
                Font = OsuFont.Torus.With(size: 28, weight: FontWeight.Bold),
                Colour = Color4.White,
            });

            foreach (var song in storySongs)
            {
                var captured = song;
                int level = captured.Chart!.Metadata.StoryLevel!.Value;
                flow.Add(new PacketRunMenuButton($"Level {level}: {captured.Title}", () =>
                {
                    this.Push(new StoryInterstitialScreen(profile, captured, audio, saveStore));
                })
                {
                    Size = new Vector2(420, 40),
                });
            }

            InternalChildren = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(6, 6, 14, 255) },
                flow,
            };
        }
    }

    public partial class StoryInterstitialScreen : PacketRunScreen
    {
        private readonly PacketRunProfile profile;
        private readonly PacketRunSongEntry song;
        private readonly AudioManager audio;
        private readonly PacketRunSaveStore saveStore;

        public StoryInterstitialScreen(PacketRunProfile profile, PacketRunSongEntry song, AudioManager audio, PacketRunSaveStore saveStore)
        {
            this.profile = profile;
            this.song = song;
            this.audio = audio;
            this.saveStore = saveStore;
        }

        public override string Title => "Story";

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(6, 6, 14, 255) },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 20),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "INCOMING TRANSMISSION",
                            Font = OsuFont.Torus.With(size: 24, weight: FontWeight.Bold),
                            Colour = new Color4(80, 200, 255, 255),
                        },
                        new OsuSpriteText
                        {
                            Text = "Clear the packet queue to sync with the node.",
                            Font = OsuFont.Torus.With(size: 18),
                            Colour = Color4.White,
                        },
                        new PacketRunMenuButton("Begin", () =>
                        {
                            var chart = PacketRunChartDecoder.DecodeFile(song.ChartPath!);
                            var working = new PacketRunWorkingBeatmap(chart, song.Directory, audio);
                            profile.ActiveStoryLevel = song.Chart!.Metadata.StoryLevel;
                            saveStore.UpdateProfile(profile);
                            StartGameplay(working);
                        }),
                    },
                },
            };
        }
    }
}
