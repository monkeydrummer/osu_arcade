// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Rulesets.PacketRun.Objects.Drawables;
using osu.Game.Rulesets.PacketRun.UI.Components;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.PacketRun.UI
{
    public partial class DrawablePacketRunJudgement : DrawableJudgement
    {
        private PacketGameplayMode mode = PacketGameplayMode.Queue;
        private Vector2 relativePosition = new Vector2(PacketRunRhythmLayout.HitLineX, 0.5f);

        [Resolved]
        private PacketGameplayProcessor processor { get; set; } = null!;

        public DrawablePacketRunJudgement()
        {
            Anchor = Anchor.TopLeft;
            Origin = Anchor.Centre;
        }

        public override void Apply(JudgementResult result, DrawableHitObject? judgedObject)
        {
            base.Apply(result, judgedObject);
            relativePosition = computeRelativePosition(judgedObject);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Alpha = 1;
            RelativePositionAxes = Axes.Both;
            Position = relativePosition;

            if (JudgementBody?.Drawable is PacketRunJudgementPiece piece)
            {
                piece.Mode = mode;
                piece.UpdateDisplayText();
            }
        }

        protected override void ApplyHitAnimations()
        {
            this.ScaleTo(0.6f).ScaleTo(1f, 200, Easing.Out);
        }

        protected override Drawable CreateDefaultJudgement(HitResult result) => new PacketRunJudgementPiece(result);

        private Vector2 computeRelativePosition(DrawableHitObject? judgedObject)
        {
            if (judgedObject is DrawablePacketHitObject packetObject)
            {
                mode = packetObject.HitObject.ModeOverride
                       ?? processor?.Beatmap.GetModeAt(packetObject.HitObject.StartTime)
                       ?? PacketGameplayMode.Queue;

                if (mode == PacketGameplayMode.Rhythm)
                {
                    return new Vector2(PacketRunRhythmLayout.HitLineX, 0.5f);
                }

                if (Parent != null && Parent.DrawWidth > 0 && Parent.DrawHeight > 0)
                {
                    return new Vector2(
                        packetObject.X / Parent.DrawWidth,
                        packetObject.Y / Parent.DrawHeight);
                }
            }
            else
            {
                mode = processor?.CurrentMode.Value ?? PacketGameplayMode.Queue;
            }

            return new Vector2(PacketRunRhythmLayout.HitLineX, 0.5f);
        }
    }
}
