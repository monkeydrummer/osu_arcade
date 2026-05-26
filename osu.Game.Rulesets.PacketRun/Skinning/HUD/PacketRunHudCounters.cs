// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.PacketRun.Scoring;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Skinning.HUD
{
    public partial class SignalCounter : PacketRunHudCounter
    {
        public SignalCounter(PacketRunScoreProcessor processor)
            : base("SIGNAL")
        {
            processor.SignalBindable.BindValueChanged(v => ValueText.Text = v.NewValue.ToString(), true);
        }
    }

    public partial class HeatCounter : PacketRunHudCounter
    {
        public HeatCounter(PacketRunScoreProcessor processor)
            : base("HEAT")
        {
            processor.HeatBindable.BindValueChanged(v => ValueText.Text = v.NewValue.ToString(), true);
        }
    }

    public partial class SyncCounter : PacketRunHudCounter
    {
        public SyncCounter(ScoreProcessor processor)
            : base("SYNC")
        {
            processor.Accuracy.BindValueChanged(v => ValueText.Text = $"{v.NewValue:P1}", true);
        }
    }

    public partial class PacketScoreCounter : PacketRunHudCounter
    {
        public PacketScoreCounter(ScoreProcessor processor)
            : base("SCORE")
        {
            processor.TotalScore.BindValueChanged(v => ValueText.Text = v.NewValue.ToString("N0"), true);
        }
    }

    public abstract partial class PacketRunHudCounter : CompositeDrawable
    {
        protected OsuSpriteText ValueText { get; private set; } = null!;

        protected PacketRunHudCounter(string label)
        {
            AutoSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = label,
                    Font = OsuFont.Torus.With(size: 14, weight: FontWeight.Bold),
                    Colour = new Color4(80, 200, 255, 255),
                },
                ValueText = new OsuSpriteText
                {
                    Y = 18,
                    Font = OsuFont.Torus.With(size: 22, weight: FontWeight.Bold),
                    Colour = Color4.White,
                },
            };
        }
    }
}
