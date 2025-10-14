namespace Tween
{
    public static class Easing
    {
        private static Random random = new Random();

        public static float Linear(float t) => t;

        public static float QuadIn(float t) => t * t;

        public static float QuadOut(float t) => t * (2 - t);

        public static float QuadInOut(float t)
        {
            if (t < 0.5f) return 2 * t * t;
            return -1 + (4 - 2 * t) * t;
        }

        public static float QuadInOutLoop(float t)
        {
            if (t < 0.5f)
            {
                return 4 * t * t;
            }
            else
            {
                float t2 = (t - 0.5f) * 2;
                return (1 - t2 * (2 - t2));
            }
        }

        public static float RandomShake(float t)
        {
            if (t >= 1f) return 0f;

            float envelope = (1 - t) * (4 * t * (1 - t));
            float frequency = 10f;
            float amplitude = 1f;
            float noise = (float)(random.NextDouble() * 2 - 1);
            float shake = noise * amplitude * envelope;

            return shake;
        }

        public static float RandomShakeFull(float t)
        {
            if (t >= 1f) return 0f;

            float envelope = (1 - t) * (4 * t * (1 - t));
            float frequency = 10f;
            float amplitude = 2f;
            float noise = (float)(random.NextDouble() * 2 - 1);
            float shake = noise * amplitude * envelope;

            return shake;
        }

        public static float ParabolicUp(float t)
        {
            float peakHeight = 1.5f;
            return -4 * peakHeight * (t - 0.5f) * (t - 0.5f) + peakHeight;
        }
    }
}