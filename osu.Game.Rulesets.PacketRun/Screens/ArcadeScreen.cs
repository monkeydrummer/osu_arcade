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
    public partial class ArcadeScreen : PacketRunScreen
    {
        public override string Title => "Arcade";

        private PacketRunSongLibrary songLibrary = null!;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, PacketRunSaveStore saveStore)
        {
            songLibrary = new PacketRunSongLibrary();
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
                Text = "Arcade",
                Font = OsuFont.Torus.With(size: 36, weight: FontWeight.Bold),
                Colour = new Color4(255, 160, 60, 255),
            });

            foreach (var song in songLibrary.Songs.Where(s => s.Chart?.Metadata.StoryLevel == null))
            {
                var captured = song;
                flow.Add(new PacketRunMenuButton($"{captured.Title} — {captured.Artist}", () =>
                {
                    var chart = PacketRunChartDecoder.DecodeFile(captured.ChartPath!);
                    var working = new PacketRunWorkingBeatmap(chart, captured.Directory, audio);
                    StartGameplay(working);
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
}
