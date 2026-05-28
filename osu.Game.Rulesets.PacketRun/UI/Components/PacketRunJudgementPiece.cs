// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.UI.Components
{
    public partial class PacketRunJudgementPiece : DefaultJudgementPiece
    {
        public PacketGameplayMode Mode { get; set; } = PacketGameplayMode.Queue;

        public PacketRunJudgementPiece(HitResult result)
            : base(result)
        {
            RelativePositionAxes = Axes.Both;
        }

        protected override SpriteText CreateJudgementText() =>
            new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Torus.With(size: 28, weight: FontWeight.Bold),
            };

        public void UpdateDisplayText()
        {
            JudgementText.Text = getDisplayText(Result, Mode);
        }

        public override void PlayAnimation()
        {
            UpdateDisplayText();

            if (Result.IsMiss())
            {
                this.ScaleTo(1.4f).ScaleTo(1, 120, Easing.In);
                this.MoveToY(-0.02f).MoveToY(0.06f, 600, Easing.InQuint);
                this.FadeOutFromOne(800);
                return;
            }

            JudgementText
                .ScaleTo(0.8f)
                .ScaleTo(1.2f, 400, Easing.OutElastic);

            this.MoveToY(-0.04f).MoveToY(-0.1f, 400, Easing.OutQuint);
            this.FadeOutFromOne(800, Easing.OutQuint);
        }

        private static string getDisplayText(HitResult result, PacketGameplayMode mode)
        {
            if (mode == PacketGameplayMode.Queue)
            {
                return result switch
                {
                    HitResult.Great => "CLEAN",
                    HitResult.Ok => "OK",
                    HitResult.Miss => "MISS",
                    _ => result.GetDescription().ToUpperInvariant(),
                };
            }

            return result.GetDescription().ToUpperInvariant();
        }
    }
}
