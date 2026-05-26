// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Rulesets.PacketRun.Objects.Drawables;
using osu.Game.Rulesets.PacketRun.Replays;
using osu.Game.Rulesets.PacketRun.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.PacketRun.UI
{
    [Cached]
    public partial class DrawablePacketRunRuleset : DrawableScrollingRuleset<PacketHitObject>
    {
        [Cached]
        public PacketGameplayProcessor Processor { get; private set; } = null!;

        public new PacketRunInputManager KeyBindingInputManager => (PacketRunInputManager)base.KeyBindingInputManager;

        public DrawablePacketRunRuleset(PacketRunRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod>? mods)
            : base(ruleset, beatmap, mods)
        {
            Direction.Value = ScrollingDirection.Left;
            TimeRange.Value = 4000;
            Processor = new PacketGameplayProcessor(PacketRunBeatmap.From(Beatmap));
        }

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            if (scoreProcessor is PacketRunScoreProcessor packetScoreProcessor)
            {
                Processor.ScoreProcessor = packetScoreProcessor;
            }

            AddInternal(new NumpadOverlay { Depth = -1 });
        }

        protected override void Update()
        {
            base.Update();
            Processor.UpdateMode(Clock.CurrentTime);
        }

        protected override Playfield CreatePlayfield() => new PacketRunPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new PacketRunFramedReplayInputHandler(replay);

        public override DrawableHitObject<PacketHitObject> CreateDrawableRepresentation(PacketHitObject h) => new DrawablePacketHitObject(h);

        protected override PassThroughInputManager CreateInputManager() => new PacketRunInputManager(Ruleset?.RulesetInfo, Variant);
    }
}
