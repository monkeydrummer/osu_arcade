// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.PacketRun.Packets
{
    [Flags]
    public enum PacketModFlags
    {
        None = 0,
        Descending = 1 << 0,
        Ascending = 1 << 1,
        Oreo = 1 << 2,
        Repeat = 1 << 3,
        Diagonals = 1 << 4,
        Columns = 1 << 5,
        PrePost0 = 1 << 6,
        AllStandard = Descending | Ascending | Oreo | Repeat,
        All = AllStandard | Diagonals | Columns | PrePost0,
    }
}
