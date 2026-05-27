// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Text.Json;
using osu.Game.Rulesets.PacketRun.Charts;

namespace osu.Game.Rulesets.PacketRun.Editor
{
    public class PacketRunEditorClipboard
    {
        private readonly List<PacketRunChartPacket> packets = new List<PacketRunChartPacket>();

        public bool HasContent => packets.Count > 0;

        public void Copy(IEnumerable<PacketRunChartPacket> source)
        {
            packets.Clear();

            foreach (var packet in source)
            {
                packets.Add(clone(packet));
            }
        }

        public IReadOnlyList<PacketRunChartPacket> GetClonedPackets() =>
            packets.ConvertAll(clone);

        private static PacketRunChartPacket clone(PacketRunChartPacket packet) =>
            JsonSerializer.Deserialize<PacketRunChartPacket>(JsonSerializer.Serialize(packet))!;
    }
}
