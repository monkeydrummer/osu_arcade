// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.PacketRun.Scoring
{
    public partial class PacketRunScoreProcessor : ScoreProcessor
    {
        public int Heat { get; private set; }

        public int Signal { get; private set; }

        public PacketRunScoreProcessor()
            : base(new PacketRunRuleset())
        {
        }

        protected override double ComputeTotalScore(double comboProgress, double accuracyProgress, double bonusPortion)
        {
            return 150000 * comboProgress
                   + 850000 * Math.Pow(Accuracy.Value, 2 + 2 * Accuracy.Value) * accuracyProgress
                   + bonusPortion;
        }

        public BindableInt SignalBindable { get; } = new BindableInt();

        public BindableInt HeatBindable { get; } = new BindableInt();

        public void RegisterHeat()
        {
            Heat++;
            HeatBindable.Value = Heat;
        }

        public void RegisterSignal(bool increment)
        {
            if (increment)
            {
                Signal++;
            }
            else
            {
                Signal = 0;
            }

            SignalBindable.Value = Signal;
        }

        public override int GetBaseScoreForResult(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                    return 300;

                case HitResult.Great:
                    return 200;

                case HitResult.Good:
                    return 100;

                case HitResult.Ok:
                    return 50;

                case HitResult.Meh:
                    return 25;
            }

            return base.GetBaseScoreForResult(result);
        }
    }
}
