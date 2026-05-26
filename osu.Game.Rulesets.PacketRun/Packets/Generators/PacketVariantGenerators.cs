// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.PacketRun.Objects;

namespace osu.Game.Rulesets.PacketRun.Packets.Generators
{
    public abstract class RowBasedGenerator : IPacketVariantGenerator
    {
        public abstract PacketVariant Variant { get; }

        public abstract PacketModFlags RequiredFlag { get; }

        protected abstract int[] GenerateFromRow(int[] row, int length, Random rng);

        public bool CanGenerate(PacketGenerationContext context) => (context.EnabledMods & RequiredFlag) != 0;

        public GeneratedPacket Generate(PacketGenerationContext context)
        {
            var row = numpad_rows[rng(context).Next(numpad_rows.Length)];
            int length = context.MinLength + rng(context).Next(context.MaxLength - context.MinLength + 1);

            return new GeneratedPacket
            {
                Digits = GenerateFromRow(row, length, rng(context)),
                Variant = Variant,
            };
        }

        private static Random rng(PacketGenerationContext context) => context.Random;

        private static readonly int[][] numpad_rows =
        {
            new[] { 1, 2, 3 },
            new[] { 4, 5, 6 },
            new[] { 7, 8, 9 },
        };
    }

    public class DescendingGenerator : RowBasedGenerator
    {
        public override PacketVariant Variant => PacketVariant.Descending;
        public override PacketModFlags RequiredFlag => PacketModFlags.Descending;

        protected override int[] GenerateFromRow(int[] row, int length, Random rng)
        {
            int start = rng.Next(row.Length - length + 1);
            return row.Skip(start).Take(length).Reverse().ToArray();
        }
    }

    public class AscendingGenerator : RowBasedGenerator
    {
        public override PacketVariant Variant => PacketVariant.Ascending;
        public override PacketModFlags RequiredFlag => PacketModFlags.Ascending;

        protected override int[] GenerateFromRow(int[] row, int length, Random rng)
        {
            int start = rng.Next(row.Length - length + 1);
            return row.Skip(start).Take(length).ToArray();
        }
    }

    public class OreoGenerator : RowBasedGenerator
    {
        public override PacketVariant Variant => PacketVariant.Oreo;
        public override PacketModFlags RequiredFlag => PacketModFlags.Oreo;

        protected override int[] GenerateFromRow(int[] row, int length, Random rng)
        {
            if (length < 3)
            {
                length = 3;
            }

            int a = row[rng.Next(row.Length)];
            int b = row[rng.Next(row.Length)];
            var digits = new int[length];
            digits[0] = a;

            for (int i = 1; i < length - 1; i++)
            {
                digits[i] = b;
            }

            digits[length - 1] = a;
            return digits;
        }
    }

    public class RepeatGenerator : RowBasedGenerator
    {
        public override PacketVariant Variant => PacketVariant.Repeat;
        public override PacketModFlags RequiredFlag => PacketModFlags.Repeat;

        protected override int[] GenerateFromRow(int[] row, int length, Random rng)
        {
            int digit = row[rng.Next(row.Length)];
            return Enumerable.Repeat(digit, length).ToArray();
        }
    }

    public class DiagonalGenerator : IPacketVariantGenerator
    {
        private readonly bool descending;

        public DiagonalGenerator(bool descending)
        {
            this.descending = descending;
        }

        public PacketVariant Variant => descending ? PacketVariant.DiagonalDesc : PacketVariant.DiagonalAsc;
        public PacketModFlags RequiredFlag => PacketModFlags.Diagonals;

        private static readonly int[][] diagonals =
        {
            new[] { 1, 5, 9 },
            new[] { 3, 5, 7 },
            new[] { 7, 5, 3 },
            new[] { 9, 5, 1 },
        };

        public bool CanGenerate(PacketGenerationContext context) => (context.EnabledMods & RequiredFlag) != 0;

        public GeneratedPacket Generate(PacketGenerationContext context)
        {
            var options = diagonals.Where(d => descending ? d[0] > d[^1] : d[0] < d[^1]).ToArray();
            var digits = options[context.Random.Next(options.Length)];

            return new GeneratedPacket
            {
                Digits = digits,
                Variant = Variant,
            };
        }
    }

    public class ColumnGenerator : IPacketVariantGenerator
    {
        private readonly bool descending;

        public ColumnGenerator(bool descending)
        {
            this.descending = descending;
        }

        public PacketVariant Variant => descending ? PacketVariant.ColumnDesc : PacketVariant.ColumnAsc;
        public PacketModFlags RequiredFlag => PacketModFlags.Columns;

        private static readonly int[][] columns =
        {
            new[] { 1, 4, 7 },
            new[] { 2, 5, 8 },
            new[] { 3, 6, 9 },
            new[] { 7, 4, 1 },
            new[] { 8, 5, 2 },
            new[] { 9, 6, 3 },
        };

        public bool CanGenerate(PacketGenerationContext context) => (context.EnabledMods & RequiredFlag) != 0;

        public GeneratedPacket Generate(PacketGenerationContext context)
        {
            var options = columns.Where(c => descending ? c[0] > c[^1] : c[0] < c[^1]).ToArray();
            var digits = options[context.Random.Next(options.Length)];

            return new GeneratedPacket
            {
                Digits = digits,
                Variant = Variant,
            };
        }
    }

    public class PrePost0Generator : IPacketVariantGenerator
    {
        private readonly PacketGeneratorRegistry registry;

        public PrePost0Generator(PacketGeneratorRegistry registry)
        {
            this.registry = registry;
        }

        public PacketVariant Variant => PacketVariant.PrePost0;
        public PacketModFlags RequiredFlag => PacketModFlags.PrePost0;

        public bool CanGenerate(PacketGenerationContext context) => (context.EnabledMods & RequiredFlag) != 0;

        public GeneratedPacket Generate(PacketGenerationContext context)
        {
            var innerContext = new PacketGenerationContext
            {
                Random = context.Random,
                EnabledMods = context.EnabledMods & ~PacketModFlags.PrePost0,
                MinLength = Math.Max(2, context.MinLength - 1),
                MaxLength = Math.Max(2, context.MaxLength - 1),
                PreferredLayout = context.PreferredLayout,
                ScrollSpeedTier = context.ScrollSpeedTier,
            };

            var inner = registry.Generate(innerContext);
            bool prefix = context.Random.NextDouble() < 0.5;

            var digits = prefix
                ? new[] { 0 }.Concat(inner.Digits).ToArray()
                : inner.Digits.Concat(new[] { 0 }).ToArray();

            return new GeneratedPacket
            {
                Digits = digits,
                Variant = Variant,
            };
        }
    }
}
