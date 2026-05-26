// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.PacketRun;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.UI
{
    [Cached]
    public partial class PacketRunPlayfield : ScrollingPlayfield, IKeyBindingHandler<PacketRunAction>
    {
        [Resolved]
        private PacketGameplayProcessor processor { get; set; } = null!;

        protected override ScrollingHitObjectContainer CreateScrollingHitObjectContainer() => new PacketRunHitObjectContainer();

        public bool OnPressed(KeyBindingPressEvent<PacketRunAction> e)
        {
            int digit = (int)e.Action;

            if (digit is < 0 or > 9)
            {
                return false;
            }

            processor.HandleDigitInput(digit, Time.Current);
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<PacketRunAction> e)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = new Color4(8, 8, 16, 255),
                Depth = float.MaxValue,
            });

            AddInternal(new GridBackground { Depth = 1 });

            AddInternal(new HitLine
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativePositionAxes = Axes.Both,
                X = 0.12f,
                Depth = 0,
            });

            AddRangeInternal(new Drawable[]
            {
                HitObjectContainer,
            });
        }
    }

    public partial class GridBackground : CompositeDrawable
    {
        public GridBackground()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const int lines = 20;
            var children = new Drawable[lines + 1];

            for (int i = 0; i <= lines; i++)
            {
                children[i] = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Colour = Color4.White,
                    Alpha = 0.03f,
                    Y = i * (512f / lines),
                };
            }

            InternalChildren = children;
        }
    }

    public partial class HitLine : CompositeDrawable
    {
        public HitLine()
        {
            Width = 3;
            RelativeSizeAxes = Axes.Y;
            Height = 0.6f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = new Color4(255, 80, 180, 255),
            };
        }
    }
}
