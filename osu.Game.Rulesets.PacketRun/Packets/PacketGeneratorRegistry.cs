// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.PacketRun.Packets.Generators;

namespace osu.Game.Rulesets.PacketRun.Packets
{
    public class PacketGeneratorRegistry
    {
        private readonly List<IPacketVariantGenerator> generators;

        public PacketGeneratorRegistry()
        {
            generators = new List<IPacketVariantGenerator>
            {
                new DescendingGenerator(),
                new AscendingGenerator(),
                new OreoGenerator(),
                new RepeatGenerator(),
                new DiagonalGenerator(descending: true),
                new DiagonalGenerator(descending: false),
                new ColumnGenerator(descending: true),
                new ColumnGenerator(descending: false),
            };

            generators.Add(new PrePost0Generator(this));
        }

        public GeneratedPacket Generate(PacketGenerationContext context)
        {
            var available = generators.Where(g => g.CanGenerate(context)).ToList();

            if (available.Count == 0)
            {
                return new DescendingGenerator().Generate(new PacketGenerationContext
                {
                    Random = context.Random,
                    EnabledMods = PacketModFlags.Descending,
                    MinLength = context.MinLength,
                    MaxLength = context.MaxLength,
                    PreferredLayout = context.PreferredLayout,
                    ScrollSpeedTier = context.ScrollSpeedTier,
                });
            }

            return available[context.Random.Next(available.Count)].Generate(context);
        }
    }
}
