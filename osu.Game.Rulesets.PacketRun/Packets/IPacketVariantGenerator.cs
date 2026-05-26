// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.PacketRun.Objects;

namespace osu.Game.Rulesets.PacketRun.Packets
{
    public readonly struct GeneratedPacket
    {
        public int[] Digits { get; init; }

        public PacketVariant Variant { get; init; }

        public PacketLayout Layout { get; init; }
    }

    public interface IPacketVariantGenerator
    {
        PacketVariant Variant { get; }

        PacketModFlags RequiredFlag { get; }

        bool CanGenerate(PacketGenerationContext context);

        GeneratedPacket Generate(PacketGenerationContext context);
    }
}
