// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Platform;
using osu.Game;

namespace osu.Desktop.Editor
{
    public static class Program
    {
        private const string game_name = "PacketRunEditor";

        [STAThread]
        public static void Main(string[] args)
        {
            var hostOptions = new HostOptions
            {
                IPCPipeName = $"{game_name}-IPC",
                FriendlyGameName = "Packet Run Editor",
            };

            using DesktopGameHost host = Host.GetSuitableDesktopHost(game_name, hostOptions);
            host.Run(new OsuGameEditorDesktop(args));
        }
    }

    internal partial class OsuGameEditorDesktop : PacketRunEditorGame
    {
        public OsuGameEditorDesktop(string[]? args = null)
            : base(args)
        {
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            host.Window.Title = "Packet Run Editor";
        }
    }
}
