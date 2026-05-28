// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.PacketRun.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.PacketRun.Skinning.HUD
{
    public partial class PacketRunHudLayout
    {
        public const float EdgeMargin = 24f;
        public const float CounterSpacing = 10f;
    }

    public partial class PacketRunHudOverlay : Container, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            RelativeSizeAxes = Axes.Both;
            UsesFixedAnchor = true;

            var leftColumn = new FillFlowContainer
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, PacketRunHudLayout.CounterSpacing),
                Margin = new MarginPadding(PacketRunHudLayout.EdgeMargin),
            };

            var packetProcessor = scoreProcessor as PacketRunScoreProcessor;

            if (packetProcessor != null)
            {
                leftColumn.Add(new SignalCounter(packetProcessor));
                leftColumn.Add(new HeatCounter(packetProcessor));
            }

            leftColumn.Add(new SyncCounter(scoreProcessor));

            InternalChildren = new Drawable[]
            {
                leftColumn,
                new PacketScoreCounter(scoreProcessor)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding(PacketRunHudLayout.EdgeMargin),
                },
            };
        }
    }
}
