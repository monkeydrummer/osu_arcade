// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.PacketRun.Skinning.HUD;
using osu.Game.Rulesets.PacketRun.UI.Components;
using osu.Game.Rulesets.Scoring;
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
                case GlobalSkinnableContainerLookup containerLookup:
                    // Only replace the ruleset-specific HUD container (see HUDOverlay.rulesetComponents).
                    if (containerLookup.Ruleset == null)
                    {
                        return base.GetDrawableComponent(lookup);
                    }

                    switch (containerLookup.Lookup)
                    {
                        case GlobalSkinnableContainers.MainHUDComponents:
                            return new PacketRunHudOverlay();
                    }

                    break;

                case SkinComponentLookup<HitResult> resultComponent:
                    return new PacketRunJudgementPiece(resultComponent.Component);
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
