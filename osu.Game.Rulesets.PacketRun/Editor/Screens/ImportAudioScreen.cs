// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.PacketRun.Screens.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Editor.Screens
{
    public partial class ImportAudioScreen : PacketRunEditorScreen
    {
        public override string Title => "Import Audio";

        [BackgroundDependencyLoader]
        private void load()
        {
            var selector = new OsuFileSelector(validFileExtensions: new[] { ".ogg", ".mp3", ".wav", ".flac" })
            {
                RelativeSizeAxes = Axes.Both,
            };

            selector.CurrentFile.ValueChanged += e =>
            {
                if (e.NewValue == null)
                {
                    return;
                }

                Session.Document = PacketRunChartDocument.FromImport(e.NewValue.FullName, GetContentRoot());
                Schedule(() => this.Push(new ChartSetupScreen()));
            };

            InternalChildren = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(6, 6, 14, 255) },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(40),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 12),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "Select audio file to import",
                                    Font = OsuFont.Torus.With(size: 28, weight: FontWeight.Bold),
                                    Colour = Color4.White,
                                },
                                selector,
                                new PacketRunMenuButton("Back", () => this.Exit()) { Size = new Vector2(120, 40) },
                            },
                        },
                    },
                },
            };
        }
    }
}
