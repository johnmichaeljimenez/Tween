using System;
using Xunit;

namespace Tween.Tests;

public class TweenManagerTests : IDisposable
{
    private float _value;

    public TweenManagerTests() => TweenManager.KillAll();

    public void Dispose() => TweenManager.KillAll();

    [Fact]
    public void FactoryTweenAutoRegistersAndUpdates()
    {
        var tween = Tween.To(() => _value, v => _value = v, 10f, 1f);
        Assert.Equal(1, TweenManager.ActiveCount);

        TweenManager.Update(0.5f, 0.5f);
        Assert.Equal(5f, _value, 4);

        TweenManager.Update(0.6f, 0.6f);
        Assert.True(tween.IsComplete);
        Assert.Equal(0, TweenManager.ActiveCount);
    }

    [Fact]
    public void AddingSameIdKillsPreviousTween()
    {
        bool oldKilled = false;
        var old = Tween.To(() => _value, v => _value = v, 10f, 1f).SetId("move").OnKill(() => oldKilled = true);
        var newest = Tween.To(() => _value, v => _value = v, 10f, 1f).SetId("move");

        Assert.True(oldKilled);
        Assert.Same(newest, TweenManager.Find("move"));
    }

    [Fact]
    public void KillByIdAndByPrefix()
    {
        Tween.To(() => _value, v => _value = v, 1f, 1f).SetId("ui.fade");
        Tween.To(() => _value, v => _value = v, 1f, 1f).SetId("ui.slide");
        Tween.To(() => _value, v => _value = v, 1f, 1f).SetId("world.move");
        Assert.Equal(3, TweenManager.ActiveCount);

        Assert.Equal(2, TweenManager.KillByPrefix("ui."));
        Assert.True(TweenManager.Kill("world.move"));
        Assert.Equal(0, TweenManager.ActiveCount);
    }

    [Fact]
    public void PauseAllAndResumeAll()
    {
        Tween.To(() => _value, v => _value = v, 10f, 1f);

        TweenManager.PauseAll();
        TweenManager.Update(0.5f, 0.5f);
        Assert.Equal(0f, _value, 4);

        TweenManager.ResumeAll();
        TweenManager.Update(0.5f, 0.5f);
        Assert.Equal(5f, _value, 4);
    }

    [Fact]
    public void RemoveDetachesWithoutKilling()
    {
        bool killed = false;
        var tween = Tween.To(() => _value, v => _value = v, 10f, 1f).OnKill(() => killed = true);

        Assert.True(TweenManager.Remove(tween));
        Assert.False(killed);
        Assert.False(tween.IsKilled);
        Assert.Equal(0, TweenManager.ActiveCount);
    }

    [Fact]
    public void KillAllClearsEverything()
    {
        Tween.To(() => _value, v => _value = v, 1f, 1f);
        Tween.To(() => _value, v => _value = v, 1f, 1f).SetId("x");
        Tween.Sequence();

        TweenManager.KillAll();
        Assert.Equal(0, TweenManager.ActiveCount);
        Assert.Null(TweenManager.Find("x"));
    }

    [Fact]
    public void AppendingManagedTweenToSequenceDetachesIt()
    {
        var child = Tween.To(() => _value, v => _value = v, 10f, 1f);
        Assert.Equal(1, TweenManager.ActiveCount);

        var sequence = Tween.Sequence().Append(child);
        Assert.Equal(1, TweenManager.ActiveCount);

        TweenManager.Update(0.5f, 0.5f);
        Assert.Equal(5f, _value, 4);

        sequence.Kill();
        Assert.Equal(0, TweenManager.ActiveCount);
    }

    [Fact]
    public void AddingKilledTweenThrows()
    {
        var tween = Tween.To(() => _value, v => _value = v, 10f, 1f);
        tween.Kill();
        Assert.Throws<ArgumentException>(() => TweenManager.Add(tween));
    }
}