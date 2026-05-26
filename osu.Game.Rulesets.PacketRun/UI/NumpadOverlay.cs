// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Extensions;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.PacketRun.UI.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.UI
{
    public partial class NumpadOverlay : CompositeDrawable
    {
        public NumpadOverlay()
        {
            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;
            Margin = new MarginPadding(24);
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new GridContainer
            {
                AutoSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize),
                },
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize),
                },
                Content = new[]
                {
                    new[] { createKey(7), createKey(8), createKey(9) },
                    new[] { createKey(4), createKey(5), createKey(6) },
                    new[] { createKey(1), createKey(2), createKey(3) },
                    new[] { createKey(0), Empty(), Empty() },
                },
            };
        }

        private Drawable createKey(int digit)
        {
            var colour = PacketRunColours.ForDigit(digit);

            return new Container
            {
                Size = new Vector2(44),
                Margin = new MarginPadding(3),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colour.Darken(0.5f),
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = digit.ToString(),
                        Font = OsuFont.Torus.With(size: 18, weight: FontWeight.Bold),
                        Colour = Color4.White,
                    },
                },
            };
        }
    }
}
