// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.PacketRun.Charts
{
    public readonly struct PacketRunTimeSignature : IEquatable<PacketRunTimeSignature>
    {
        public int Numerator { get; }

        public int Denominator { get; }

        public PacketRunTimeSignature(int numerator, int denominator)
        {
            if (numerator < 1 || numerator > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(numerator));
            }

            if (denominator is not (2 or 4 or 8 or 16))
            {
                throw new ArgumentOutOfRangeException(nameof(denominator));
            }

            Numerator = numerator;
            Denominator = denominator;
        }

        public static PacketRunTimeSignature CommonFourFour => new PacketRunTimeSignature(4, 4);

        public static bool TryParse(string text, out PacketRunTimeSignature signature)
        {
            signature = default;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var parts = text.Split('/');

            if (parts.Length != 2)
            {
                return false;
            }

            if (!int.TryParse(parts[0].Trim(), out int numerator) || !int.TryParse(parts[1].Trim(), out int denominator))
            {
                return false;
            }

            try
            {
                signature = new PacketRunTimeSignature(numerator, denominator);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public static PacketRunTimeSignature FromTimingPoint(PacketRunChartTimingPoint timing) =>
            new PacketRunTimeSignature(timing.Meter, timing.MeterDenominator <= 0 ? 4 : timing.MeterDenominator);

        public void ApplyTo(PacketRunChartTimingPoint timing)
        {
            timing.Meter = Numerator;
            timing.MeterDenominator = Denominator;
        }

        public override string ToString() => $"{Numerator}/{Denominator}";

        public bool Equals(PacketRunTimeSignature other) => Numerator == other.Numerator && Denominator == other.Denominator;

        public override bool Equals(object? obj) => obj is PacketRunTimeSignature other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
    }
}
