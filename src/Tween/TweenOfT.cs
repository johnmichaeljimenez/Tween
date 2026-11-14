using System;

namespace TweenLib
{
    public sealed class Tween<T> : TweenBase where T : struct
    {
        private readonly Func<T> getter;
        private readonly Action<T> setter;
        private readonly Func<T, T, float, T> lerper;
        private readonly float duration;
        private float initialDelay;

        private float delayRemaining;
        private EasingFunction easing = Easing.Linear;
        private int steps;
        private T from;
        private T to;
        private bool hasCustomFrom;
        private bool hasStarted;
        private bool startedOnce;
        private bool reverseOnStart;
        private bool isReversed;
        private float elapsedTime;

        private T initialFrom;
        private T initialTo;
        private bool initialHasCustomFrom;
        private bool capturedInitials;

        public Tween(Func<T> getter, Action<T> setter, T to, float duration)
        {
            this.getter = getter ?? throw new ArgumentNullException(nameof(getter));
            this.setter = setter ?? throw new ArgumentNullException(nameof(setter));
            if (duration < 0f)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration cannot be negative.");

            lerper = Lerper.Get<T>();
            this.to = to;
            this.duration = duration;
        }

        public T CurrentValue { get; private set; }

        public float Duration => duration;

        public float Delay => initialDelay;

        public T FromValue => from;

        public T ToValue => to;

        public override float NormalizedTime => duration <= 0f
            ? 1f
            : Math.Clamp(elapsedTime / duration, 0f, 1f);

        public new Tween<T> SetId(string id)
        {
            base.SetId(id);
            return this;
        }

        public new Tween<T> SetLoops(int count, bool yoyo = false)
        {
            base.SetLoops(count, yoyo);
            return this;
        }

        public new Tween<T> OnStart(Action action)
        {
            base.OnStart(action);
            return this;
        }

        public new Tween<T> OnUpdate(Action action)
        {
            base.OnUpdate(action);
            return this;
        }

        public new Tween<T> OnComplete(Action action)
        {
            base.OnComplete(action);
            return this;
        }

        public new Tween<T> OnKill(Action action)
        {
            base.OnKill(action);
            return this;
        }

        public new Tween<T> SetLink(Func<bool> isAlive)
        {
            base.SetLink(isAlive);
            return this;
        }

        public Tween<T> SetFrom(T value)
        {
            hasCustomFrom = true;
            from = value;
            setter(from);
            return this;
        }

        public Tween<T> SetDelay(float delay)
        {
            if (delay < 0f)
                throw new ArgumentOutOfRangeException(nameof(delay), "Delay cannot be negative.");

            initialDelay = delay;
            delayRemaining = delay;
            return this;
        }

        public Tween<T> SetEasing(EasingFunction easingFunction)
        {
            easing = easingFunction ?? throw new ArgumentNullException(nameof(easingFunction));
            return this;
        }

        public Tween<T> SetSteps(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Steps cannot be negative.");

            steps = count;
            return this;
        }

        public override void Reverse()
        {
            if (!hasStarted)
            {
                reverseOnStart = !reverseOnStart;
                return;
            }

            IsComplete = false;
            isReversed = !isReversed;
            elapsedTime = duration <= 0f ? 0f : Math.Clamp(elapsedTime, 0f, duration);
        }

        internal override void Restart()
        {
            IsComplete = false;
            CurrentLoop = 0;
            elapsedTime = 0f;
            isReversed = false;
            reverseOnStart = false;
            delayRemaining = startedOnce ? 0f : initialDelay;
            hasStarted = false;

            if (!capturedInitials)
                return;

            // Keep the original endpoints so the next start does not re-capture
            // the current (end) value as the new from.
            from = initialFrom;
            to = initialTo;
            hasCustomFrom = true;
        }

        public override void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (!CanUpdate())
                return;

            float dt = UseUnscaledTime ? unscaledDeltaTime : deltaTime;

            if (delayRemaining > 0f)
            {
                delayRemaining -= dt;
                if (delayRemaining > 0f)
                    return;
                dt = -delayRemaining;
            }

            if (!hasStarted)
            {
                hasStarted = true;
                startedOnce = true;

                if (reverseOnStart)
                {
                    reverseOnStart = false;
                    var target = to;
                    to = getter();
                    from = target;
                    hasCustomFrom = true;
                }
                else if (!hasCustomFrom)
                {
                    from = getter();
                }

                if (!capturedInitials)
                {
                    capturedInitials = true;
                    initialFrom = from;
                    initialTo = to;
                    initialHasCustomFrom = hasCustomFrom;
                }

                CurrentValue = from;
                setter(from);
                RaiseStart();
                if (IsKilled)
                    return;
            }

            elapsedTime += dt * (isReversed ? -1f : 1f);

            float progress = duration <= 0f ? 1f : Math.Clamp(elapsedTime / duration, 0f, 1f);
            float sampled = steps > 0 ? (float)Math.Floor(progress * steps) / steps : progress;
            CurrentValue = Sample(sampled);
            setter(CurrentValue);
            RaiseUpdate();
            if (IsKilled)
                return;

            if (progress >= 1f || (isReversed && elapsedTime <= 0f))
            {
                elapsedTime = isReversed ? 0f : duration;
                CurrentValue = Sample(isReversed ? 0f : 1f);
                setter(CurrentValue);

                if (LoopCount < 0 || CurrentLoop < LoopCount - 1)
                {
                    CurrentLoop++;
                    IsComplete = false;
                    elapsedTime = 0f;
                    isReversed = false;
                    if (Yoyo)
                    {
                        var swap = from;
                        from = to;
                        to = swap;
                        hasCustomFrom = true;
                    }
                    RaiseStart();
                }
                else
                {
                    IsComplete = true;
                    RaiseComplete();
                }
            }
        }

        private T Sample(float normalizedTime) => lerper(from, to, easing(normalizedTime));
    }
}