using System;
using Xunit;

namespace TweenLib.Tests;

public class TweenTests
{
    private float _value;

    private Tween<float> CreateTween(float to = 10f, float duration = 1f)
        => new(() => _value, v => _value = v, to, duration);

    [Fact]
    public void ProgressesLinearlyTowardTarget()
    {
        var tween = CreateTween();
        tween.Update(0.5f, 0.5f);
        Assert.Equal(5f, _value, 4);
        tween.Update(0.5f, 0.5f);
        Assert.Equal(10f, _value, 4);
        Assert.True(tween.IsComplete);
    }

    [Fact]
    public void CustomFromIsAppliedImmediately()
    {
        var tween = CreateTween(to: 0f).SetFrom(100f);
        Assert.Equal(100f, _value, 4);

        tween.Update(0.5f, 0.5f);
        Assert.Equal(50f, _value, 4);
    }

    [Fact]
    public void EasingShapesTheValue()
    {
        var tween = CreateTween().SetEasing(Easing.QuadIn);
        tween.Update(0.5f, 0.5f);
        Assert.Equal(2.5f, _value, 4);
    }

    [Fact]
    public void StepsQuantizeProgress()
    {
        var tween = CreateTween().SetSteps(2);
        tween.Update(0.25f, 0.25f);
        Assert.Equal(0f, _value, 4);
        tween.Update(0.35f, 0.35f);
        Assert.Equal(5f, _value, 4);
    }

    [Fact]
    public void DelayHoldsValueThenAppliesLeftoverDelta()
    {
        var tween = CreateTween().SetDelay(0.5f);
        tween.Update(0.4f, 0.4f);
        Assert.Equal(0f, _value, 4);

        tween.Update(0.2f, 0.2f);
        Assert.Equal(1f, _value, 4);
    }

    [Fact]
    public void CallbacksFireExpectedNumberOfTimes()
    {
        int starts = 0, updates = 0, completes = 0;
        var tween = CreateTween()
            .OnStart(() => starts++)
            .OnUpdate(() => updates++)
            .OnComplete(() => completes++);

        tween.Update(0.25f, 0.25f);
        tween.Update(0.25f, 0.25f);
        tween.Update(0.5f, 0.5f);

        Assert.Equal(1, starts);
        Assert.Equal(3, updates);
        Assert.Equal(1, completes);
    }

    [Fact]
    public void YoyoLoopsPingPong()
    {
        var tween = CreateTween().SetLoops(2, yoyo: true);

        tween.Update(1f, 1f);
        Assert.False(tween.IsComplete);
        Assert.Equal(10f, _value, 4);

        tween.Update(0.5f, 0.5f);
        Assert.Equal(5f, _value, 4);

        tween.Update(0.5f, 0.5f);
        Assert.True(tween.IsComplete);
        Assert.Equal(0f, _value, 4);
    }

    [Fact]
    public void InfiniteLoopsNeverComplete()
    {
        var tween = CreateTween().SetLoops(-1, yoyo: true);
        for (int i = 0; i < 10; i++)
            tween.Update(0.5f, 0.5f);
        Assert.False(tween.IsComplete);
    }

    [Fact]
    public void ReversePlaysBackwardContinuously()
    {
        var tween = CreateTween();
        tween.Update(0.5f, 0.5f);
        Assert.Equal(5f, _value, 4);

        tween.Reverse();
        tween.Update(0.25f, 0.25f);
        Assert.Equal(2.5f, _value, 4);

        tween.Reverse();
        tween.Update(0.25f, 0.25f);
        Assert.Equal(5f, _value, 4);
    }

    [Fact]
    public void ReverseBeforeStartPlaysFromTarget()
    {
        var tween = CreateTween();
        tween.Reverse();

        tween.Update(0.5f, 0.5f);
        Assert.Equal(10f, tween.FromValue, 4);
        Assert.Equal(5f, _value, 4);
    }

    [Fact]
    public void PauseFreezesAndResumeContinues()
    {
        var tween = CreateTween();
        tween.Update(0.5f, 0.5f);

        tween.Pause();
        tween.Update(0.5f, 0.5f);
        Assert.Equal(5f, _value, 4);

        tween.Resume();
        tween.Update(0.5f, 0.5f);
        Assert.Equal(10f, _value, 4);
    }

    [Fact]
    public void KillFiresOnKillOnceAndFreezes()
    {
        int kills = 0;
        var tween = CreateTween().OnKill(() => kills++);

        tween.Update(0.5f, 0.5f);
        tween.Kill();
        tween.Kill();
        Assert.Equal(1, kills);

        tween.Update(0.5f, 0.5f);
        Assert.Equal(5f, _value, 4);
        Assert.True(tween.IsKilled);
    }

    [Fact]
    public void UsesUnscaledTimeWhenRequested()
    {
        var tween = CreateTween();
        tween.UseUnscaledTime = true;
        tween.Update(1f, 0.5f);
        Assert.Equal(5f, _value, 4);
    }

    [Fact]
    public void ZeroDurationCompletesImmediately()
    {
        var tween = CreateTween(duration: 0f);
        tween.Update(0.016f, 0.016f);
        Assert.True(tween.IsComplete);
        Assert.Equal(10f, _value, 4);
    }

    [Fact]
    public void IntLerperRoundsToNearest()
    {
        var intValue = 0;
        var tween = new Tween<int>(() => intValue, v => intValue = v, 10, 1f);
        tween.Update(0.26f, 0.26f);
        Assert.Equal(3, intValue);
    }

    [Fact]
    public void RejectsInvalidArguments()
    {
        Assert.Throws<ArgumentNullException>(() => new Tween<float>(null!, v => _value = v, 1f, 1f));
        Assert.Throws<ArgumentNullException>(() => new Tween<float>(() => _value, null!, 1f, 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateTween(duration: -1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateTween().SetDelay(-1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateTween().SetSteps(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateTween().SetLoops(0));
        Assert.Throws<ArgumentNullException>(() => CreateTween().SetEasing(null!));
    }

    [Fact]
    public void MissingLerperThrowsWithGuidance()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Tween<DateTime>(() => DateTime.MinValue, _ => { }, DateTime.MaxValue, 1f));
        Assert.Contains("Lerper.Register", ex.Message);
    }

    [Fact]
    public void CustomLerperCanBeRegistered()
    {
        Lerper.Register<(int, int)>((a, b, t) =>
            ((int)(a.Item1 + (b.Item1 - a.Item1) * t), (int)(a.Item2 + (b.Item2 - a.Item2) * t)));

        var pair = (0, 0);
        var tween = new Tween<(int, int)>(() => pair, v => pair = v, (10, 20), 1f);
        tween.Update(0.5f, 0.5f);
        Assert.Equal((5, 10), pair);
    }
}