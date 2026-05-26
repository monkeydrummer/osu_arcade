// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.PacketRun
{
    [Cached]
    public partial class PacketRunInputManager : RulesetInputManager<PacketRunAction>
    {
        public PacketRunInputManager(RulesetInfo ruleset, int variant)
            : base(ruleset, variant, SimultaneousBindingMode.Unique)
        {
        }

        public int? ActionToDigit(PacketRunAction action)
        {
            int digit = (int)action;
            if (digit >= 0 && digit <= 9)
                return digit;

            return null;
        }
    }
}
