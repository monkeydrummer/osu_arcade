// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.PacketRun.Charts;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Rulesets.PacketRun.Screens.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Editor.Screens
{
    public partial class ChartSetupScreen : PacketRunEditorScreen
    {
        public override string Title => "Chart Setup";

        private PacketRunChartDocument document = null!;
        private OsuTextBox titleBox = null!;
        private OsuTextBox artistBox = null!;
        private OsuTextBox audioBox = null!;
        private OsuTextBox bpmBox = null!;
        private OsuTextBox digitsBox = null!;
        private OsuSpriteText timeSigText = null!;
        private OsuSpriteText statusText = null!;
        private OsuSpriteText modeText = null!;
        private OsuSpriteText layoutText = null!;

        private static readonly PacketRunTimeSignature[] time_signature_presets =
        {
            new PacketRunTimeSignature(4, 4),
            new PacketRunTimeSignature(3, 4),
            new PacketRunTimeSignature(2, 4),
            new PacketRunTimeSignature(6, 8),
            new PacketRunTimeSignature(9, 8),
            new PacketRunTimeSignature(12, 8),
        };

        private int timeSignaturePresetIndex;

        [BackgroundDependencyLoader]
        private void load()
        {
            document = RequireDocument();
            timeSignaturePresetIndex = Array.FindIndex(time_signature_presets, s => s.Equals(document.SetupSettings.TimeSignature));

            if (timeSignaturePresetIndex < 0)
            {
                timeSignaturePresetIndex = 0;
            }

            InternalChildren = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(6, 6, 14, 255) },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(40),
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "Chart Setup",
                                Font = OsuFont.Torus.With(size: 32, weight: FontWeight.Bold),
                                Colour = new Color4(80, 220, 120, 255),
                            },
                            createLabel("Title"),
                            titleBox = createTextBox(document.Chart.Metadata.Title),
                            createLabel("Artist"),
                            artistBox = createTextBox(document.Chart.Metadata.Artist),
                            createLabel("Audio file (filename in song folder, or full path)"),
                            audioBox = createTextBox(document.Chart.Metadata.AudioFile),
                            createLabel("BPM"),
                            bpmBox = createTextBox(document.GetPrimaryTiming().Bpm.ToString("0.##")),
                            createLabel("Digits per packet"),
                            digitsBox = createTextBox(document.SetupSettings.DigitsPerPacket.ToString()),
                            createLabel("Time signature"),
                            timeSigText = new OsuSpriteText
                            {
                                Font = OsuFont.Torus.With(size: 18),
                                Colour = Color4.White,
                            },
                            new PacketRunMenuButton("Cycle Time Signature", cycleTimeSignature) { Size = new Vector2(220, 36) },
                            modeText = new OsuSpriteText
                            {
                                Font = OsuFont.Torus.With(size: 16),
                                Colour = Color4.White,
                            },
                            new PacketRunMenuButton("Cycle Mode", cycleMode) { Size = new Vector2(180, 36) },
                            layoutText = new OsuSpriteText
                            {
                                Font = OsuFont.Torus.With(size: 16),
                                Colour = Color4.White,
                            },
                            new PacketRunMenuButton("Cycle Layout", cycleLayout) { Size = new Vector2(180, 36) },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(12, 0),
                                Margin = new MarginPadding { Top = 12 },
                                Children = new Drawable[]
                                {
                                    new PacketRunMenuButton("BPM Utility", () => openEditorScreen(new BpmCalibrationScreen())) { Size = new Vector2(140, 40) },
                                    new PacketRunMenuButton("Record", () => openEditorScreen(new RecordScreen())) { Size = new Vector2(120, 40) },
                                    new PacketRunMenuButton("Edit Chart", () => openEditorScreen(new ChartEditorScreen())) { Size = new Vector2(140, 40) },
                                    new PacketRunMenuButton("Save", saveChart) { Size = new Vector2(100, 40) },
                                    new PacketRunMenuButton("Save As", saveChartAs) { Size = new Vector2(120, 40) },
                                    new PacketRunMenuButton("Back", () => this.Exit()) { Size = new Vector2(100, 40) },
                                },
                            },
                            statusText = new OsuSpriteText
                            {
                                Font = OsuFont.Torus.With(size: 16),
                                Colour = new Color4(255, 200, 80, 255),
                            },
                        },
                    },
                },
            };

            refreshButtons();
            updateStatus();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            refreshTimingFields();
        }

        private void refreshTimingFields()
        {
            bpmBox.Text = document.GetPrimaryTiming().Bpm.ToString("0.##");
            timeSignaturePresetIndex = Array.FindIndex(time_signature_presets, s => s.Equals(document.SetupSettings.TimeSignature));

            if (timeSignaturePresetIndex < 0)
            {
                timeSignaturePresetIndex = 0;
            }

            refreshButtons();
        }

        private void syncFromFields()
        {
            document.Chart.Metadata.Title = titleBox.Text;
            document.Chart.Metadata.Artist = artistBox.Text;
            document.Chart.Metadata.AudioFile = audioBox.Text.Trim();

            if (int.TryParse(digitsBox.Text, out int digits) && digits > 0)
            {
                document.SetupSettings.DigitsPerPacket = digits;
            }

            if (double.TryParse(bpmBox.Text, out double bpm) && bpm > 0)
            {
                document.GetPrimaryTiming().Bpm = bpm;
            }

            document.SetupSettings.TimeSignature = time_signature_presets[timeSignaturePresetIndex];
            document.SyncMetadataFromSetup();
        }

        private void saveChart()
        {
            syncFromFields();

            try
            {
                document.Save();
                updateStatus("Saved.");
            }
            catch (Exception ex)
            {
                updateStatus(ex.Message);
            }
        }

        private void saveChartAs()
        {
            syncFromFields();

            try
            {
                document.SaveAs(document.FilePath ?? System.IO.Path.Combine(document.SongDirectory, "chart.packet.json"));
                updateStatus("Saved.");
            }
            catch (Exception ex)
            {
                updateStatus(ex.Message);
            }
        }

        private void cycleTimeSignature()
        {
            timeSignaturePresetIndex = (timeSignaturePresetIndex + 1) % time_signature_presets.Length;
            document.SetupSettings.TimeSignature = time_signature_presets[timeSignaturePresetIndex];
            refreshButtons();
        }

        private void cycleMode()
        {
            document.SetupSettings.DefaultMode = document.SetupSettings.DefaultMode == PacketGameplayMode.Queue
                ? PacketGameplayMode.Rhythm
                : PacketGameplayMode.Queue;
            refreshButtons();
        }

        private void cycleLayout()
        {
            document.SetupSettings.DefaultLayout = document.SetupSettings.DefaultLayout == PacketLayout.Horizontal
                ? PacketLayout.Vertical
                : PacketLayout.Horizontal;
            refreshButtons();
        }

        private void refreshButtons()
        {
            timeSigText.Text = document.SetupSettings.TimeSignature.ToString();
            modeText.Text = $"Mode: {document.SetupSettings.DefaultMode}";
            layoutText.Text = $"Layout: {document.SetupSettings.DefaultLayout}";
        }

        private void updateStatus(string? text = null)
        {
            string dirty = document.IsDirty ? " (unsaved changes)" : string.Empty;
            statusText.Text = text ?? $"File: {document.FilePath ?? "not saved"}{dirty}";
        }

        private void openEditorScreen(PacketRunEditorScreen screen)
        {
            syncFromFields();
            this.Push(screen);
        }

        private static OsuSpriteText createLabel(string text) => new OsuSpriteText
        {
            Text = text,
            Font = OsuFont.Torus.With(size: 14),
            Colour = new Color4(180, 180, 200, 255),
        };

        private static OsuTextBox createTextBox(string initial) => new OsuTextBox
        {
            Width = 400,
            Text = initial ?? string.Empty,
        };

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.ControlPressed && e.Key == osuTK.Input.Key.S)
            {
                saveChart();
                return true;
            }

            return base.OnKeyDown(e);
        }
    }
}
