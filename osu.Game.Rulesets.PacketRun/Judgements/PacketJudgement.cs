// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.PacketRun.Judgements
{
    public class PacketJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Perfect;

        public override HitResult MinResult => HitResult.Miss;

        protected override double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                    return -0.008;

                case HitResult.Meh:
                case HitResult.Ok:
                    return 0.008;

                case HitResult.Good:
                    return 0.014;

                case HitResult.Great:
                case HitResult.Perfect:
                    return 0.018;

                default:
                    return base.HealthIncreaseFor(result);
            }
        }
    }
}
