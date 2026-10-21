using System;
using Xunit;

namespace TweenLib.Tests;

public class LerperTests
{
    [Fact]
    public void NumericPrimitivesAreRegisteredByDefault()
    {
        Assert.True(Lerper.IsRegistered<float>());
        Assert.True(Lerper.IsRegistered<double>());
        Assert.True(Lerper.IsRegistered<decimal>());
        Assert.True(Lerper.IsRegistered<int>());
        Assert.True(Lerper.IsRegistered<uint>());
        Assert.True(Lerper.IsRegistered<long>());
        Assert.True(Lerper.IsRegistered<ulong>());
        Assert.True(Lerper.IsRegistered<short>());
        Assert.True(Lerper.IsRegistered<ushort>());
        Assert.True(Lerper.IsRegistered<byte>());
        Assert.True(Lerper.IsRegistered<sbyte>());
    }

    [Fact]
    public void GetForUnregisteredTypeThrows()
    {
        Assert.Throws<ArgumentException>(() => Lerper.Get<DateTime>());
    }

    [Fact]
    public void RegisteredLerperOverwritesPreviousOne()
    {
        Lerper.Register<float>((from, to, t) => to);
        try
        {
            var lerper = Lerper.Get<float>();
            Assert.Equal(10f, lerper(0f, 10f, 0.5f), 4);
        }
        finally
        {
            Lerper.Register<float>((from, to, t) => from + (to - from) * t);
        }
    }
}