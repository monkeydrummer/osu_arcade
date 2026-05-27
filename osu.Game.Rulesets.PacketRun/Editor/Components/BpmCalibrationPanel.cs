// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.PacketRun.Charts;
using osu.Game.Rulesets.PacketRun.Screens.Components;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.PacketRun.Editor.Components
{
    public partial class BpmCalibrationPanel : CompositeDrawable
    {
        private readonly PacketRunChartDocument document;
        private readonly EditorClock clock;
        private readonly List<double> tapWallTimes = new List<double>();
        private OsuSpriteText statusText = null!;
        private OsuSpriteText resultText = null!;
        private double? suggestedBpm;
        private double? suggestedOffset;
        private double? firstTapSongTime;

        public override bool HandleNonPositionalInput => true;

        public BpmCalibrationPanel(PacketRunChartDocument document, EditorClock clock)
        {
            this.document = document;
            this.clock = clock;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 12),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = "BPM & Offset Calibration",
                        Font = OsuFont.Torus.With(size: 28, weight: FontWeight.Bold),
                        Colour = new Color4(80, 220, 120, 255),
                    },
                    new OsuSpriteText
                    {
                        Text = "Play the song and tap Space on each beat.",
                        Font = OsuFont.Torus.With(size: 16),
                        Colour = Color4.White,
                    },
                    statusText = new OsuSpriteText
                    {
                        Font = OsuFont.Torus.With(size: 16),
                        Colour = Color4.White,
                    },
                    resultText = new OsuSpriteText
                    {
                        Font = OsuFont.Torus.With(size: 16),
                        Colour = new Color4(255, 200, 80, 255),
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(12, 0),
                        Children = new Drawable[]
                        {
                            new PacketRunMenuButton("Play / Pause", togglePlayback) { Size = new Vector2(140, 40) },
                            new PacketRunMenuButton("Reset Taps", resetTaps) { Size = new Vector2(120, 40) },
                            new PacketRunMenuButton("Apply", applyResult) { Size = new Vector2(120, 40) },
                        },
                    },
                },
            };

            updateStatus();
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == Key.Space && !e.Repeat)
            {
                registerTap();
                return true;
            }

            return base.OnKeyDown(e);
        }

        private void registerTap()
        {
            if (tapWallTimes.Count == 0)
            {
                firstTapSongTime = clock.CurrentTime;
            }

            tapWallTimes.Add(Clock.CurrentTime);
            updateStatus();

            if (tapWallTimes.Count >= 4)
            {
                computeResult();
            }
        }

        private void computeResult()
        {
            var intervals = new List<double>();

            for (int i = 1; i < tapWallTimes.Count; i++)
            {
                double interval = tapWallTimes[i] - tapWallTimes[i - 1];

                if (interval > 0)
                {
                    intervals.Add(interval);
                }
            }

            if (intervals.Count == 0)
            {
                suggestedBpm = null;
                suggestedOffset = null;
                resultText.Text = "Could not detect a beat interval. Tap more steadily on each beat.";
                return;
            }

            intervals.Sort();
            double medianInterval = intervals[intervals.Count / 2];
            suggestedBpm = 60000.0 / medianInterval;
            suggestedOffset = firstTapSongTime ?? 0;
            resultText.Text = $"Suggested: {suggestedBpm:N1} BPM, offset {suggestedOffset:N0} ms ({document.SetupSettings.TimeSignature})";
        }

        private void applyResult()
        {
            if (suggestedBpm == null || suggestedOffset == null)
            {
                statusText.Text = "Tap at least 4 beats before applying.";
                return;
            }

            document.ApplyTimingResult(suggestedBpm.Value, suggestedOffset.Value, document.SetupSettings.TimeSignature);
            statusText.Text = "Applied BPM and offset to chart.";
        }

        private void togglePlayback()
        {
            if (clock.IsRunning)
            {
                clock.Stop();
            }
            else
            {
                clock.Start();
            }
        }

        private void resetTaps()
        {
            tapWallTimes.Clear();
            firstTapSongTime = null;
            suggestedBpm = null;
            suggestedOffset = null;
            resultText.Text = string.Empty;
            updateStatus();
        }

        private void updateStatus()
        {
            statusText.Text = $"Taps: {tapWallTimes.Count}  |  Time: {clock.CurrentTime:N0} ms";
        }
    }
}
