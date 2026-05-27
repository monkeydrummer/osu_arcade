// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Rulesets.PacketRun.Screens.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Editor.Screens
{
    public partial class EditorMainMenu : PacketRunEditorScreen
    {
        public override string Title => "Packet Run Editor";

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(6, 6, 14, 255),
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 16),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "PACKET RUN EDITOR",
                            Font = OsuFont.Torus.With(size: 42, weight: FontWeight.Bold),
                            Colour = new Color4(80, 220, 120, 255),
                            Margin = new MarginPadding { Bottom = 32 },
                        },
                        new PacketRunMenuButton("Import Audio (New Chart)", () => this.Push(new ImportAudioScreen())),
                        new PacketRunMenuButton("Open Song", () => this.Push(new EditorSongSelectScreen())),
                        new PacketRunMenuButton("Continue", continueEditing),
                    },
                },
            };
        }

        private void continueEditing()
        {
            if (Session.Document != null)
            {
                this.Push(new ChartSetupScreen());
            }
        }
    }
}
