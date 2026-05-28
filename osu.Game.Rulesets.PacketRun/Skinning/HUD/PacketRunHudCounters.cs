// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.PacketRun.Scoring;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Skinning.HUD
{
    public partial class SignalCounter : PacketRunHudCounter
    {
        public SignalCounter(PacketRunScoreProcessor processor)
            : base("SIGNAL")
        {
            BindProcessor(processor.SignalBindable, v =>
            {
                SetValueText(v.NewValue.ToString());

                bool wasIncrease = v.NewValue > v.OldValue;
                bool wasReset = v.OldValue > 0 && v.NewValue == 0;

                if (wasReset)
                {
                    PulseValueContainer(0.7f, 2000, Color4.Red);
                }
                else if (wasIncrease)
                {
                    PulseValueContainer(1.25f, 500);

                    if (v.NewValue is 10 or 25 or 50)
                    {
                        ValueContainer.FlashColour(new Color4(80, 200, 255, 255), 400, Easing.OutQuint);
                    }
                }
            });
        }
    }

    public partial class HeatCounter : PacketRunHudCounter
    {
        public HeatCounter(PacketRunScoreProcessor processor)
            : base("HEAT")
        {
            BindProcessor(processor.HeatBindable, v =>
            {
                SetValueText(v.NewValue.ToString());

                if (v.NewValue > v.OldValue)
                {
                    PulseValueContainer(1.3f, 400, new Color4(255, 120, 60, 255));
                }
            });
        }
    }

    public partial class SyncCounter : PacketRunHudCounter
    {
        public SyncCounter(ScoreProcessor processor)
            : base("SYNC")
        {
            BindProcessor(processor.Accuracy, v =>
            {
                SetValueText($"{v.NewValue:P1}");

                if (Math.Abs(v.NewValue - v.OldValue) > 0.0001)
                {
                    PulseValueContainer(1.1f, 300);
                }
            });
        }
    }

    public partial class PacketScoreCounter : PacketRunHudCounter
    {
        private long displayedScore;

        public long DisplayedScore
        {
            get => displayedScore;
            set
            {
                displayedScore = value;
                SetValueText(displayedScore.ToString("N0"));
            }
        }

        public PacketScoreCounter(ScoreProcessor processor)
            : base("SCORE")
        {
            BindProcessor(processor.TotalScore, v =>
            {
                if (IsLoaded)
                {
                    this.TransformTo(nameof(DisplayedScore), v.NewValue, 250, Easing.OutQuint);
                    PulseValueContainer(1.08f, 250);
                }
                else
                {
                    DisplayedScore = v.NewValue;
                }
            });
        }
    }

    public abstract partial class PacketRunHudCounter : CompositeDrawable
    {
        protected Container ValueContainer { get; private set; } = null!;

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
                ValueContainer = new Container
                {
                    Y = 18,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Child = ValueText = new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Font = OsuFont.Torus.With(size: 24, weight: FontWeight.Bold),
                        Colour = Color4.White,
                    },
                },
            };
        }

        protected void BindProcessor<T>(Bindable<T> bindable, Action<ValueChangedEvent<T>> onChange) =>
            bindable.BindValueChanged(onChange, true);

        protected void SetValueText(string text) => ValueText.Text = text;

        protected void PulseValueContainer(float peakScale, double duration, Color4? flashColour = null)
        {
            if (!IsLoaded)
            {
                return;
            }

            ValueContainer.ClearTransforms(true);
            ValueContainer.ScaleTo(peakScale).ScaleTo(1, duration, Easing.OutQuint);

            if (flashColour != null)
            {
                ValueContainer.FlashColour(flashColour.Value, duration, Easing.OutQuint);
            }
        }
    }
}
