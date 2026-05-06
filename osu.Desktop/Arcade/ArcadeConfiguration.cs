// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using Newtonsoft.Json;
using osu.Framework.Logging;

namespace osu.Desktop.Arcade
{
    /// <summary>
    /// Operator-supplied configuration for arcade / kiosk mode.
    /// Deserialised from <c>arcade.json</c> next to the executable, or from the path
    /// provided via <c>--arcade-config=&lt;path&gt;</c>.
    /// </summary>
    public class ArcadeConfiguration
    {
        /// <summary>
        /// The name of the in-game beatmap collection whose maps are shown in song select.
        /// Must match the collection name exactly (case-sensitive).
        /// </summary>
        [JsonProperty("collectionName")]
        public string CollectionName { get; set; } = "Arcade";

        /// <summary>
        /// Short name of the ruleset to lock the session to (e.g. <c>"osu"</c>, <c>"taiko"</c>,
        /// <c>"fruits"</c>, <c>"mania"</c>).  When <see langword="null"/> the default ruleset
        /// from the user config is used and the player can switch freely from song select.
        /// </summary>
        [JsonProperty("ruleset")]
        public string? RulesetName { get; set; }

        /// <summary>
        /// When <see langword="true"/>, the arcade session was launched with <c>--arcade_d</c> and
        /// runs without kiosk UI restrictions (search panel, mods/options footer buttons, and leaderboard
        /// scope lock are all visible/unlocked).  Never serialised to JSON — set programmatically only.
        /// </summary>
        [JsonIgnore]
        public bool IsDeveloperMode { get; set; }

        // -----------------------------------------------------------------------
        // Factory helpers
        // -----------------------------------------------------------------------

        /// <summary>Loads configuration from the given path, or returns a default instance on failure.</summary>
        public static ArcadeConfiguration LoadFromFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<ArcadeConfiguration>(json) ?? new ArcadeConfiguration();
            }
            catch (Exception ex)
            {
                Logger.Log($"[Arcade] Failed to read config from '{path}': {ex.Message}. Using defaults.", LoggingTarget.Runtime, LogLevel.Important);
                return new ArcadeConfiguration();
            }
        }

        /// <summary>
        /// Returns the default config file path (arcade.json next to the executable).
        /// </summary>
        public static string DefaultConfigPath =>
            Path.Combine(AppContext.BaseDirectory, "arcade.json");
    }
}
