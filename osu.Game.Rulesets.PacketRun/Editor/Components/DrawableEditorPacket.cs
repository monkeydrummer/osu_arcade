// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.PacketRun.Charts;
using osu.Game.Rulesets.PacketRun.UI.Components;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.PacketRun.Editor.Components
{
    public partial class DigitBox : CompositeDrawable
    {
        public event Action? Clicked;

        public DigitBox(int digit, bool selected)
        {
            Size = new Vector2(DrawablePacket.DIGIT_SIZE);
            Colour4 glow = PacketRunColours.ForDigit(digit);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = selected ? Color4.White : glow.Darken(0.6f),
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = glow,
                    Alpha = selected ? 0.95f : 0.5f,
                    Margin = new MarginPadding(2),
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = digit.ToString(),
                    Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold),
                    Colour = Color4.White,
                },
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            Clicked?.Invoke();
            return true;
        }
    }

    public partial class DrawableEditorPacket : CompositeDrawable
    {
        public event Action<DrawableEditorPacket>? Selected;

        public event Action<DrawableEditorPacket, int>? DigitSelected;

        public event Action<DrawableEditorPacket, double>? TimeChanged;

        public PacketRunChartPacket Packet { get; }

        public float PixelsPerMs { get; set; } = 0.1f;

        public bool UseVerticalLayout { get; set; }

        public bool IsPacketSelected { get; private set; }

        public int? SelectedDigitIndex { get; private set; }

        private bool dragging;
        private float dragStartX;
        private double dragStartTime;

        private Container content = null!;

        public DrawableEditorPacket(PacketRunChartPacket packet)
        {
            Packet = packet;
            AutoSizeAxes = Axes.Both;
            InternalChild = content = new Container { AutoSizeAxes = Axes.Both };
            rebuild();
        }

        public void SetVerticalLayout(bool vertical)
        {
            if (UseVerticalLayout == vertical)
            {
                return;
            }

            UseVerticalLayout = vertical;
            rebuild();
        }

        protected override void Update()
        {
            base.Update();
            X = (float)(Packet.TimeMs * PixelsPerMs);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
            {
                return false;
            }

            dragging = true;
            dragStartX = e.MouseDownPosition.X;
            dragStartTime = Packet.TimeMs;
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (e.Button != MouseButton.Left)
            {
                return;
            }

            dragging = false;
        }

        protected override bool OnClick(ClickEvent e)
        {
            Selected?.Invoke(this);
            return true;
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (!dragging)
            {
                return false;
            }

            Selected?.Invoke(this);
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            if (!dragging)
            {
                return;
            }

            double deltaMs = (e.MousePosition.X - dragStartX) / PixelsPerMs;
            double newTime = Math.Max(0, dragStartTime + deltaMs);
            TimeChanged?.Invoke(this, newTime);
        }

        public void SetSelected(bool packetSelected, int? digitIndex)
        {
            IsPacketSelected = packetSelected;
            SelectedDigitIndex = digitIndex;
            rebuild();
        }

        private Vector2 getDigitPosition(int index)
        {
            if (UseVerticalLayout)
            {
                return new Vector2(index * DrawablePacket.VERTICAL_STAGGER, index * (DrawablePacket.DIGIT_SIZE + DrawablePacket.DIGIT_SPACING));
            }

            return new Vector2(index * (DrawablePacket.DIGIT_SIZE + DrawablePacket.DIGIT_SPACING), 0);
        }

        private void rebuild()
        {
            content.Clear();

            if (IsPacketSelected)
            {
                content.Add(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Alpha = 0.15f,
                    Depth = -1,
                });
            }

            for (int i = 0; i < Packet.Digits.Length; i++)
            {
                int digit = Packet.Digits[i];
                int capturedIndex = i;
                bool digitSelected = SelectedDigitIndex == i;

                var digitBox = new DigitBox(digit, digitSelected)
                {
                    Position = getDigitPosition(i),
                };
                digitBox.Clicked += () => DigitSelected?.Invoke(this, capturedIndex);
                content.Add(digitBox);
            }
        }
    }
}
