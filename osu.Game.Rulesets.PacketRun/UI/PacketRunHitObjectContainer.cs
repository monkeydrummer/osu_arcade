// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Rulesets.PacketRun.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

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

                if (mode != PacketGameplayMode.Queue)
                {
                    continue;
                }

                if (visibleIndices.TryGetValue(drawable, out int index))
                {
                    drawable.Position = new Vector2(DrawWidth * queue_x, DrawHeight * queue_base_y + index * queue_spacing);
                    drawable.Alpha = 1;
                }
                else
                {
                    drawable.Alpha = 0;
                }
            }
        }
    }
}
