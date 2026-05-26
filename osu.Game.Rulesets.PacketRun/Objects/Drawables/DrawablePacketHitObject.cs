// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Rulesets.PacketRun.UI;
using osu.Game.Rulesets.PacketRun.UI.Components;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.PacketRun.Objects.Drawables
{
    public partial class DrawablePacketHitObject : DrawableHitObject<PacketHitObject>
    {
        [Resolved]
        private PacketGameplayProcessor processor { get; set; } = null!;

        private const double queue_slide_duration = 80;

        private DrawablePacket? packetVisual;
        private bool registered;
        private int queueSlot = -1;

        public DrawablePacketHitObject(PacketHitObject hitObject)
            : base(hitObject)
        {
            Origin = Anchor.Centre;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            AddInternal(packetVisual = new DrawablePacket(HitObject, processor.Beatmap.GetLayoutAt(HitObject.StartTime)));
        }

        protected override void Update()
        {
            base.Update();

            if (!registered && Time.Current >= HitObject.StartTime - 100)
            {
                registered = true;
                processor.RegisterPacket(this);
            }
        }

        public void ApplyCustomResult(HitResult result)
        {
            ApplyResult(result);
        }

        public void RefreshProgress(int progressIndex, bool enteredCorrectly)
        {
            packetVisual?.RefreshProgress(progressIndex, enteredCorrectly);
        }

        public void SetActive(bool active, int progressIndex)
        {
            packetVisual?.SetActive(active, progressIndex);
        }

        public void MarkComplete()
        {
            Expire();
        }

        public void FlashWrong() => packetVisual?.FlashWrong();

        public void UpdateQueueLayout(float x, float targetY, int index)
        {
            X = x;

            if (queueSlot != index)
            {
                if (queueSlot < 0)
                {
                    Y = targetY;
                }
                else
                {
                    this.MoveToY(targetY, queue_slide_duration, Easing.OutCubic);
                }

                queueSlot = index;
            }
            else if (!Transforms.Any())
            {
                Y = targetY;
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            var mode = HitObject.ModeOverride ?? processor.CurrentMode.Value;

            if (mode == PacketGameplayMode.Queue)
            {
                return;
            }

            if (!userTriggered && timeOffset > (HitObject.HitWindows?.WindowFor(HitResult.Miss) ?? 200))
            {
                ApplyMinResult();
            }
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                    this.FadeOut(300);
                    break;

                case ArmedState.Miss:
                    this.FadeOut(300);
                    break;
            }
        }
    }
}
