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
using osu.Game.Rulesets.PacketRun;
using osu.Game.Rulesets.PacketRun.Charts;
using osu.Game.Rulesets.PacketRun.Editor.Components;
using osu.Game.Rulesets.PacketRun.Screens.Components;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.Editor.Screens
{
    public partial class BpmCalibrationScreen : PacketRunEditorScreen
    {
        public override string Title => "BPM Utility";

        private PacketRunChartDocument document = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            document = RequireDocument();

            var playback = new EditorPlaybackHost(document);
            AddPlaybackHost(playback);

            InternalChildren = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(6, 6, 14, 255) },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(40),
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 16),
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Child = new BpmCalibrationPanel(document, playback.EditorClock),
                            },
                            new PacketRunMenuButton("Back", () => this.Exit()) { Size = new Vector2(120, 40) },
                        },
                    },
                },
            };
        }
    }

    public partial class RecordScreen : PacketRunEditorScreen
    {
        public override string Title => "Record";

        private PacketRunChartDocument document = null!;
        private EditorClock clock = null!;
        private readonly List<PacketRunChartPacket> sessionPackets = new List<PacketRunChartPacket>();
        private readonly List<int> digitBuffer = new List<int>();
        private double? bufferStartTime;
        private FillFlowContainer packetList = null!;
        private OsuSpriteText statusText = null!;
        private Container beatOverlayHost = null!;
        private RecordDigitInputHandler digitInputHandler = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            document = RequireDocument();

            var playback = new EditorPlaybackHost(document);
            AddPlaybackHost(playback);
            clock = playback.EditorClock;

            digitInputHandler = new RecordDigitInputHandler(HandleDigitPressed);

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
                            Text = "Recording",
                            Font = OsuFont.Torus.With(size: 28, weight: FontWeight.Bold),
                            Colour = new Color4(255, 160, 60, 255),
                        },
                        statusText = new OsuSpriteText
                        {
                            Font = OsuFont.Torus.With(size: 16),
                            Colour = Color4.White,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(8, 0),
                            Children = new Drawable[]
                            {
                                new PacketRunMenuButton("Play/Pause", togglePlayback) { Size = new Vector2(120, 36) },
                                new PacketRunMenuButton("Undo Last", undoLast) { Size = new Vector2(120, 36) },
                                new PacketRunMenuButton("Finish", finish) { Size = new Vector2(120, 36) },
                                new PacketRunMenuButton("Back", () => this.Exit()) { Size = new Vector2(100, 36) },
                            },
                        },
                        beatOverlayHost = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 80,
                        },
                        packetList = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 4),
                        },
                    },
                });

            rebuildToolbar();
            rebuildBeatOverlay();
            updateStatus();
        }

        private void HandleDigitPressed(int digit)
        {
            if (bufferStartTime == null)
            {
                bufferStartTime = clock.CurrentTime;
            }

            digitBuffer.Add(digit);
            updateStatus();

            if (digitBuffer.Count >= document.SetupSettings.DigitsPerPacket)
            {
                commitBuffer();
            }
        }

        private void commitBuffer()
        {
            if (bufferStartTime == null || digitBuffer.Count == 0)
            {
                return;
            }

            var packet = new PacketRunChartPacket
            {
                TimeMs = document.SnapTime(bufferStartTime.Value),
                Digits = digitBuffer.ToArray(),
                Variant = PacketRunVariantMatcher.TryInfer(digitBuffer.ToArray()),
            };

            sessionPackets.Add(packet);
            digitBuffer.Clear();
            bufferStartTime = null;
            refreshPacketList();
            updateStatus();
        }

        private void undoLast()
        {
            if (sessionPackets.Count > 0)
            {
                sessionPackets.RemoveAt(sessionPackets.Count - 1);
                refreshPacketList();
            }
        }

        private void finish()
        {
            document.AddPackets(sessionPackets);
            sessionPackets.Clear();
            this.Exit();
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

        private void refreshPacketList()
        {
            packetList.Clear();

            foreach (var packet in sessionPackets)
            {
                packetList.Add(new OsuSpriteText
                {
                    Text = $"{packet.TimeMs:N0} ms: [{string.Join(", ", packet.Digits)}]",
                    Font = OsuFont.Torus.With(size: 14),
                    Colour = Color4.White,
                });
            }
        }

        private void rebuildToolbar()
        {
            // Toolbar is rebuilt inline in beat overlay row
        }

        private void rebuildBeatOverlay()
        {
            beatOverlayHost.Clear();

            var overlay = new BeatGridOverlay(document)
            {
                RelativeSizeAxes = Axes.Both,
                VisibleStartMs = 0,
                VisibleEndMs = clock.TrackLength,
            };

            beatOverlayHost.Add(overlay);
            beatOverlayHost.Add(new EditorBeatToolbar(document, rebuildBeatOverlay)
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            });
        }

        private void updateStatus()
        {
            string buffer = digitBuffer.Count > 0 ? string.Join(string.Empty, digitBuffer) : "-";
            statusText.Text = $"Time: {clock.CurrentTime:N0} ms | Buffer: {buffer} | Recorded: {sessionPackets.Count}";
        }

        protected override void Update()
        {
            base.Update();
            updateStatus();
        }

        private partial class RecordDigitInputHandler : Drawable, IKeyBindingHandler<PacketRunAction>
        {
            private readonly Action<int> onDigitPressed;

            public RecordDigitInputHandler(Action<int> onDigitPressed)
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

                onDigitPressed(digit.Value);
                return true;
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
