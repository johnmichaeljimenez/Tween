using System;

namespace Tween
{
    public static class Easing
    {
        private static Random random = new Random();

        public static float Linear(TweenBase t) => t.NormalizedTime;

        public static float QuadIn(TweenBase t) => t.NormalizedTime * t.NormalizedTime;

        public static float QuadOut(TweenBase t) => t.NormalizedTime * (2 - t.NormalizedTime);

        public static float QuadInOut(TweenBase t)
        {
            var n = t.NormalizedTime;

            if (n < 0.5f) return 2 * n * n;
            return -1 + (4 - 2 * n) * n;
        }

        public static float QuadInOutLoop(TweenBase t)
        {
            var n = t.NormalizedTime;

            if (n < 0.5f)
            {
                return 4 * n * n;
            }
            else
            {
                float t2 = (n - 0.5f) * 2;
                return (1 - t2 * (2 - t2));
            }
        }

        public static float RandomShake(TweenBase t)
        {
            var n = t.NormalizedTime;

            //p1 = frequency
            //p2 = amplitude

            if (n >= 1f) return 0f;
            float envelope = (1 - n) * (4 * n * (1 - n));
            float noise = (float)(random.NextDouble() * 2 - 1);
            return noise * t.Parameter2 * envelope * t.Parameter1;
        }

        // public static float RandomShakeFull(TweenBase t)
        // {
        //     return RandomShake(frequency: 5f, amplitude: 2f)(t);
        // }

        public static float ParabolicUp(TweenBase t)
        {
            //p1 = peak height
            return -4 * t.Parameter1 * (t.NormalizedTime - 0.5f) * (t.NormalizedTime - 0.5f) + t.Parameter1;
        }
    }
}