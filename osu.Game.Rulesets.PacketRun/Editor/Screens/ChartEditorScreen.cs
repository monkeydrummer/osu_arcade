// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.PacketRun;
using osu.Game.Rulesets.PacketRun.Charts;
using osu.Game.Rulesets.PacketRun.Editor.Components;
using osu.Game.Rulesets.PacketRun.Screens.Components;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Editor.Screens
{
    public partial class ChartEditorScreen : PacketRunEditorScreen
    {
        public override string Title => "Chart Editor";

        private PacketRunChartDocument document = null!;
        private EditorClock clock = null!;
        private EditorPlaybackHost playbackHost = null!;
        private ChartTimeline timeline = null!;
        private OsuTextBox timeBox = null!;
        private OsuTextBox digitsBox = null!;
        private OsuSpriteText statusText = null!;

        private DrawableEditorPacket? selectedPacket;
        private PacketRunChartPacket? selectedChartPacket;
        private int? selectedDigitIndex;
        private readonly PacketRunEditorClipboard clipboard = new PacketRunEditorClipboard();
        private Container toolbarHost = null!;
        private ChartEditorDigitInputHandler digitInputHandler = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            document = RequireDocument();
            playbackHost = new EditorPlaybackHost(document);
            AddPlaybackHost(playbackHost);
            clock = playbackHost.EditorClock;

            timeline = new ChartTimeline(document, clock)
            {
                RelativeSizeAxes = Axes.X,
                Height = 200,
            };

            timeline.SeekRequested += time =>
            {
                clock.Seek(time);
            };

            digitInputHandler = new ChartEditorDigitInputHandler(TryHandleDigitPressed);

            AddEditorContent(
                digitInputHandler,
                new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(6, 6, 14, 255) },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(20),
                    Spacing = new Vector2(0, 8),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "Chart Editor",
                            Font = OsuFont.Torus.With(size: 28, weight: FontWeight.Bold),
                            Colour = new Color4(80, 200, 255, 255),
                        },
                        toolbarHost = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        timeline,
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(8, 0),
                            Children = new Drawable[]
                            {
                                new PacketRunMenuButton("Play/Pause", togglePlayback) { Size = new Vector2(120, 36) },
                                new PacketRunMenuButton("Back", seekToBeginning) { Size = new Vector2(100, 36) },
                                new PacketRunMenuButton("Add", addPacket) { Size = new Vector2(100, 36) },
                                new PacketRunMenuButton("Delete", deleteSelection) { Size = new Vector2(100, 36) },
                                new PacketRunMenuButton("Save", saveChart) { Size = new Vector2(100, 36) },
                                new PacketRunMenuButton("Close", () => this.Exit()) { Size = new Vector2(100, 36) },
                            },
                        },
                        createLabel("Time (ms)"),
                        timeBox = createTextBox("0"),
                        createLabel("Digits"),
                        digitsBox = createDigitsTextBox(),
                        statusText = new OsuSpriteText
                        {
                            Font = OsuFont.Torus.With(size: 14),
                            Colour = Color4.White,
                        },
                    },
                });

            timeBox.OnCommit += (_, _) => applyInspector();
            digitsBox.OnCommit += (_, _) => applyInspector();

            wireTimelineEvents();
            rebuildToolbar();
            updateInspector();
            updatePlaybackStatus();
        }

        protected override void Update()
        {
            base.Update();
            updatePlaybackStatus();
        }

        private void updatePlaybackStatus()
        {
            if (selectedChartPacket != null)
            {
                return;
            }

            if (!playbackHost.HasRealAudio)
            {
                string lookup = playbackHost.WorkingBeatmap.ResolvedAudioPath
                                ?? PacketRunAudioPathResolver.DescribeLookup(document.Chart.Metadata.AudioFile, document.SongDirectory, document.FilePath);

                statusText.Text = playbackHost.WorkingBeatmap.AudioLoadStatus == PacketRunAudioLoadStatus.TrackLoadFailed
                    ? $"Audio file found but could not be loaded: {lookup}"
                    : $"Audio not found: {lookup}. Put the audio in the song folder or enter its full path in Chart Setup.";
            }
            else if (clock.IsRunning)
            {
                statusText.Text = $"Playing: {clock.CurrentTimeAccurate:N0} ms";
            }
            else
            {
                statusText.Text = $"Paused: {clock.CurrentTimeAccurate:N0} ms";
            }
        }

        private void wireTimelineEvents()
        {
            timeline.RebuildPackets();
            afterTimelineRebuild();
        }

        private void afterTimelineRebuild()
        {
            foreach (var drawable in timeline.PacketDrawables)
            {
                bindPacketEvents(drawable);
            }

            restoreSelection();
        }

        private void restoreSelection()
        {
            if (selectedChartPacket == null)
            {
                return;
            }

            var drawable = timeline.PacketDrawables.FirstOrDefault(d => ReferenceEquals(d.Packet, selectedChartPacket));

            if (drawable == null)
            {
                selectedChartPacket = null;
                selectedPacket = null;
                selectedDigitIndex = null;
                updateInspector();
                return;
            }

            if (selectedPacket != null)
            {
                selectedPacket.SetSelected(false, null);
            }

            selectedPacket = drawable;
            drawable.SetSelected(true, selectedDigitIndex);
        }

        private void bindPacketEvents(DrawableEditorPacket drawable)
        {
            drawable.Selected += d => selectPacket(d, null);
            drawable.DigitSelected += (d, index) => selectPacket(d, index);
            drawable.TimeChanged += (d, time) =>
            {
                d.Packet.TimeMs = document.SnapTime(time);
                document.MarkDirty();
                updateInspector();
            };
        }

        private void selectPacket(DrawableEditorPacket drawable, int? digitIndex)
        {
            if (selectedPacket != null)
            {
                selectedPacket.SetSelected(false, null);
            }

            selectedPacket = drawable;
            selectedChartPacket = drawable.Packet;
            selectedDigitIndex = digitIndex;
            drawable.SetSelected(true, digitIndex);
            updateInspector();
            scheduleDigitsBoxFocus();
        }

        private void scheduleDigitsBoxFocus()
        {
            // Defer until after the click completes so mouse-up does not steal focus from the text box.
            Scheduler.AddOnce(() =>
            {
                if (selectedChartPacket == null)
                {
                    return;
                }

                GetContainingFocusManager()?.ChangeFocus(digitsBox);
                digitsBox.SelectAll();
            });
        }

        private void updateInspector()
        {
            if (selectedChartPacket == null)
            {
                timeBox.Text = string.Empty;
                digitsBox.Text = string.Empty;
                updatePlaybackStatus();
                return;
            }

            timeBox.Text = selectedChartPacket.TimeMs.ToString("0");
            digitsBox.Text = string.Join(string.Empty, selectedChartPacket.Digits);
            statusText.Text = selectedDigitIndex == null
                ? $"Selected packet at {selectedChartPacket.TimeMs:N0} ms"
                : $"Selected digit {selectedDigitIndex} in packet";
        }

        private void applyInspector()
        {
            if (selectedChartPacket == null)
            {
                return;
            }

            if (double.TryParse(timeBox.Text, out double time))
            {
                selectedChartPacket.TimeMs = document.SnapTime(time);
            }

            if (digitsBox.Text.Length > 0 && digitsBox.Text.All(char.IsDigit))
            {
                selectedChartPacket.Digits = digitsBox.Text.Select(c => c - '0').ToArray();
                selectedChartPacket.Variant = PacketRunVariantMatcher.TryInfer(selectedChartPacket.Digits);
            }

            document.SortPackets();
            document.MarkDirty();
            timeline.RebuildPackets();
            afterTimelineRebuild();
            updateInspector();

            GetContainingFocusManager()?.ChangeFocus(null);
        }

        private bool TryHandleDigitPressed(int digit)
        {
            if (selectedChartPacket == null)
            {
                return false;
            }

            if (selectedDigitIndex != null)
            {
                var digits = selectedChartPacket.Digits.ToArray();
                digits[selectedDigitIndex.Value] = digit;
                selectedChartPacket.Digits = digits;
            }
            else
            {
                selectedChartPacket.Digits = digitsBox.Text.Where(char.IsDigit).Select(c => c - '0').Append(digit).ToArray();
            }

            selectedChartPacket.Variant = PacketRunVariantMatcher.TryInfer(selectedChartPacket.Digits);
            document.MarkDirty();
            timeline.RebuildPackets();
            afterTimelineRebuild();
            updateInspector();

            return true;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == osuTK.Input.Key.Space && !e.Repeat && !isEditingText())
            {
                togglePlayback();
                return true;
            }

            if (e.ControlPressed)
            {
                switch (e.Key)
                {
                    case osuTK.Input.Key.C:
                        copySelection();
                        return true;

                    case osuTK.Input.Key.V:
                        pasteSelection();
                        return true;

                    case osuTK.Input.Key.X:
                        copySelection();
                        deleteSelection();
                        return true;

                    case osuTK.Input.Key.S:
                        saveChart();
                        return true;
                }
            }

            if (e.Key == osuTK.Input.Key.Enter)
            {
                applyInspector();
                return true;
            }

            return base.OnKeyDown(e);
        }

        private void copySelection()
        {
            if (selectedChartPacket == null)
            {
                return;
            }

            clipboard.Copy(new[] { selectedChartPacket });
            statusText.Text = "Copied packet.";
        }

        private void pasteSelection()
        {
            if (!clipboard.HasContent)
            {
                return;
            }

            double baseTime = selectedChartPacket?.TimeMs ?? clock.CurrentTime;
            var clones = clipboard.GetClonedPackets();

            foreach (var clone in clones)
            {
                clone.TimeMs = document.SnapTime(baseTime);
                document.Chart.Packets.Add(clone);
            }

            document.SortPackets();
            document.MarkDirty();
            timeline.RebuildPackets();
            afterTimelineRebuild();

            statusText.Text = "Pasted packet(s).";
        }

        private void addPacket()
        {
            int[] digits = { 1, 1, 1 };
            var packet = new PacketRunChartPacket
            {
                TimeMs = document.SnapTime(clock.CurrentTimeAccurate),
                Digits = digits,
                Variant = PacketRunVariantMatcher.TryInfer(digits),
            };

            document.AddPacket(packet);
            timeline.RebuildPackets();
            afterTimelineRebuild();

            var drawable = timeline.PacketDrawables.FirstOrDefault(d => ReferenceEquals(d.Packet, packet));

            if (drawable != null)
            {
                selectPacket(drawable, null);
            }

            statusText.Text = $"Added packet at {packet.TimeMs:N0} ms";
        }

        private void deleteSelection()
        {
            if (selectedChartPacket == null)
            {
                return;
            }

            document.RemovePackets(new[] { selectedChartPacket });
            selectedPacket = null;
            selectedChartPacket = null;
            selectedDigitIndex = null;
            timeline.RebuildPackets();
            afterTimelineRebuild();
            updateInspector();
        }

        private void saveChart()
        {
            applyInspector();

            try
            {
                document.Save();
                statusText.Text = "Saved.";
            }
            catch (Exception ex)
            {
                statusText.Text = ex.Message;
            }
        }

        private void togglePlayback()
        {
            if (clock.IsRunning)
            {
                clock.Stop();
            }
            else
            {
                clock.Start();
            }
        }

        private void seekToBeginning()
        {
            clock.Stop();
            clock.Seek(0);
            timeline.SeekToChartTime(0);
            updatePlaybackStatus();
        }

        private bool isEditingText() => timeBox.HasFocus || digitsBox.HasFocus;

        private void rebuildToolbar()
        {
            toolbarHost.Clear();
            toolbarHost.Add(new EditorBeatToolbar(document, () =>
            {
                rebuildToolbar();
                timeline.RefreshPacketLayout();
            }));
        }

        private static OsuSpriteText createLabel(string text) => new OsuSpriteText
        {
            Text = text,
            Font = OsuFont.Torus.With(size: 14),
            Colour = new Color4(180, 180, 200, 255),
        };

        private static OsuTextBox createTextBox(string initial) => new OsuTextBox
        {
            Width = 300,
            Text = initial,
        };

        private static OsuTextBox createDigitsTextBox() => new OsuTextBox
        {
            Width = 300,
            Text = string.Empty,
            SelectAllOnFocus = true,
        };

        private partial class ChartEditorDigitInputHandler : Drawable, IKeyBindingHandler<PacketRunAction>
        {
            private readonly Func<int, bool> onDigitPressed;

            public ChartEditorDigitInputHandler(Func<int, bool> onDigitPressed)
            {
                this.onDigitPressed = onDigitPressed;
            }

            public bool OnPressed(KeyBindingPressEvent<PacketRunAction> e)
            {
                int? digit = actionToDigit(e.Action);

                if (digit == null)
                {
                    return false;
                }

                return onDigitPressed(digit.Value);
            }

            public void OnReleased(KeyBindingReleaseEvent<PacketRunAction> e)
            {
            }

            private static int? actionToDigit(PacketRunAction action)
            {
                int digit = (int)action;

                if (digit >= 0 && digit <= 9)
                {
                    return digit;
                }

                return null;
            }
        }
    }
}
