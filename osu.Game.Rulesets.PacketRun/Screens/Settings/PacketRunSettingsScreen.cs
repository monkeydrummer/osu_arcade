// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings.Sections;
using osu.Game.Overlays.Settings.Sections.Audio;
using osu.Game.Rulesets.PacketRun;
using osu.Game.Rulesets.PacketRun.Screens.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Screens.Settings
{
    public partial class PacketRunSettingsScreen : PacketRunScreen
    {
        public override string Title => "Settings";

        private readonly Bindable<string> activeSection = new Bindable<string>("audio");

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, AudioManager audio)
        {
            InternalChildren = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(6, 6, 14, 255) },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(30),
                    ColumnDimensions = new[] { new Dimension(GridSizeMode.Absolute, 180), new Dimension() },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            createSidebar(),
                            createSectionContent(config, audio),
                        },
                    },
                },
            };
        }

        private Drawable createSidebar()
        {
            var flow = new FillFlowContainer { RelativeSizeAxes = Axes.Y, Width = 180, Direction = FillDirection.Vertical, Spacing = new Vector2(0, 8) };

            flow.Add(createSidebarButton("Audio", "audio"));
            flow.Add(createSidebarButton("Video", "video"));
            flow.Add(createSidebarButton("Delay", "delay"));

            return flow;
        }

        private Drawable createSidebarButton(string label, string id) =>
            new PacketRunMenuButton(label, () => activeSection.Value = id) { Size = new Vector2(160, 36) };

        private Drawable createSectionContent(OsuConfigManager config, AudioManager audio)
        {
            var container = new Container { RelativeSizeAxes = Axes.Both };

            void refresh()
            {
                container.Clear();

                container.Add(activeSection.Value switch
                {
                    "video" => createVideoSection(config),
                    "delay" => new DelayCalibrationPanel(config),
                    _ => createAudioSection(config),
                });
            }

            activeSection.BindValueChanged(_ => refresh(), true);
            return container;
        }

        private Drawable createAudioSection(OsuConfigManager config)
        {
            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 16),
                Children = new Drawable[]
                {
                    sectionTitle("Audio"),
                    new AudioOffsetAdjustControl { Current = config.GetBindable<double>(OsuSetting.AudioOffset) },
                    labelValue("Master volume uses global osu! audio settings."),
                },
            };
        }

        private Drawable createVideoSection(OsuConfigManager config)
        {
            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 16),
                Children = new Drawable[]
                {
                    sectionTitle("Video"),
                    labelValue("Resolution, fullscreen, and frame limiter use the framework host configuration."),
                    labelValue("Resolution and display settings are available in the framework host configuration."),
                },
            };
        }

        private static OsuSpriteText sectionTitle(string text) => new OsuSpriteText
        {
            Text = text,
            Font = OsuFont.Torus.With(size: 28, weight: FontWeight.Bold),
            Colour = new Color4(80, 200, 255, 255),
        };

        private static OsuSpriteText labelValue(string text) => new OsuSpriteText
        {
            Text = text,
            Font = OsuFont.Torus.With(size: 16),
            Colour = Color4.White,
        };
    }

    public partial class DelayCalibrationPanel : CompositeDrawable, IKeyBindingHandler<PacketRunAction>
    {
        private readonly OsuConfigManager config;
        private readonly List<double> tapTimes = new List<double>();
        private ScheduledDelegate? finishSchedule;
        private ScheduledDelegate? metronomeSchedule;
        private Sample? metronomeSample;
        private double startTime;
        private bool running;
        private OsuSpriteText statusText = null!;
        private PacketRunMenuButton applyButton = null!;

        public DelayCalibrationPanel(OsuConfigManager config)
        {
            this.config = config;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 12),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = "Delay Calibration",
                        Font = OsuFont.Torus.With(size: 28, weight: FontWeight.Bold),
                        Colour = new Color4(80, 220, 120, 255),
                    },
                    statusText = new OsuSpriteText
                    {
                        Font = OsuFont.Torus.With(size: 16),
                        Colour = Color4.White,
                    },
                    new PacketRunMenuButton("Start Calibration", startCalibration),
                    applyButton = new PacketRunMenuButton("Apply Suggested Offset", applyOffset) { Alpha = 0.5f },
                },
            };

            updateStatus("Tap Numpad 0 to the metronome beat.");
            metronomeSample = audio.Samples.Get("UI/generic-hit");
        }

        private void startCalibration()
        {
            tapTimes.Clear();
            running = true;
            startTime = Time.Current;
            finishSchedule?.Cancel();
            metronomeSchedule?.Cancel();
            scheduleMetronomeTick();
            finishSchedule = Scheduler.AddDelayed(finishCalibration, 10000);
            updateStatus("Calibrating… tap 0 on each beat.");
        }

        private void scheduleMetronomeTick()
        {
            if (!running)
            {
                return;
            }

            playMetronomeTick();
            metronomeSchedule = Scheduler.AddDelayed(scheduleMetronomeTick, 500);
        }

        private void playMetronomeTick()
        {
            metronomeSample?.Play();
        }

        private void finishCalibration()
        {
            running = false;
            metronomeSchedule?.Cancel();
            metronomeSchedule = null;

            if (tapTimes.Count < 3)
            {
                updateStatus("Not enough taps. Try again.");
                return;
            }

            double beatInterval = 500;
            var offsets = new List<double>();

            for (int i = 0; i < tapTimes.Count; i++)
            {
                double expected = startTime + i * beatInterval;
                offsets.Add(tapTimes[i] - expected);
            }

            offsets.Sort();
            double median = offsets[offsets.Count / 2];
            suggestedOffset = median;
            applyButton.Alpha = 1;
            updateStatus($"Suggested offset: {median:N0} ms");
        }

        private double suggestedOffset;

        private void applyOffset()
        {
            config.SetValue(OsuSetting.AudioOffset, suggestedOffset);
            updateStatus($"Applied offset: {suggestedOffset:N0} ms");
        }

        private void updateStatus(string text) => statusText.Text = text;

        public bool OnPressed(KeyBindingPressEvent<PacketRunAction> e)
        {
            if (!running || e.Action != PacketRunAction.Key0)
            {
                return false;
            }

            tapTimes.Add(Time.Current);
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<PacketRunAction> e)
        {
        }
    }
}
