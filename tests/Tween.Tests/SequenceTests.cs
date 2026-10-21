using System;
using Xunit;

namespace Tween.Tests;

public class SequenceTests
{
    private float _a;
    private float _b;

    private Tween<float> TweenA(float to = 10f, float duration = 1f)
        => new(() => _a, v => _a = v, to, duration);

    private Tween<float> TweenB(float to = 100f, float duration = 1f)
        => new(() => _b, v => _b = v, to, duration);

    [Fact]
    public void RunsChildrenInOrder()
    {
        var sequence = new Sequence().Append(TweenA()).Append(TweenB());

        sequence.Update(0.5f, 0.5f);
        Assert.Equal(5f, _a, 4);
        Assert.Equal(0f, _b, 4);

        sequence.Update(0.6f, 0.6f);
        Assert.Equal(10f, _a, 4);
        Assert.Equal(0f, _b, 4);

        sequence.Update(0.4f, 0.4f);
        Assert.Equal(40f, _b, 4);
    }

    [Fact]
    public void CompletesOnTheFrameTheLastChildFinishes()
    {
        var sequence = new Sequence().Append(TweenA()).Append(TweenB());
        sequence.Update(0.5f, 0.5f);
        sequence.Update(0.6f, 0.6f);
        sequence.Update(0.4f, 0.4f);
        sequence.Update(0.7f, 0.7f);

        Assert.True(sequence.IsComplete);
        Assert.Equal(100f, _b, 4);
    }

    [Fact]
    public void IntervalWaitsBetweenChildren()
    {
        var first = TweenA();
        var second = TweenB(to: 10f);
        var sequence = new Sequence().Append(first).AppendInterval(0.5f).Append(second);

        sequence.Update(1.1f, 1.1f);
        sequence.Update(0.3f, 0.3f);
        Assert.False(second.IsComplete);

        sequence.Update(0.3f, 0.3f);
        sequence.Update(1.1f, 1.1f);
        Assert.True(sequence.IsComplete);
        Assert.Equal(10f, _b, 4);
    }

    [Fact]
    public void CallbackFiresOnceAtItsPosition()
    {
        int calls = 0;
        var sequence = new Sequence()
            .Append(TweenA())
            .AppendCallback(() => calls++)
            .Append(TweenB(to: 10f));

        sequence.Update(1.1f, 1.1f);
        Assert.Equal(0, calls);

        sequence.Update(0.1f, 0.1f);
        Assert.Equal(1, calls);

        sequence.Update(2f, 2f);
        Assert.True(sequence.IsComplete);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void LoopsReplayChildren()
    {
        int completes = 0;
        var sequence = new Sequence()
            .Append(TweenA().OnComplete(() => completes++))
            .SetLoops(2);

        sequence.Update(1.1f, 1.1f);
        Assert.Equal(1, completes);

        sequence.Update(1.1f, 1.1f);
        Assert.Equal(2, completes);
        Assert.True(sequence.IsComplete);
    }

    [Fact]
    public void LoopRestartRestoresInitialFromValue()
    {
        var tween = TweenA();
        var sequence = new Sequence().Append(tween).SetLoops(2);

        sequence.Update(1.1f, 1.1f);
        Assert.Equal(10f, _a, 4);

        sequence.Update(0.5f, 0.5f);
        Assert.Equal(5f, _a, 4);

        sequence.Update(0.6f, 0.6f);
        Assert.True(sequence.IsComplete);
    }

    [Fact]
    public void YoyoReversesChildOrderOnSecondLoop()
    {
        var sequence = new Sequence().Append(TweenA()).Append(TweenB()).SetLoops(2, yoyo: true);

        sequence.Update(1.1f, 1.1f);
        sequence.Update(1.1f, 1.1f);

        // Yoyo plays children backward in reverse order: B then A.
        sequence.Update(0.5f, 0.5f);
        Assert.Equal(50f, _b, 4);
        Assert.Equal(10f, _a, 4);

        sequence.Update(0.6f, 0.6f);
        Assert.Equal(0f, _b, 4);

        sequence.Update(1.1f, 1.1f);
        Assert.Equal(0f, _a, 4);
        Assert.True(sequence.IsComplete);
    }

    [Fact]
    public void KillingSequenceKillsChildren()
    {
        var child = TweenA();
        var sequence = new Sequence().Append(child);
        sequence.Kill();

        Assert.True(sequence.IsKilled);
        Assert.True(child.IsKilled);
    }

    [Fact]
    public void RejectsInvalidAppends()
    {
        var sequence = new Sequence();
        var tween = TweenA();

        Assert.Throws<ArgumentNullException>(() => sequence.Append(null!));
        Assert.Throws<ArgumentException>(() => sequence.Append(sequence));
        Assert.Throws<ArgumentException>(() => sequence.Append(tween).Append(tween));

        tween.Kill();
        Assert.Throws<ArgumentException>(() => sequence.Append(tween));
    }

    [Fact]
    public void EmptySequenceCompletesImmediately()
    {
        var sequence = new Sequence();
        sequence.Update(0.1f, 0.1f);
        Assert.True(sequence.IsComplete);
    }

    [Fact]
    public void NormalizedTimeAggregatesChildren()
    {
        var sequence = new Sequence().Append(TweenA()).Append(TweenB());
        sequence.Update(0.5f, 0.5f);
        Assert.Equal(0.25f, sequence.NormalizedTime, 4);
    }
}