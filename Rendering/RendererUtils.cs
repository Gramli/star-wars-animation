using System;

namespace StarWarsAnimation.Rendering
{
    public static class RendererUtils
    {
        public static float CalculateWidthScale(float heightScale)
        {
            float compressed = 0.4f + (heightScale * 0.6f);
            return Math.Clamp(compressed, 0.4f, 1.0f);
        }

        public static int ScaleOffset(int value, float scale)
        {
            if (value == 0)
            {
                return 0;
            }

            int scaled = (int)Math.Round(value * scale);
            if (scaled == 0)
            {
                return value > 0 ? 1 : -1;
            }

            return scaled;
        }

        public static (int x, int y) GetShakeOffset(bool shake)
        {
            if (!shake)
            {
                return (0, 0);
            }

            var rng = new Random();
            return (rng.Next(-1, 2), rng.Next(-1, 2));
        }
    }
}
