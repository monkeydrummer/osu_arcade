// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
        private static readonly Color4 unselectedColour = new(20, 20, 40, 255);
        private static readonly Color4 selectedColour = new(80, 200, 255, 200);
        private static readonly Color4 hoverColour = new(255, 80, 180, 120);

        private readonly Action onClick;
        private Box background = null!;
        private OsuSpriteText label = null!;
        private bool selected;
        private bool hovered;

        public bool Selected
        {
            get => selected;
            set
            {
                if (selected == value)
                {
                    return;
                }

                selected = value;
                refreshBackground();
            }
        }

        public PacketRunMenuButton(string text, Action onClick)
        {
            this.onClick = onClick;
            Size = new Vector2(320, 48);

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = unselectedColour,
                },
                label = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = text,
                    Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold),
                    Colour = Color4.White,
                },
            };
        }

        public void BindSelected(IBindable<bool> bindable)
        {
            bindable.BindValueChanged(e => Selected = e.NewValue, true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hovered = true;
            refreshBackground();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hovered = false;
            refreshBackground();
        }

        protected override bool OnClick(ClickEvent e)
        {
            onClick();
            return true;
        }

        private void refreshBackground()
        {
            if (hovered)
            {
                background.Colour = hoverColour;
                label.Colour = Color4.White;
                return;
            }

            background.Colour = selected ? selectedColour : unselectedColour;
            label.Colour = selected ? Color4.White : new Color4(180, 180, 200, 255);
        }
    }
}
