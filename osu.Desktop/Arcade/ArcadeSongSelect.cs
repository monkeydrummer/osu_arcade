// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Select;

namespace osu.Desktop.Arcade
{
    /// <summary>
    /// A <see cref="SoloSongSelect"/> variant for arcade / kiosk mode.
    /// <list type="bullet">
    ///   <item>Restricts the carousel to maps that belong to the configured collection.</item>
    ///   <item>Optionally locks the session to a single ruleset.</item>
    ///   <item>Strips editing / online context-menu entries so players cannot navigate away.</item>
    ///   <item>Keeps overlays disabled.</item>
    /// </list>
    /// </summary>
    public partial class ArcadeSongSelect : SoloSongSelect
    {
        private readonly ArcadeConfiguration arcadeConfig;

        /// <summary>Hashes from the configured collection, computed once on load.</summary>
        private ImmutableHashSet<string>? collectionHashes;

        /// <summary>Locked ruleset info, or <see langword="null"/> if not specified.</summary>
        private RulesetInfo? lockedRuleset;

        public ArcadeSongSelect(ArcadeConfiguration arcadeConfig)
        {
            this.arcadeConfig = arcadeConfig;
        }

        // Keep overlays and toolbar hidden throughout song select.
        public override bool HideOverlaysOnEnter => true;

        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.Disabled;

        // Lock the ruleset bindable so nothing can switch it externally while this screen is active.
        public override bool DisallowExternalBeatmapRulesetChanges => arcadeConfig.RulesetName != null;

        [BackgroundDependencyLoader]
        private void load(RealmAccess realm, IRulesetStore rulesets)
        {
            // Resolve the collection hashes once so that ApplyRequiredCriteria is a pure lookup.
            collectionHashes = realm.Run(r =>
            {
                var collection = r.All<BeatmapCollection>()
                                  .FirstOrDefault(c => c.Name == arcadeConfig.CollectionName);

                if (collection == null)
                {
                    Logger.Log(
                        $"[Arcade] Collection '{arcadeConfig.CollectionName}' not found. " +
                        "All beatmaps will be shown.",
                        LoggingTarget.Runtime,
                        LogLevel.Important);
                    return null;
                }

                Logger.Log(
                    $"[Arcade] Resolved collection '{arcadeConfig.CollectionName}' " +
                    $"with {collection.BeatmapMD5Hashes.Count} beatmaps.",
                    LoggingTarget.Runtime);

                return collection.BeatmapMD5Hashes.ToImmutableHashSet();
            });

            // Resolve the locked ruleset (if any).
            if (!string.IsNullOrEmpty(arcadeConfig.RulesetName))
            {
                lockedRuleset = rulesets.GetRuleset(arcadeConfig.RulesetName) as RulesetInfo;

                if (lockedRuleset == null)
                {
                    Logger.Log(
                        $"[Arcade] Ruleset '{arcadeConfig.RulesetName}' not found. " +
                        "Using the current default ruleset.",
                        LoggingTarget.Runtime,
                        LogLevel.Important);
                }
            }

            FilterControl.ApplyRequiredCriteria = criteria =>
            {
                if (collectionHashes != null)
                    criteria.CollectionBeatmapMD5Hashes = collectionHashes;

                if (lockedRuleset != null)
                    criteria.Ruleset = lockedRuleset;
            };
        }

        // In regular arcade mode (non-developer), hide the filter/search panel at the top right.
        protected override bool ShowFilterControl => arcadeConfig.IsDeveloperMode;

        // In regular arcade mode, force the leaderboard scope to Local only (no sign-in placeholder).
        protected override BeatmapDetailsArea CreateBeatmapDetailsArea() =>
            new BeatmapDetailsArea { ForceLocalScope = !arcadeConfig.IsDeveloperMode };

        // In regular arcade mode, remove the Mods and Options footer buttons; keep only Random.
        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons()
        {
            if (arcadeConfig.IsDeveloperMode)
                return base.CreateFooterButtons();

            return base.CreateFooterButtons()
                       .Where(b => b is not FooterButtonMods && b is not FooterButtonOptions)
                       .ToList();
        }

        /// <summary>
        /// Strip context-menu entries that could navigate to online resources or the beatmap editor.
        /// Players only need "Play".
        /// </summary>
        public override IEnumerable<OsuMenuItem> GetForwardActions(BeatmapInfo beatmap)
        {
            yield return new OsuMenuItem(
                ButtonSystemStrings.Play.ToSentence(),
                MenuItemType.Highlighted,
                () => SelectAndRun(beatmap, OnStart))
            {
                Icon = FontAwesome.Solid.Check
            };
        }
    }
}
