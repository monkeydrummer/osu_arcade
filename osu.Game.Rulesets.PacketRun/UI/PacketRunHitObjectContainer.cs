// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Rulesets.PacketRun.Objects.Drawables;
using osu.Game.Rulesets.PacketRun.UI.Components;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.PacketRun.UI
{
    [Cached]
    public partial class PacketRunHitObjectContainer : ScrollingHitObjectContainer
    {
        private const int max_visible_queue_depth = 5;
        private const float queue_spacing = 46f;
        private const float queue_x = 0.72f;
        private const float queue_base_y = 0.30f;

        [Resolved]
        private PacketGameplayProcessor processor { get; set; } = null!;

        protected override void UpdateAfterChildrenLife()
        {
            base.UpdateAfterChildrenLife();

            processor.UpdateRhythmVisuals(Time.Current);

            var visibleQueue = processor.GetOrderedQueue().Take(max_visible_queue_depth).ToList();
            var visibleIndices = new Dictionary<DrawablePacketHitObject, int>(visibleQueue.Count);

            for (int i = 0; i < visibleQueue.Count; i++)
            {
                visibleIndices[visibleQueue[i]] = i;
            }

            foreach (var entry in AliveEntries)
            {
                if (entry.Value is not DrawablePacketHitObject drawable)
                {
                    continue;
                }

                var mode = drawable.HitObject.ModeOverride ?? processor.CurrentMode.Value;

                if (mode == PacketGameplayMode.Rhythm)
                {
                    applyRhythmScrollPosition(drawable, out bool visible);
                    drawable.Y = DrawHeight / 2f;
                    drawable.Alpha = visible ? 1 : 0;
                    continue;
                }

                if (mode != PacketGameplayMode.Queue)
                {
                    continue;
                }

                if (visibleIndices.TryGetValue(drawable, out int index))
                {
                    float x = DrawWidth * queue_x;
                    float y = DrawHeight * queue_base_y + index * queue_spacing;
                    drawable.UpdateQueueLayout(x, y, index);
                    drawable.Alpha = 1;
                }
                else
                {
                    drawable.Alpha = 0;
                }
            }
        }

        private void applyRhythmScrollPosition(DrawablePacketHitObject drawable, out bool visible)
        {
            double timeToHit = drawable.HitObject.StartTime - Time.Current;
            float spawnX = DrawWidth * PacketRunRhythmLayout.SpawnX;
            float visualHitX = DrawWidth * PacketRunRhythmLayout.HitLineX;
            // Packet visuals are top-left anchored on the hit object origin, so offset the scroll target
            // left by half a digit width so the first digit centers on the hit line at StartTime.
            float hitX = visualHitX - DrawablePacket.DIGIT_SIZE / 2f;
            float travel = spawnX - hitX;
            double approachDuration = PacketRunRhythmLayout.ApproachDuration;

            if (timeToHit > approachDuration)
            {
                drawable.X = spawnX;
                visible = false;
                return;
            }

            visible = true;

            if (timeToHit > 0)
            {
                float progress = 1f - (float)(timeToHit / approachDuration);
                drawable.X = spawnX - travel * progress;
            }
            else
            {
                float progress = (float)(-timeToHit / approachDuration);
                drawable.X = hitX - travel * progress;
            }
        }
    }
}
