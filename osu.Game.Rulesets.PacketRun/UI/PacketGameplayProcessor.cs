// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Rulesets.PacketRun.Objects.Drawables;
using osu.Game.Rulesets.PacketRun.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.PacketRun.UI
{
    public class PacketGameplayProcessor
    {
        private readonly Queue<DrawablePacketHitObject> waitingPackets = new Queue<DrawablePacketHitObject>();
        private DrawablePacketHitObject? activePacket;
        private int progressIndex;
        private bool packetEnteredCorrectly = true;
        private int rhythmMissesInPacket;

        public PacketRunBeatmap Beatmap { get; }

        public PacketRunScoreProcessor? ScoreProcessor { get; set; }

        public Bindable<PacketGameplayMode> CurrentMode { get; } = new Bindable<PacketGameplayMode>();

        public event Action? ActivePacketChanged;

        public PacketGameplayProcessor(PacketRunBeatmap beatmap)
        {
            Beatmap = beatmap;
        }

        public void RegisterPacket(DrawablePacketHitObject drawable)
        {
            waitingPackets.Enqueue(drawable);
            tryActivateNext();
        }

        public void UpdateMode(double currentTime)
        {
            CurrentMode.Value = Beatmap.GetModeAt(currentTime);
        }

        public void HandleDigitInput(int digit, double currentTime)
        {
            if (activePacket == null)
            {
                return;
            }

            var hitObject = activePacket.HitObject;
            var mode = hitObject.ModeOverride ?? CurrentMode.Value;

            if (progressIndex >= hitObject.Digits.Length)
            {
                return;
            }

            int expected = hitObject.Digits[progressIndex];

            if (digit != expected)
            {
                handleWrongDigit(mode);
                return;
            }

            if (mode == PacketGameplayMode.Rhythm && progressIndex == 0)
            {
                double timeOffset = currentTime - hitObject.StartTime;
                var result = hitObject.HitWindows?.ResultFor(timeOffset) ?? HitResult.Miss;

                if (result == HitResult.None)
                {
                    return;
                }

                activePacket.ApplyCustomResult(result);
            }
            else if (mode == PacketGameplayMode.Queue && progressIndex == 0)
            {
                activePacket.ApplyCustomResult(HitResult.Great);
            }
            else
            {
                activePacket.ApplyCustomResult(HitResult.Great);
            }

            progressIndex++;

            if (progressIndex >= hitObject.Digits.Length)
            {
                completeActivePacket();
            }
            else
            {
                activePacket.RefreshProgress(progressIndex, packetEnteredCorrectly);
            }
        }

        private void handleWrongDigit(PacketGameplayMode mode)
        {
            ScoreProcessor?.RegisterHeat();
            ScoreProcessor?.RegisterSignal(false);
            packetEnteredCorrectly = false;
            activePacket?.FlashWrong();

            if (mode == PacketGameplayMode.Queue)
            {
                return;
            }

            rhythmMissesInPacket++;
            activePacket?.ApplyCustomResult(HitResult.Miss);
            progressIndex++;

            if (progressIndex >= activePacket!.HitObject.Digits.Length)
            {
                completeActivePacket();
            }
            else
            {
                activePacket.RefreshProgress(progressIndex, packetEnteredCorrectly);
            }
        }

        private void completeActivePacket()
        {
            if (packetEnteredCorrectly && rhythmMissesInPacket == 0)
            {
                ScoreProcessor?.RegisterSignal(true);
            }

            activePacket?.MarkComplete();
            activePacket = null;
            progressIndex = 0;
            packetEnteredCorrectly = true;
            rhythmMissesInPacket = 0;
            ActivePacketChanged?.Invoke();
            tryActivateNext();
        }

        private void tryActivateNext()
        {
            if (activePacket != null)
            {
                return;
            }

            while (waitingPackets.Count > 0)
            {
                var next = waitingPackets.Dequeue();

                if (!next.IsAlive)
                {
                    continue;
                }

                activePacket = next;
                progressIndex = 0;
                packetEnteredCorrectly = true;
                rhythmMissesInPacket = 0;
                next.SetActive(true, progressIndex);
                ActivePacketChanged?.Invoke();
                return;
            }
        }

        public DrawablePacketHitObject? Active => activePacket;

        public IEnumerable<DrawablePacketHitObject> QueuedPackets => waitingPackets;
    }
}
