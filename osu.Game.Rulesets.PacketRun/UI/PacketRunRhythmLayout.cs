// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.PacketRun.UI.Components;

namespace osu.Game.Rulesets.PacketRun.UI
{
    public static class PacketRunRhythmLayout
    {
        public const float HitLineX = 0.20f;

        public const float SpawnX = 0.80f;

        public const double ApproachDuration = 12000;

        public const double TimeRange = 14000;

        /// <summary>
        /// Computes the horizontal scroll position for a rhythm object at the given time-to-hit offset.
        /// </summary>
        public static float GetScrollX(double timeToHit, float drawWidth, out bool visible)
        {
            float spawnX = drawWidth * SpawnX;
            float visualHitX = drawWidth * HitLineX;
            float hitX = visualHitX - DrawablePacket.DIGIT_SIZE / 2f;
            float travel = spawnX - hitX;

            if (timeToHit > ApproachDuration)
            {
                visible = false;
                return spawnX;
            }

            visible = true;

            if (timeToHit > 0)
            {
                float progress = 1f - (float)(timeToHit / ApproachDuration);
                return spawnX - travel * progress;
            }

            float pastProgress = (float)(-timeToHit / ApproachDuration);
            return hitX - travel * pastProgress;
        }
    }
}
