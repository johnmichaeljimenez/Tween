using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace TweenLib.Tests;

public class EasingTests
{
    [Fact]
    public void LinearIsIdentity()
    {
        Assert.Equal(0.37f, Easing.Linear(0.37f), 4);
    }

    [Fact]
    public void QuadMidpoints()
    {
        Assert.Equal(0.25f, Easing.QuadIn(0.5f), 4);
        Assert.Equal(0.75f, Easing.QuadOut(0.5f), 4);
        Assert.Equal(0.5f, Easing.QuadInOut(0.5f), 4);
    }

    [Fact]
    public void EveryStandardEasingHitsEndpoints()
    {
        foreach (var method in StandardEasingMethods())
        {
            Assert.Equal(0f, Invoke(method, 0f), 3);
            Assert.Equal(1f, Invoke(method, 1f), 3);
        }
    }

    [Fact]
    public void EveryStandardEasingStaysFiniteInsideUnitInterval()
    {
        foreach (var method in StandardEasingMethods())
        {
            foreach (var probe in new[] { 0.1f, 0.37f, 0.5f, 0.83f, 0.99f })
            {
                var value = Invoke(method, probe);
                Assert.False(float.IsNaN(value), $"{method.Name}({probe}) is NaN");
                Assert.False(float.IsInfinity(value), $"{method.Name}({probe}) is infinite");
            }
        }
    }

    [Fact]
    public void ShakeReturnsToZeroAtBothEnds()
    {
        var shake = Easing.Shake(5f, 2f);
        Assert.Equal(0f, shake(0f), 4);
        Assert.Equal(0f, shake(1f), 4);
    }

    [Fact]
    public void ParabolaPeaksAtMidpointAndReturnsToZero()
    {
        var parabola = Easing.Parabola(3f);
        Assert.Equal(3f, parabola(0.5f), 4);
        Assert.Equal(0f, parabola(0f), 4);
        Assert.Equal(0f, parabola(1f), 4);
    }

    private static IEnumerable<MethodInfo> StandardEasingMethods()
        => typeof(Easing).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.ReturnType == typeof(float)
                && m.GetParameters() is { Length: 1 } parameters
                && parameters[0].ParameterType == typeof(float));

    private static float Invoke(MethodInfo method, float t)
        => (float)method.Invoke(null, new object[] { t })!;
}