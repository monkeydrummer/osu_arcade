// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
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

        private DrawablePacket? packetVisual;
        private bool registered;

        public DrawablePacketHitObject(PacketHitObject hitObject)
            : base(hitObject)
        {
            Origin = Anchor.Centre;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            AddInternal(packetVisual = new DrawablePacket(HitObject));
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

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
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
