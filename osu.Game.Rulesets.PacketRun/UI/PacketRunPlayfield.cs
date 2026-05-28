// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.PacketRun;
using osu.Game.Rulesets.PacketRun.BeatGrid;
using osu.Game.Rulesets.PacketRun.UI.Components;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.UI
{
    [Cached]
    public partial class PacketRunPlayfield : ScrollingPlayfield, IKeyBindingHandler<PacketRunAction>
    {
        public const float HitLineX = PacketRunRhythmLayout.HitLineX;

        [Resolved]
        private PacketGameplayProcessor processor { get; set; } = null!;

        private readonly JudgementContainer<DrawablePacketRunJudgement> judgementLayer;
        private readonly JudgementPooler<DrawablePacketRunJudgement> judgementPooler;
        private readonly Container judgementAboveHitObjectLayer;

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

        public PacketRunPlayfield()
        {
            InternalChildren = new Drawable[]
            {
                HitObjectContainer,
                judgementLayer = new JudgementContainer<DrawablePacketRunJudgement> { RelativeSizeAxes = Axes.Both },
                judgementAboveHitObjectLayer = new Container { RelativeSizeAxes = Axes.Both },
            };

            AddInternal(judgementPooler = new JudgementPooler<DrawablePacketRunJudgement>(new[]
            {
                HitResult.Perfect,
                HitResult.Great,
                HitResult.Good,
                HitResult.Ok,
                HitResult.Meh,
                HitResult.Miss,
            }, onJudgementLoaded));

            NewResult += onNewResult;
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

            AddInternal(new GridBackground { Depth = 2 });

            AddInternal(new RhythmBeatGridOverlay(new PacketRunBeatGrid(processor.Beatmap))
            {
                RelativeSizeAxes = Axes.Both,
                Depth = 1,
            });

            AddInternal(new HitLine(DrawablePacket.DIGIT_SIZE)
            {
                Origin = Anchor.Centre,
                RelativePositionAxes = Axes.Both,
                X = HitLineX,
                Y = 0.5f,
                Depth = 0,
            });
        }

        private void onJudgementLoaded(DrawablePacketRunJudgement judgement)
        {
            judgementAboveHitObjectLayer.Add(judgement.ProxiedAboveHitObjectsContent);
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
            {
                return;
            }

            var explosion = judgementPooler.Get(result.Type, doj => doj.Apply(result, judgedObject));

            if (explosion == null)
            {
                return;
            }

            judgementLayer.Add(explosion);
            judgementAboveHitObjectLayer.ChangeChildDepth(explosion.ProxiedAboveHitObjectsContent, (float)-result.TimeAbsolute);
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
        private const float border_thickness = 2f;

        public HitLine(float width)
        {
            Width = width;
            RelativeSizeAxes = Axes.Y;
            Height = 1f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var colour = new Color4(255, 80, 180, 255);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour,
                    Alpha = 0.12f,
                },
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = border_thickness,
                    Colour = colour,
                },
                new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = border_thickness,
                    Colour = colour,
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = border_thickness,
                    Colour = colour,
                },
                new Box
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = border_thickness,
                    Colour = colour,
                },
            };
        }
    }
}
