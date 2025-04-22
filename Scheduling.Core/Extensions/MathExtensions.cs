using System.Numerics;

namespace Scheduling.Core.Extensions
{
    public static class MathExtensions
    {
        public static double Inverse(this double value) => (1.0).DividedBy(value);

        public static T DividedBy<T>(this T numerator, T denominator) where T: INumber<T>
        {
            if (denominator == default || numerator == default)
                return default;
            return numerator / denominator;
        }

        public static double DividedBy(this int numerator, double denominator) =>
            ((double)numerator).DividedBy(denominator);

        public static double CalculateRelativeError(this double approximateValue, double exactValue) =>
            Math.Abs(approximateValue - exactValue).DividedBy(exactValue) * 100;
    }
}
