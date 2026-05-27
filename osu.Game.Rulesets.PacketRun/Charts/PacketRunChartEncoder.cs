// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace osu.Game.Rulesets.PacketRun.Charts
{
    public static class PacketRunChartEncoder
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
        };

        public static string Encode(PacketRunChartFile chart) => JsonSerializer.Serialize(chart, options);

        public static void EncodeFile(PacketRunChartFile chart, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, Encode(chart));
        }
    }
}
