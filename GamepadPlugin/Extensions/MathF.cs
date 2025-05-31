using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamepadPlugin.Extensions
{
    class MathF
    {
        public static float Abs(float value)
        {
            if (value < 0)
            {
                return -value;
            }
            return value;
        }

        public static float Round(float value, int digits)
        {
            if (digits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(digits), "Number of digits must be non-negative.");
            }
            float factor = (float)Math.Pow(10, digits);
            return (float)Math.Round(value * factor) / factor;
        }
    }
}
