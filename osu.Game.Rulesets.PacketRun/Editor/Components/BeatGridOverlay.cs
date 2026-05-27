// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.PacketRun.Editor;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Editor.Components
{
    public partial class BeatGridOverlay : CompositeDrawable
    {
        private readonly PacketRunChartDocument document;
        private readonly Container linesContainer;

        public float PixelsPerMs { get; set; } = 0.1f;

        public double VisibleStartMs { get; set; }

        public double VisibleEndMs { get; set; } = 60000;

        public BeatGridOverlay(PacketRunChartDocument document)
        {
            this.document = document;
            RelativeSizeAxes = Axes.Both;

            InternalChild = linesContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        protected override void Update()
        {
            base.Update();
            linesContainer.Clear();

            if (!document.ViewSettings.ShowBeatLines)
            {
                return;
            }

            var beatGrid = document.CreateBeatGrid();
            bool showMinor = document.ViewSettings.ShowMinorBeatLines;

            foreach (double time in beatGrid.GetBeatTimes(VisibleStartMs, VisibleEndMs, majorOnly: !showMinor))
            {
                bool major = beatGrid.IsMajorBeat(time);
                float x = (float)(time * PixelsPerMs);

                linesContainer.Add(new Box
                {
                    Position = new Vector2(x, 0),
                    Size = new Vector2(major ? 2 : 1, DrawHeight),
                    Colour = major ? new Color4(120, 180, 255, 180) : new Color4(80, 80, 120, 100),
                });
            }
        }
    }
}
