// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.PacketRun.Screens.Components;

namespace osu.Game.Rulesets.PacketRun.Editor.Components
{
    public partial class EditorBeatToolbar : FillFlowContainer
    {
        public EditorBeatToolbar(PacketRunChartDocument document, Action onChanged)
        {
            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;
            Spacing = new osuTK.Vector2(8, 0);

            Add(new PacketRunMenuButton($"Beat Lines: {(document.ViewSettings.ShowBeatLines ? "On" : "Off")}", () =>
            {
                document.ViewSettings.ShowBeatLines = !document.ViewSettings.ShowBeatLines;
                onChanged();
            })
            { Size = new osuTK.Vector2(160, 36) });

            Add(new PacketRunMenuButton($"Minor: {(document.ViewSettings.ShowMinorBeatLines ? "On" : "Off")}", () =>
            {
                document.ViewSettings.ShowMinorBeatLines = !document.ViewSettings.ShowMinorBeatLines;
                onChanged();
            })
            { Size = new osuTK.Vector2(120, 36) });

            Add(new PacketRunMenuButton($"Vertical: {(document.ViewSettings.ShowVerticalDigits ? "On" : "Off")}", () =>
            {
                document.ViewSettings.ShowVerticalDigits = !document.ViewSettings.ShowVerticalDigits;
                onChanged();
            })
            { Size = new osuTK.Vector2(120, 36) });

            Add(new PacketRunMenuButton($"Snap: {(document.ViewSettings.SnapEnabled ? "On" : "Off")}", () =>
            {
                document.ViewSettings.SnapEnabled = !document.ViewSettings.SnapEnabled;
                onChanged();
            })
            { Size = new osuTK.Vector2(100, 36) });

            Add(new PacketRunMenuButton($"Div 1/{document.ViewSettings.SnapDivisor}", cycleDivisor)
            { Size = new osuTK.Vector2(80, 36) });

            void cycleDivisor()
            {
                document.ViewSettings.SnapDivisor = document.ViewSettings.SnapDivisor switch
                {
                    1 => 2,
                    2 => 4,
                    4 => 8,
                    _ => 1,
                };
                onChanged();
            }
        }
    }
}
