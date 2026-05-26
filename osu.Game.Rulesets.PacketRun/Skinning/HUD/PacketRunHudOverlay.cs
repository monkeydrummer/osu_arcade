// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
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
        public static Vector2 SignalPosition => new(0.02f, 0.02f);
        public static Vector2 HeatPosition => new(0.02f, 0.08f);
        public static Vector2 SyncPosition => new(0.02f, 0.14f);
        public static Vector2 ScorePosition => new(0.98f, 0.02f);
    }

    public partial class PacketRunHudOverlay : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            RelativeSizeAxes = Axes.Both;

            var packetProcessor = scoreProcessor as PacketRunScoreProcessor;
            var children = new List<Drawable>
            {
                new SyncCounter(scoreProcessor)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativePositionAxes = Axes.Both,
                    Position = PacketRunHudLayout.SyncPosition,
                },
                new PacketScoreCounter(scoreProcessor)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativePositionAxes = Axes.Both,
                    Position = PacketRunHudLayout.ScorePosition,
                },
            };

            if (packetProcessor != null)
            {
                children.Insert(0, new SignalCounter(packetProcessor)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativePositionAxes = Axes.Both,
                    Position = PacketRunHudLayout.SignalPosition,
                });
                children.Insert(1, new HeatCounter(packetProcessor)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativePositionAxes = Axes.Both,
                    Position = PacketRunHudLayout.HeatPosition,
                });
            }

            InternalChildren = children.ToArray();
        }
    }
}
