// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.PacketRun.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.PacketRun.UI.Components
{
    public partial class DrawablePacket : CompositeDrawable
    {
        public const float DIGIT_SIZE = 36f;
        public const float DIGIT_SPACING = 4f;
        public const float VERTICAL_STAGGER = 14f;

        private readonly PacketHitObject hitObject;
        private readonly PacketLayout layout;

        private readonly Container digitsContainer;
        private int progressIndex;
        private bool active;
        private bool enteredCorrectly = true;

        public DrawablePacket(PacketHitObject hitObject, PacketLayout layout)
        {
            this.hitObject = hitObject;
            this.layout = layout;

            AutoSizeAxes = Axes.Both;

            InternalChild = digitsContainer = new Container { AutoSizeAxes = Axes.Both };

            rebuild();
        }

        public void RefreshProgress(int progress, bool correctSoFar)
        {
            progressIndex = progress;
            enteredCorrectly = correctSoFar;
            rebuild();
        }

        public void SetActive(bool isActive, int progress)
        {
            active = isActive;
            progressIndex = progress;
            rebuild();
        }

        public void FlashWrong()
        {
            this.FlashColour(Color4.Red, 200);
            this.ScaleTo(1.08f).ScaleTo(1, 150, Easing.Out);
        }

        public void FlashCorrectDigit(int digitIndex)
        {
            if (digitIndex < 0 || digitIndex >= digitsContainer.Count)
            {
                return;
            }

            digitsContainer[digitIndex].FlashColour(new Color4(80, 255, 120, 255), 150);
        }

        private void rebuild()
        {
            digitsContainer.Clear();

            for (int i = 0; i < hitObject.Digits.Length; i++)
            {
                int digit = hitObject.Digits[i];
                bool completed = i < progressIndex;
                bool current = active && i == progressIndex;

                var box = createDigitBox(digit, completed, current);
                Vector2 position = getDigitPosition(i);
                box.Position = position;
                digitsContainer.Add(box);
            }
        }

        private Vector2 getDigitPosition(int index)
        {
            if (layout == PacketLayout.Vertical)
            {
                return new Vector2(index * VERTICAL_STAGGER, index * (DIGIT_SIZE + DIGIT_SPACING));
            }

            return new Vector2(index * (DIGIT_SIZE + DIGIT_SPACING), 0);
        }

        private Drawable createDigitBox(int digit, bool completed, bool current)
        {
            Color4 glow = PacketRunColours.ForDigit(digit);
            float alpha = completed ? 0.45f : 1f;

            return new Container
            {
                Size = new Vector2(DIGIT_SIZE),
                Alpha = alpha,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = glow.Darken(0.6f),
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = glow,
                        Alpha = current ? 0.9f : 0.35f,
                        Margin = new MarginPadding(2),
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = digit.ToString(),
                        Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold),
                        Colour = Color4.White,
                    },
                },
            };
        }
    }

    public static class PacketRunColours
    {
        public static Color4 ForDigit(int digit) => digit switch
        {
            0 => new Color4(80, 220, 120, 255),
            >= 1 and <= 3 => new Color4(255, 80, 180, 255),
            >= 4 and <= 6 => new Color4(255, 160, 60, 255),
            >= 7 and <= 9 => new Color4(80, 200, 255, 255),
            _ => Color4.White,
        };
    }
}
