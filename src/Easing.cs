namespace SharpEngine
{
    public class Easing
    {
        public enum CalcType
        {
            Quadratic,
            Cubic,
            Quartic,
            Quintic,
            Sinusoidal,
            Exponential,
            Circular,
            None
        }

        private static double StartPoint;

        private static double EndPoint;

        private static double Diff;

        private static int TimeMs;

        private static CalcType Type;

        private static double CounterValue;

        private static double Value;

        public static double easeInQuart(double x)
        {
            return x * x * x * x;
        }

        public static double easeOutBounce(double x)
        {
            double n1 = 7.5625;
            double d1 = 2.75;
            if (x < 1.0 / d1)
            {
                return n1 * x * x;
            }
            if (x < 2.0 / d1)
            {
                return n1 * (x -= 1.5 / d1) * x + 0.75;
            }
            if (x < 2.5 / d1)
            {
                return n1 * (x -= 2.25 / d1) * x + 0.9375;
            }
            return n1 * (x -= 2.625 / d1) * x + 63.0 / 64.0;
        }

        public static double easeOutCubic(double x)
        {
            return 1.0 - Math.Pow(1.0 - x, 3.0);
        }

        public static double easeOutQuart(double x)
        {
            return 1.0 - Math.Pow(1.0 - x, 4.0);
        }

        public static double easeOutQuad(double x)
        {
            return 1.0 - (1.0 - x) * (1.0 - x);
        }

        public static double easeInQuad(double x)
        {
            return x * x;
        }

        public static double easeOutExpo(double x)
        {
            return (x == 1.0) ? 1.0 : (1.0 - Math.Pow(2.0, -10.0 * x));
        }

        public static double easeInSine(double x)
        {
            return 1.0 - Math.Cos(x * Math.PI / 2.0);
        }

        public static double easeOutSine(double x)
        {
            return Math.Sin(x * Math.PI / 2.0);
        }

        public static double easeInOutSine(double x)
        {
            return (0.0 - (Math.Cos(Math.PI * x) - 1.0)) / 2.0;
        }

        public static double easeOutCirc(double x)
        {
            return Math.Sqrt(1.0 - Math.Pow(x - 1.0, 2.0));
        }

        public static double easeInOutCirc(double x)
        {
            return (x < 0.5) ? ((1.0 - Math.Sqrt(1.0 - Math.Pow(2.0 * x, 2.0))) / 2.0) : ((Math.Sqrt(1.0 - Math.Pow(-2.0 * x + 2.0, 2.0)) + 1.0) / 2.0);
        }

        public static double Linear(double start, double end, double value)
        {
            return start + (end - start) * value;
        }

        public static double EaseInCubic(double start, double end, double value)
        {
            end -= start;
            return end * value * value * value + start;
        }

        public static double easeInCubic(double value)
        {
            return value * value * value;
        }

        public static double EaseInSine(double start, double end, double value)
        {
            end -= start;
            return (0.0 - end) * Math.Cos(value * (Math.PI / 2.0)) + end + start;
        }

        public static double EaseOutSine(double start, double end, double value)
        {
            end -= start;
            return end * Math.Sin(value * (Math.PI / 2.0)) + start;
        }

        public static int EaseIn(double countervalue, double counterend, double startPoint, double endPoint, CalcType type)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Diff = EndPoint - StartPoint;
            TimeMs = (int)counterend;
            Type = type;
            CounterValue = countervalue;
            switch (Type)
            {
                case CalcType.Quadratic:
                    CounterValue /= TimeMs;
                    Value = Diff * CounterValue * CounterValue + StartPoint;
                    break;
                case CalcType.Cubic:
                    CounterValue /= TimeMs;
                    Value = Diff * CounterValue * CounterValue * CounterValue + StartPoint;
                    break;
                case CalcType.Quartic:
                    CounterValue /= TimeMs;
                    Value = Diff * CounterValue * CounterValue * CounterValue * CounterValue + StartPoint;
                    break;
                case CalcType.Quintic:
                    CounterValue /= TimeMs;
                    Value = Diff * CounterValue * CounterValue * CounterValue * CounterValue * CounterValue + StartPoint;
                    break;
                case CalcType.Sinusoidal:
                    Value = (0.0 - Diff) * Math.Cos(CounterValue / (double)TimeMs * (Math.PI / 2.0)) + Diff + StartPoint;
                    break;
                case CalcType.Exponential:
                    Value = Diff * Math.Pow(2.0, 10.0 * (CounterValue / (double)TimeMs - 1.0)) + StartPoint;
                    break;
                case CalcType.Circular:
                    CounterValue /= TimeMs;
                    Value = (0.0 - Diff) * (Math.Sqrt(1.0 - CounterValue * CounterValue) - 1.0) + StartPoint;
                    break;
            }
            return (int)Value;
        }

        public static int EaseOut(double countervalue, double counterend, double startPoint, double endPoint, CalcType type)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Diff = EndPoint - StartPoint;
            TimeMs = (int)counterend;
            Type = type;
            CounterValue = countervalue;
            switch (Type)
            {
                case CalcType.Quadratic:
                    CounterValue /= TimeMs;
                    Value = (0.0 - Diff) * CounterValue * (CounterValue - 2.0) + StartPoint;
                    break;
                case CalcType.Cubic:
                    CounterValue /= TimeMs;
                    CounterValue -= 1.0;
                    Value = Diff * (CounterValue * CounterValue * CounterValue + 1.0) + StartPoint;
                    break;
                case CalcType.Quartic:
                    CounterValue /= TimeMs;
                    CounterValue -= 1.0;
                    Value = (0.0 - Diff) * (CounterValue * CounterValue * CounterValue * CounterValue - 1.0) + StartPoint;
                    break;
                case CalcType.Quintic:
                    CounterValue /= TimeMs;
                    CounterValue -= 1.0;
                    Value = Diff * (CounterValue * CounterValue * CounterValue * CounterValue * CounterValue + 1.0) + StartPoint;
                    break;
                case CalcType.Sinusoidal:
                    Value = Diff * Math.Sin(CounterValue / (double)TimeMs * (Math.PI / 2.0)) + StartPoint;
                    break;
                case CalcType.Exponential:
                    Value = Diff * (0.0 - Math.Pow(2.0, -10.0 * CounterValue / (double)TimeMs) + 1.0) + StartPoint;
                    break;
                case CalcType.Circular:
                    CounterValue /= TimeMs;
                    CounterValue -= 1.0;
                    Value = Diff * Math.Sqrt(1.0 - CounterValue * CounterValue) + StartPoint;
                    break;
                case CalcType.None:
                    Value = startPoint + countervalue / counterend * (endPoint - startPoint);
                    break;
            }
            return (int)Value;
        }
    }
}
