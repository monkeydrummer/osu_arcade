// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Replays;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.PacketRun.Replays
{
    public class PacketRunReplayFrame : ReplayFrame
    {
        public int Digit { get; set; }
    }

    public class PacketRunFramedReplayInputHandler : FramedReplayInputHandler<PacketRunReplayFrame>
    {
        public PacketRunFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }
    }
}
