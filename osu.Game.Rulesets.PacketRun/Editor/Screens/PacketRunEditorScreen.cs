// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.PacketRun;
using osu.Game.Rulesets.PacketRun.Editor;
using osu.Game.Rulesets.PacketRun.Editor.Components;
using osu.Game.Screens;

namespace osu.Game.Rulesets.PacketRun.Editor.Screens
{
    public abstract partial class PacketRunEditorScreen : OsuScreen
    {
        public override bool ShowFooter => false;

        public override bool HideOverlaysOnEnter => true;

        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.Disabled;

        public override bool AllowUserExit => true;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Resolved]
        protected PacketRunEditorSession Session { get; private set; } = null!;

        private PacketRunEditorInputManager editorInput = null!;
        private readonly List<EditorPlaybackHost> playbackHosts = new List<EditorPlaybackHost>();

        [BackgroundDependencyLoader]
        private void loadEditorInput(RulesetStore rulesets)
        {
            var rulesetInfo = rulesets.GetRuleset(PacketRunRuleset.SHORT_NAME)
                              ?? throw new InvalidOperationException("Packet Run ruleset is not available.");

            AddInternal(editorInput = new PacketRunEditorInputManager(rulesetInfo));
        }

        /// <summary>
        /// Adds drawables inside the editor key-binding input manager so digit keys reach packet key handlers.
        /// </summary>
        protected void AddEditorContent(params Drawable[] drawables)
        {
            editorInput.AddRange(drawables);
        }

        protected void AddPlaybackHost(EditorPlaybackHost host)
        {
            playbackHosts.Add(host);
            AddInternal(host);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            foreach (var host in playbackHosts)
            {
                host.StopPlayback();
            }

            return base.OnExiting(e);
        }

        protected PacketRunChartDocument RequireDocument()
        {
            if (Session.Document == null)
            {
                this.Exit();
                throw new System.InvalidOperationException("No chart document in session.");
            }

            return Session.Document;
        }

        protected static string GetContentRoot() => PacketRunEditorContent.GetContentRoot();
    }
}
