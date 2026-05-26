// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Screens.Components
{
    public partial class PacketRunMenuButton : CompositeDrawable
    {
        private readonly Action onClick;
        private Box background = null!;

        public PacketRunMenuButton(string text, Action onClick)
        {
            this.onClick = onClick;
            Size = new Vector2(320, 48);

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(20, 20, 40, 255),
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = text,
                    Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold),
                    Colour = Color4.White,
                },
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(new Color4(255, 80, 180, 120), 150);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(new Color4(20, 20, 40, 255), 150);
        }

        protected override bool OnClick(ClickEvent e)
        {
            onClick();
            return true;
        }
    }
}
