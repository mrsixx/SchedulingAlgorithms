namespace Scheduling.Core.Extensions
{
    public static class MathExtensions
    {
        public static double Inverse(this double value) => (1.0).DividedBy(value);

        public static double DividedBy(this double numerator, double denominator)
        {
            if (denominator == 0 || numerator == 0)
                return 0;
            return numerator / denominator;
        }

        public static double CalculateRelativeError(this double approximateValue, double exactValue) =>
            Math.Abs(approximateValue - exactValue).DividedBy(exactValue) * 100;
    }
}
