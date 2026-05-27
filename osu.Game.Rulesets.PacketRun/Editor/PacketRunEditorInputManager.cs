// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.PacketRun.Editor
{
    /// <summary>
    /// Routes Packet Run digit key bindings to editor screens (record, chart editor, etc.).
    /// </summary>
    public partial class PacketRunEditorInputManager : PacketRunInputManager
    {
        public PacketRunEditorInputManager(RulesetInfo ruleset)
            : base(ruleset, 0)
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected override KeyBindingContainer<PacketRunAction> CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new EditorPacketRunKeyBindingContainer(ruleset);

        private partial class EditorPacketRunKeyBindingContainer : KeyBindingContainer<PacketRunAction>
        {
            private readonly IEnumerable<IKeyBinding> defaultKeyBindings;

            public EditorPacketRunKeyBindingContainer(RulesetInfo ruleset)
            {
                defaultKeyBindings = ruleset.CreateInstance().GetDefaultKeyBindings(variant: 0);
            }

            public override IEnumerable<IKeyBinding> DefaultKeyBindings => defaultKeyBindings;
        }
    }
}
