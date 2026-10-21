using System;

namespace Tween;

public delegate float EasingFunction(float normalizedTime);

public static class Easing
{
    private const float BackOvershoot = 1.70158f;
    private const float BackOvershootDouble = 2.5949095f;
    private const float ElasticPeriod = 0.3f;
    private const float BounceGravity = 7.5625f;
    private const float BounceThreshold = 2.75f;

    public static float Linear(float t) => t;

    public static float QuadIn(float t) => t * t;

    public static float QuadOut(float t) => t * (2f - t);

    public static float QuadInOut(float t) => t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;

    public static float CubicIn(float t) => t * t * t;

    public static float CubicOut(float t) => (t - 1f) * (t - 1f) * (t - 1f) + 1f;

    public static float CubicInOut(float t)
    {
        if (t < 0.5f)
            return 4f * t * t * t;
        var u = 2f * t - 2f;
        return 0.5f * u * u * u + 1f;
    }

    public static float QuartIn(float t) => t * t * t * t;

    public static float QuartOut(float t) => 1f - (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f);

    public static float QuartInOut(float t)
    {
        if (t < 0.5f)
            return 8f * t * t * t * t;
        var u = t - 1f;
        return 1f - 8f * u * u * u * u;
    }

    public static float QuintIn(float t) => t * t * t * t * t;

    public static float QuintOut(float t) => (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f) + 1f;

    public static float QuintInOut(float t)
    {
        if (t < 0.5f)
            return 16f * t * t * t * t * t;
        var u = 2f * t - 2f;
        return 0.5f * u * u * u * u * u + 1f;
    }

    public static float SineIn(float t) => 1f - MathF.Cos(t * MathF.PI * 0.5f);

    public static float SineOut(float t) => MathF.Sin(t * MathF.PI * 0.5f);

    public static float SineInOut(float t) => -(MathF.Cos(MathF.PI * t) - 1f) * 0.5f;

    public static float ExpoIn(float t) => t <= 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);

    public static float ExpoOut(float t) => t >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);

    public static float ExpoInOut(float t)
    {
        if (t <= 0f)
            return 0f;
        if (t >= 1f)
            return 1f;
        if (t < 0.5f)
            return 0.5f * MathF.Pow(2f, 20f * t - 10f);
        return 1f - 0.5f * MathF.Pow(2f, -20f * t + 10f);
    }

    public static float CircIn(float t) => 1f - MathF.Sqrt(1f - t * t);

    public static float CircOut(float t) => MathF.Sqrt(1f - (t - 1f) * (t - 1f));

    public static float CircInOut(float t)
    {
        if (t < 0.5f)
            return 0.5f * (1f - MathF.Sqrt(1f - 4f * t * t));
        var u = -2f * t + 2f;
        return 0.5f * (MathF.Sqrt(1f - u * u) + 1f);
    }

    public static float BackIn(float t)
    {
        var u = BackOvershoot + 1f;
        return u * t * t * t - BackOvershoot * t * t;
    }

    public static float BackOut(float t)
    {
        var u = t - 1f;
        return 1f + (BackOvershoot + 1f) * u * u * u + BackOvershoot * u * u;
    }

    public static float BackInOut(float t)
    {
        if (t < 0.5f)
        {
            var u = 2f * t;
            return 0.5f * u * u * ((BackOvershootDouble + 1f) * u - BackOvershootDouble);
        }
        var v = 2f * t - 2f;
        return 0.5f * (v * v * ((BackOvershootDouble + 1f) * v + BackOvershootDouble) + 2f);
    }

    public static float ElasticIn(float t)
    {
        if (t <= 0f)
            return 0f;
        if (t >= 1f)
            return 1f;
        return -MathF.Pow(2f, 10f * t - 10f) * MathF.Sin((t * 10f - 10.75f) * (2f * MathF.PI / ElasticPeriod));
    }

    public static float ElasticOut(float t)
    {
        if (t <= 0f)
            return 0f;
        if (t >= 1f)
            return 1f;
        return MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * (2f * MathF.PI / ElasticPeriod)) + 1f;
    }

    public static float ElasticInOut(float t)
    {
        if (t <= 0f)
            return 0f;
        if (t >= 1f)
            return 1f;
        var period = ElasticPeriod * 1.5f;
        if (t < 0.5f)
            return -0.5f * MathF.Pow(2f, 20f * t - 10f) * MathF.Sin((20f * t - 11.125f) * (2f * MathF.PI / period));
        return MathF.Pow(2f, -20f * t + 10f) * MathF.Sin((20f * t - 11.125f) * (2f * MathF.PI / period)) * 0.5f + 1f;
    }

    public static float BounceIn(float t) => 1f - BounceOut(1f - t);

    public static float BounceOut(float t)
    {
        if (t < 1f / BounceThreshold)
            return BounceGravity * t * t;
        if (t < 2f / BounceThreshold)
        {
            var u = t - 1.5f / BounceThreshold;
            return BounceGravity * u * u + 0.75f;
        }
        if (t < 2.5f / BounceThreshold)
        {
            var u = t - 2.25f / BounceThreshold;
            return BounceGravity * u * u + 0.9375f;
        }
        var v = t - 2.625f / BounceThreshold;
        return BounceGravity * v * v + 0.984375f;
    }

    public static float BounceInOut(float t) => t < 0.5f
        ? (1f - BounceOut(1f - 2f * t)) * 0.5f
        : (1f + BounceOut(2f * t - 1f)) * 0.5f;

    public static EasingFunction Parabola(float height) => t => -4f * height * (t - 0.5f) * (t - 0.5f) + height;

    public static EasingFunction Shake(float frequency, float amplitude) => t =>
    {
        if (t <= 0f || t >= 1f)
            return 0f;
        float envelope = (1f - t) * (4f * t * (1f - t));
        return (float)(Random.Shared.NextDouble() * 2.0 - 1.0) * amplitude * envelope * frequency;
    };
}