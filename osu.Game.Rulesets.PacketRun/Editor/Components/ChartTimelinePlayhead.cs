// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Editor.Components
{
    public partial class ChartTimelinePlayhead : CompositeDrawable
    {
        public const float HIT_WIDTH = 12f;

        public ChartTimelinePlayhead()
        {
            Width = HIT_WIDTH;
            Origin = Anchor.TopCentre;
            Anchor = Anchor.TopLeft;
            AlwaysPresent = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
                new Box
                {
                    Width = 2,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Colour = Color4.Red,
                    RelativeSizeAxes = Axes.Y,
                },
            };
        }
    }
}
