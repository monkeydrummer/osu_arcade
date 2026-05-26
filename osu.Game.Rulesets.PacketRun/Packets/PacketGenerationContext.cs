// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.PacketRun.Objects;

namespace osu.Game.Rulesets.PacketRun.Packets
{
    public class PacketGenerationContext
    {
        public Random Random { get; init; } = new Random();

        public PacketModFlags EnabledMods { get; init; } = PacketModFlags.AllStandard;

        public int MinLength { get; init; } = 3;

        public int MaxLength { get; init; } = 3;

        public PacketLayout PreferredLayout { get; init; } = PacketLayout.Horizontal;

        public double ScrollSpeedTier { get; init; } = 1;
    }
}
