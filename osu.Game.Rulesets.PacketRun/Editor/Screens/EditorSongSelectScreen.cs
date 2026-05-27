// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.PacketRun.Charts;
using osu.Game.Rulesets.PacketRun.Screens.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Editor.Screens
{
    public partial class EditorSongSelectScreen : PacketRunEditorScreen
    {
        public override string Title => "Select Song";

        private PacketRunEditorSongLibrary library = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            library = new PacketRunEditorSongLibrary();
            library.Scan(GetContentRoot());

            var flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(40),
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 12),
            };

            flow.Add(new OsuSpriteText
            {
                Text = "Select Song",
                Font = OsuFont.Torus.With(size: 32, weight: FontWeight.Bold),
                Colour = new Color4(80, 220, 120, 255),
            });

            foreach (var song in library.Songs)
            {
                var captured = song;
                string badge = captured.HasChart ? "[chart]" : "[audio only]";
                flow.Add(new PacketRunMenuButton($"{badge} {captured.Title} — {captured.Artist}", () => openSong(captured))
                {
                    Size = new Vector2(520, 40),
                });
            }

            flow.Add(new PacketRunMenuButton("Back", () => this.Exit()) { Size = new Vector2(120, 40), Margin = new MarginPadding { Top = 20 } });

            InternalChildren = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(6, 6, 14, 255) },
                flow,
            };
        }

        private void openSong(PacketRunEditorSongEntry song)
        {
            var document = new PacketRunChartDocument();

            if (song.HasChart && song.ChartPath != null)
            {
                document.Load(song.ChartPath);
            }
            else
            {
                string audioFile = Path.GetFileName(song.AudioPath!);
                document.NewBlank(song.Directory, audioFile, song.Title, song.Artist);
            }

            Session.Document = document;
            this.Push(new ChartSetupScreen());
        }
    }
}
