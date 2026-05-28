// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.PacketRun.BeatGrid;
using osu.Game.Rulesets.PacketRun.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.UI.Components
{
    public partial class RhythmBeatGridOverlay : CompositeDrawable
    {
        private readonly PacketRunBeatGrid beatGrid;
        private readonly Container linesContainer;
        private readonly Dictionary<long, BeatLine> linePool = new Dictionary<long, BeatLine>();

        [Resolved]
        private PacketGameplayProcessor processor { get; set; } = null!;

        private readonly IBindable<PacketGameplayMode> currentMode = new Bindable<PacketGameplayMode>();

        public RhythmBeatGridOverlay(PacketRunBeatGrid beatGrid)
        {
            this.beatGrid = beatGrid;
            RelativeSizeAxes = Axes.Both;

            InternalChild = linesContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            currentMode.BindTo(processor.CurrentMode);
        }

        protected override void Update()
        {
            base.Update();

            if (currentMode.Value != PacketGameplayMode.Rhythm)
            {
                foreach (var line in linePool.Values)
                {
                    line.SetVisible(false);
                }

                return;
            }

            double currentTime = Time.Current;
            double lookAhead = PacketRunRhythmLayout.ApproachDuration + beatGrid.BeatIntervalMs * 2;
            double lookBack = PacketRunRhythmLayout.ApproachDuration + beatGrid.BeatIntervalMs * 2;

            var activeKeys = new HashSet<long>();

            foreach (double beatTime in beatGrid.GetBeatTimes(currentTime - lookBack, currentTime + lookAhead, majorOnly: false))
            {
                long key = (long)Math.Round(beatTime);
                activeKeys.Add(key);

                if (!linePool.TryGetValue(key, out var line))
                {
                    bool major = beatGrid.IsMajorBeat(beatTime);
                    line = new BeatLine(major);
                    linePool[key] = line;
                    linesContainer.Add(line);
                }

                double timeToHit = beatTime - currentTime;
                float x = PacketRunRhythmLayout.GetScrollX(timeToHit, DrawWidth, out bool visible);
                line.Position = new Vector2(x, 0);
                line.Height = DrawHeight;
                line.Alpha = visible ? 1 : 0;
            }

            foreach (var (key, line) in linePool)
            {
                if (!activeKeys.Contains(key))
                {
                    line.SetVisible(false);
                }
            }
        }

        private partial class BeatLine : Box
        {
            public BeatLine(bool major)
            {
                Width = major ? 2 : 1;
                Colour = major ? new Color4(120, 180, 255, 180) : new Color4(80, 80, 120, 100);
            }

            public void SetVisible(bool visible) => Alpha = visible ? 1 : 0;
        }
    }
}
