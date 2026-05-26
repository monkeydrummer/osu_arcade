// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.PacketRun.Skinning.HUD;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.PacketRun.Skinning
{
    public class PacketRunSkinTransformer : SkinTransformer
    {
        public PacketRunSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case GlobalSkinnableContainerLookup global when global.Lookup == GlobalSkinnableContainers.MainHUDComponents:
                    return new PacketRunHudOverlay();
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
