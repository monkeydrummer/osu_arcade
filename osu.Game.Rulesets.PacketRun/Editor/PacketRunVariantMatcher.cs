// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.PacketRun.Objects;

namespace osu.Game.Rulesets.PacketRun.Editor
{
    public static class PacketRunVariantMatcher
    {
        public static string TryInfer(int[] digits)
        {
            if (digits.Length == 0)
            {
                return "custom";
            }

            if (digits.All(d => d == digits[0]))
            {
                return "repeat";
            }

            bool ascending = true;
            bool descending = true;

            for (int i = 1; i < digits.Length; i++)
            {
                if (digits[i] <= digits[i - 1])
                {
                    ascending = false;
                }

                if (digits[i] >= digits[i - 1])
                {
                    descending = false;
                }
            }

            if (ascending)
            {
                return "ascending";
            }

            if (descending)
            {
                return "descending";
            }

            if (digits.Length >= 3 && digits[0] == digits[^1])
            {
                return "oreo";
            }

            return "custom";
        }

        public static PacketVariant ParseVariant(string variant) =>
            System.Enum.TryParse<PacketVariant>(variant, true, out var parsed) ? parsed : PacketVariant.Custom;
    }
}
