// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Rulesets.PacketRun.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.UI
{
    [Cached]
    public partial class PacketRunHitObjectContainer : ScrollingHitObjectContainer
    {
        [Resolved]
        private PacketGameplayProcessor processor { get; set; } = null!;

        protected override void UpdateAfterChildrenLife()
        {
            base.UpdateAfterChildrenLife();

            int queueIndex = 0;

            foreach (var entry in AliveEntries)
            {
                if (entry.Value is not DrawablePacketHitObject drawable)
                {
                    continue;
                }

                var mode = drawable.HitObject.ModeOverride ?? processor.CurrentMode.Value;

                if (mode == PacketGameplayMode.Queue)
                {
                    float y = DrawHeight * 0.35f + queueIndex * 70;
                    drawable.Position = new Vector2(DrawWidth * 0.72f, y);
                    queueIndex++;
                }
            }
        }
    }
}
