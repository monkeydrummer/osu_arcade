// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.PacketRun.Beatmaps;
using osu.Game.Rulesets.PacketRun.Difficulty;
using osu.Game.Rulesets.PacketRun.Mods;
using osu.Game.Rulesets.PacketRun.Scoring;
using osu.Game.Rulesets.PacketRun.Skinning;
using osu.Game.Rulesets.PacketRun.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.PacketRun
{
    public class PacketRunRuleset : Ruleset
    {
        public const string SHORT_NAME = "packetrun";

        public override string Description => "Packet Run";

        public override string ShortName => SHORT_NAME;

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods) =>
            new DrawablePacketRunRuleset(this, beatmap, mods);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new PacketRunBeatmapConverter(beatmap, this);

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) =>
            new PacketRunDifficultyCalculator(RulesetInfo, beatmap);

        public override ScoreProcessor CreateScoreProcessor() => new PacketRunScoreProcessor();

        public override HealthProcessor CreateHealthProcessor(double drainStartTime) => new PacketRunHealthProcessor(drainStartTime);

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.Automation:
                    return new[] { new PacketRunModAutoplay() };

                default:
                    return Array.Empty<Mod>();
            }
        }

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0)
        {
            foreach (var action in new[]
                     {
                         (InputKey.Keypad0, PacketRunAction.Key0),
                         (InputKey.Keypad1, PacketRunAction.Key1),
                         (InputKey.Keypad2, PacketRunAction.Key2),
                         (InputKey.Keypad3, PacketRunAction.Key3),
                         (InputKey.Keypad4, PacketRunAction.Key4),
                         (InputKey.Keypad5, PacketRunAction.Key5),
                         (InputKey.Keypad6, PacketRunAction.Key6),
                         (InputKey.Keypad7, PacketRunAction.Key7),
                         (InputKey.Keypad8, PacketRunAction.Key8),
                         (InputKey.Keypad9, PacketRunAction.Key9),
                         (InputKey.Number0, PacketRunAction.Key0),
                         (InputKey.Number1, PacketRunAction.Key1),
                         (InputKey.Number2, PacketRunAction.Key2),
                         (InputKey.Number3, PacketRunAction.Key3),
                         (InputKey.Number4, PacketRunAction.Key4),
                         (InputKey.Number5, PacketRunAction.Key5),
                         (InputKey.Number6, PacketRunAction.Key6),
                         (InputKey.Number7, PacketRunAction.Key7),
                         (InputKey.Number8, PacketRunAction.Key8),
                         (InputKey.Number9, PacketRunAction.Key9),
                     })
            {
                yield return new KeyBinding(action.Item1, action.Item2);
            }
        }

        public override Drawable CreateIcon() => new OsuSpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Text = "PR",
        };

        public override ISkin? CreateSkinTransformer(ISkin skin, IBeatmap beatmap) => new PacketRunSkinTransformer(skin);

        public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;
    }
}
