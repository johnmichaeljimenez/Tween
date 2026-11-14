using System;

namespace TweenLib
{
    internal sealed class CallbackAction : TweenBase
    {
        private readonly Action action;

        public CallbackAction(Action action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public override float NormalizedTime => 1f;

        public override void Reverse() => IsComplete = false;

        internal override void Restart() => IsComplete = false;

        public override void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (IsComplete || IsKilled || IsPaused)
                return;

            IsComplete = true;
            RaiseStart();
            action();
            RaiseComplete();
        }
    }
}