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
        private sealed class RhythmPacketState
        {
            public int ProgressIndex;
            public bool EnteredCorrectly = true;
            public int MissesInPacket;
            public HitResult? FirstDigitResult;
        }

        private readonly Queue<DrawablePacketHitObject> waitingPackets = new Queue<DrawablePacketHitObject>();
        private readonly Dictionary<DrawablePacketHitObject, RhythmPacketState> rhythmPackets = new Dictionary<DrawablePacketHitObject, RhythmPacketState>();
        private DrawablePacketHitObject? activePacket;
        private int progressIndex;
        private bool packetEnteredCorrectly = true;

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
            var mode = drawable.HitObject.ModeOverride ?? Beatmap.GetModeAt(drawable.HitObject.StartTime);

            if (mode == PacketGameplayMode.Queue)
            {
                waitingPackets.Enqueue(drawable);
                tryActivateNext();
            }
            else
            {
                rhythmPackets[drawable] = new RhythmPacketState();
            }
        }

        public void UpdateMode(double currentTime)
        {
            CurrentMode.Value = Beatmap.GetModeAt(currentTime);
        }

        public void UpdateRhythmVisuals(double currentTime)
        {
            foreach (var (drawable, state) in rhythmPackets)
            {
                if (!drawable.IsAlive)
                {
                    continue;
                }

                bool inWindow = isRhythmInputWindow(drawable, state, currentTime);
                drawable.SetActive(inWindow, state.ProgressIndex);
            }
        }

        public void NotifyRhythmExpired(DrawablePacketHitObject drawable)
        {
            if (!rhythmPackets.Remove(drawable))
            {
                return;
            }

            drawable.SetActive(false, 0);

            if (!drawable.Result.HasResult)
            {
                drawable.ApplyCustomResult(HitResult.Miss);
            }

            drawable.MarkComplete();
        }

        public void HandleDigitInput(int digit, double currentTime)
        {
            if (tryGetRhythmTarget(currentTime, out var rhythmTarget, out var rhythmState))
            {
                handleRhythmDigit(rhythmTarget, rhythmState, digit, currentTime);
                return;
            }

            if (activePacket == null)
            {
                return;
            }

            handleQueueDigit(activePacket, ref progressIndex, ref packetEnteredCorrectly, digit, currentTime);
        }

        private void handleQueueDigit(
            DrawablePacketHitObject target,
            ref int targetProgress,
            ref bool enteredCorrectly,
            int digit,
            double currentTime)
        {
            var hitObject = target.HitObject;

            if (targetProgress >= hitObject.Digits.Length)
            {
                return;
            }

            int expected = hitObject.Digits[targetProgress];

            if (digit != expected)
            {
                handleWrongDigit(target, ref enteredCorrectly);
                return;
            }

            targetProgress++;

            if (targetProgress >= hitObject.Digits.Length)
            {
                completeQueuePacket(enteredCorrectly);
            }
            else
            {
                target.RefreshProgress(targetProgress, enteredCorrectly);
            }
        }

        private void handleRhythmDigit(DrawablePacketHitObject target, RhythmPacketState state, int digit, double currentTime)
        {
            var hitObject = target.HitObject;

            if (state.ProgressIndex >= hitObject.Digits.Length)
            {
                return;
            }

            int expected = hitObject.Digits[state.ProgressIndex];

            if (digit != expected)
            {
                ScoreProcessor?.RegisterHeat();
                ScoreProcessor?.RegisterSignal(false);
                state.EnteredCorrectly = false;
                target.FlashWrong();
                state.MissesInPacket++;
                state.ProgressIndex++;

                if (state.ProgressIndex >= hitObject.Digits.Length)
                {
                    completeRhythmPacket(target, state);
                }
                else
                {
                    target.RefreshProgress(state.ProgressIndex, state.EnteredCorrectly);
                }

                return;
            }

            if (state.ProgressIndex == 0)
            {
                double timeOffset = currentTime - hitObject.StartTime;
                var result = hitObject.HitWindows?.ResultFor(timeOffset) ?? HitResult.Miss;

                if (result == HitResult.None)
                {
                    return;
                }

                state.FirstDigitResult = result;
            }

            state.ProgressIndex++;

            if (state.ProgressIndex >= hitObject.Digits.Length)
            {
                completeRhythmPacket(target, state);
            }
            else
            {
                target.RefreshProgress(state.ProgressIndex, state.EnteredCorrectly);
            }
        }

        private void handleWrongDigit(DrawablePacketHitObject target, ref bool enteredCorrectly)
        {
            ScoreProcessor?.RegisterHeat();
            ScoreProcessor?.RegisterSignal(false);
            enteredCorrectly = false;
            target.FlashWrong();
        }

        private void completeQueuePacket(bool enteredCorrectly)
        {
            if (activePacket != null && !activePacket.Result.HasResult)
            {
                activePacket.ApplyCustomResult(enteredCorrectly ? HitResult.Great : HitResult.Ok);
            }

            if (enteredCorrectly)
            {
                ScoreProcessor?.RegisterSignal(true);
            }

            activePacket?.MarkComplete();
            activePacket = null;
            progressIndex = 0;
            packetEnteredCorrectly = true;
            ActivePacketChanged?.Invoke();
            tryActivateNext();
        }

        private void completeRhythmPacket(DrawablePacketHitObject drawable, RhythmPacketState state)
        {
            if (!drawable.Result.HasResult)
            {
                drawable.ApplyCustomResult(getFinalResult(PacketGameplayMode.Rhythm, state.MissesInPacket, state.FirstDigitResult, state.EnteredCorrectly));
            }

            if (state.EnteredCorrectly && state.MissesInPacket == 0)
            {
                ScoreProcessor?.RegisterSignal(true);
            }

            rhythmPackets.Remove(drawable);
            drawable.SetActive(false, state.ProgressIndex);
            drawable.MarkComplete();
        }

        private HitResult getFinalResult(PacketGameplayMode mode, int missesInPacket, HitResult? firstDigitResult, bool enteredCorrectly)
        {
            if (mode == PacketGameplayMode.Rhythm)
            {
                if (firstDigitResult == HitResult.Miss)
                {
                    return HitResult.Miss;
                }

                if (missesInPacket >= PacketRunGameplayConstants.MaxWrongDigitsBeforePacketMiss)
                {
                    return HitResult.Miss;
                }

                if (missesInPacket > 0)
                {
                    return HitResult.Ok;
                }

                return firstDigitResult ?? HitResult.Great;
            }

            return enteredCorrectly ? HitResult.Great : HitResult.Ok;
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
                next.SetActive(true, progressIndex);
                ActivePacketChanged?.Invoke();
                return;
            }
        }

        private bool tryGetRhythmTarget(double currentTime, out DrawablePacketHitObject target, out RhythmPacketState state)
        {
            target = null!;
            state = null!;

            DrawablePacketHitObject? best = null;
            RhythmPacketState? bestState = null;
            double bestDelta = double.MaxValue;

            foreach (var pair in rhythmPackets)
            {
                var drawable = pair.Key;
                var packetState = pair.Value;

                if (!drawable.IsAlive || drawable.Result.HasResult)
                {
                    continue;
                }

                if (!isRhythmInputWindow(drawable, packetState, currentTime))
                {
                    continue;
                }

                double delta = Math.Abs(currentTime - drawable.HitObject.StartTime);

                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    best = drawable;
                    bestState = packetState;
                }
            }

            if (best == null || bestState == null)
            {
                return false;
            }

            target = best;
            state = bestState;
            return true;
        }

        private static bool isRhythmInputWindow(DrawablePacketHitObject drawable, RhythmPacketState state, double currentTime)
        {
            if (state.ProgressIndex >= drawable.HitObject.Digits.Length)
            {
                return false;
            }

            double timeOffset = currentTime - drawable.HitObject.StartTime;
            double missWindow = drawable.HitObject.HitWindows?.WindowFor(HitResult.Miss) ?? 188;

            if (state.ProgressIndex == 0)
            {
                return Math.Abs(timeOffset) <= missWindow;
            }

            return timeOffset <= missWindow * 4;
        }

        public DrawablePacketHitObject? Active => activePacket;

        public IEnumerable<DrawablePacketHitObject> QueuedPackets => waitingPackets;

        public IReadOnlyList<DrawablePacketHitObject> GetOrderedQueue()
        {
            var queue = new List<DrawablePacketHitObject>();

            if (activePacket != null)
            {
                queue.Add(activePacket);
            }

            queue.AddRange(waitingPackets);
            return queue;
        }
    }
}
