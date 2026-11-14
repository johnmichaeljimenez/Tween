using System;

namespace TweenLib
{
    internal sealed class Interval : TweenBase
    {
        private readonly float duration;
        private float elapsedTime;
        private bool isReversed;
        private bool hasStarted;

        public Interval(float duration)
        {
            if (duration < 0f)
                throw new ArgumentOutOfRangeException(nameof(duration), "Interval cannot be negative.");
            this.duration = duration;
        }

        public override float NormalizedTime => duration <= 0f
            ? 1f
            : Math.Clamp(elapsedTime / duration, 0f, 1f);

        public override void Reverse()
        {
            IsComplete = false;
            isReversed = !isReversed;
            elapsedTime = duration <= 0f ? 0f : Math.Clamp(elapsedTime, 0f, duration);
        }

        internal override void Restart()
        {
            IsComplete = false;
            isReversed = false;
            elapsedTime = 0f;
            hasStarted = false;
        }

        public override void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (IsComplete || IsKilled || IsPaused)
                return;

            if (!hasStarted)
            {
                hasStarted = true;
                RaiseStart();
                if (IsKilled)
                    return;
            }

            float dt = UseUnscaledTime ? unscaledDeltaTime : deltaTime;
            elapsedTime += isReversed ? -dt : dt;

            if (isReversed)
            {
                if (elapsedTime < 0f)
                {
                    elapsedTime = 0f;
                    IsComplete = true;
                    RaiseComplete();
                }
                return;
            }

            if (elapsedTime >= duration)
            {
                IsComplete = true;
                RaiseComplete();
            }
        }
    }
}