// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.PacketRun.Charts;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.PacketRun.Editor.Components
{
    public partial class ChartTimeline : OsuScrollContainer
    {
        public const float BASE_PIXELS_PER_MS = 0.1f;

        private const float playhead_viewport_fraction = 0.25f;

        public float Zoom { get; set; } = 1f;

        public float PixelsPerMs => BASE_PIXELS_PER_MS * Zoom;

        public event Action<double>? SeekRequested;

        private readonly PacketRunChartDocument document;
        private readonly EditorClock clock;
        private readonly BeatGridOverlay beatGrid;
        private readonly Container packetContainer;
        private readonly Container timelineContent;
        private readonly ChartTimelinePlayhead playhead;

        private readonly List<DrawableEditorPacket> packetDrawables = new List<DrawableEditorPacket>();

        private bool followPlayhead = true;
        private bool wasClockRunning;
        private bool draggingPlayhead;
        private float lastContentWidth;

        public ChartTimeline(PacketRunChartDocument document, EditorClock clock)
            : base(Direction.Horizontal)
        {
            this.document = document;
            this.clock = clock;
            RelativeSizeAxes = Axes.Both;

            Content.Add(timelineContent = new Container
            {
                RelativeSizeAxes = Axes.Y,
                Height = 1,
            });

            timelineContent.Add(beatGrid = new BeatGridOverlay(document)
            {
                RelativeSizeAxes = Axes.Both,
            });

            timelineContent.Add(packetContainer = new Container
            {
                RelativeSizeAxes = Axes.Y,
                Height = 0.8f,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Padding = new MarginPadding { Top = 8, Bottom = 8 },
            });

            timelineContent.Add(playhead = new ChartTimelinePlayhead
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Y,
                Height = 1,
            });

            RebuildPackets();
        }

        public void RebuildPackets()
        {
            packetDrawables.Clear();
            packetContainer.Clear();

            foreach (var packet in document.Chart.Packets)
            {
                var drawable = new DrawableEditorPacket(packet)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    PixelsPerMs = PixelsPerMs,
                };
                packetDrawables.Add(drawable);
                packetContainer.Add(drawable);
            }

            applyPacketDigitLayout();
            updateContentWidth();
        }

        public void RefreshPacketLayout()
        {
            applyPacketDigitLayout();
        }

        public void SeekToChartTime(double timeMs)
        {
            followPlayhead = false;
            float target = (float)(timeMs * PixelsPerMs - DisplayableContent * playhead_viewport_fraction);
            ScrollTo(Math.Clamp(target, 0, ScrollableExtent), false);
        }

        public IReadOnlyList<DrawableEditorPacket> PacketDrawables => packetDrawables;

        protected override void Update()
        {
            base.Update();

            if (clock.IsRunning && !wasClockRunning)
            {
                followPlayhead = true;
            }

            wasClockRunning = clock.IsRunning;

            double currentTime = clock.CurrentTimeAccurate;

            beatGrid.PixelsPerMs = PixelsPerMs;
            beatGrid.VisibleStartMs = Current / PixelsPerMs;
            beatGrid.VisibleEndMs = (Current + DisplayableContent) / PixelsPerMs;

            foreach (var drawable in packetDrawables)
            {
                drawable.PixelsPerMs = PixelsPerMs;
            }

            if (DrawWidth > 0)
            {
                updateContentWidth();
            }

            playhead.X = (float)(currentTime * PixelsPerMs);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (Scrollbar is { IsDragged: true })
            {
                followPlayhead = false;
            }

            if (clock.IsRunning && followPlayhead && !draggingPlayhead)
            {
                float target = (float)(clock.CurrentTimeAccurate * PixelsPerMs - DisplayableContent * playhead_viewport_fraction);
                ScrollTo(Math.Clamp(target, 0, ScrollableExtent), false);
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (draggingPlayhead || isMouseOverPlayhead(e.MousePosition))
            {
                return false;
            }

            seekFromViewportX(e.MousePosition.X);
            return true;
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (e.ControlPressed)
            {
                Zoom = Math.Clamp(Zoom * (e.ScrollDelta.Y > 0 ? 1.1f : 0.9f), 0.25f, 4f);
                updateContentWidth();
                return true;
            }

            followPlayhead = false;
            return base.OnScroll(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (isMouseOverPlayhead(e.MousePosition))
            {
                draggingPlayhead = true;
                followPlayhead = false;
                return true;
            }

            return base.OnDragStart(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            if (draggingPlayhead)
            {
                seekFromViewportX(e.MousePosition.X);
                return;
            }

            followPlayhead = false;
            base.OnDrag(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            draggingPlayhead = false;
            base.OnDragEnd(e);
        }

        private void seekFromViewportX(float viewportX)
        {
            double time = (Current + viewportX) / PixelsPerMs;
            SeekRequested?.Invoke(document.SnapTime(Math.Max(0, time)));
        }

        private bool isMouseOverPlayhead(Vector2 timelineLocalMouse)
        {
            float playheadViewportX = ToLocalSpace(playhead.ScreenSpaceDrawQuad.Centre).X;
            return Math.Abs(timelineLocalMouse.X - playheadViewportX) <= ChartTimelinePlayhead.HIT_WIDTH * 0.5f;
        }

        private void applyPacketDigitLayout()
        {
            bool vertical = document.ViewSettings.ShowVerticalDigits;

            foreach (var drawable in packetDrawables)
            {
                drawable.SetVerticalLayout(vertical);
            }
        }

        private void updateContentWidth()
        {
            double maxTime = Math.Max(60000, clock.TrackLength);
            maxTime = Math.Max(maxTime, document.Chart.Metadata.OffsetMs + 5000);

            foreach (var packet in document.Chart.Packets)
            {
                maxTime = Math.Max(maxTime, packet.TimeMs + 5000);
            }

            float newWidth = (float)(maxTime * PixelsPerMs) + DisplayableContent;

            if (Math.Abs(newWidth - lastContentWidth) > 0.5f)
            {
                timelineContent.Width = newWidth;
                packetContainer.Width = newWidth;
                lastContentWidth = newWidth;
            }
        }
    }
}
