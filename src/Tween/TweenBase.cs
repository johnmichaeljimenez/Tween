using System;

namespace TweenLib
{
    public abstract class TweenBase
    {
        private Action? onStart;
        private Action? onUpdate;
        private Action? onComplete;
        private Action? onKill;

        public string? Id { get; private set; }
        public bool IsComplete { get; protected set; }
        public bool IsKilled { get; private set; }
        public bool IsPaused { get; private set; }
        public bool UseUnscaledTime { get; set; }

        protected int LoopCount { get; private set; } = 1;
        protected int CurrentLoop { get; set; }
        protected bool Yoyo { get; private set; }

        public int Loops => LoopCount;

        public abstract float NormalizedTime { get; }

        public TweenBase SetId(string id)
        {
            var previousId = Id;
            Id = id;
            TweenManager.OnIdChanged(this, previousId);
            return this;
        }

        public TweenBase SetLoops(int count, bool yoyo = false)
        {
            if (count == 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Loop count cannot be zero. Use -1 for infinite loops.");

            LoopCount = count;
            Yoyo = yoyo;
            return this;
        }

        public TweenBase OnStart(Action action)
        {
            onStart = action ?? throw new ArgumentNullException(nameof(action));
            return this;
        }

        public TweenBase OnUpdate(Action action)
        {
            onUpdate = action ?? throw new ArgumentNullException(nameof(action));
            return this;
        }

        public TweenBase OnComplete(Action action)
        {
            onComplete = action ?? throw new ArgumentNullException(nameof(action));
            return this;
        }

        public TweenBase OnKill(Action action)
        {
            onKill = action ?? throw new ArgumentNullException(nameof(action));
            return this;
        }

        public virtual void Pause() => IsPaused = true;

        public virtual void Resume() => IsPaused = false;

        public abstract void Reverse();

        public abstract void Update(float deltaTime, float unscaledDeltaTime);

        public virtual void Kill()
        {
            if (IsKilled)
                return;

            IsKilled = true;
            TweenManager.Detach(this);
            onKill?.Invoke();
            onStart = null;
            onUpdate = null;
            onComplete = null;
            onKill = null;
        }

        protected void RaiseStart() => onStart?.Invoke();

        protected void RaiseUpdate() => onUpdate?.Invoke();

        protected void RaiseComplete() => onComplete?.Invoke();

        internal virtual void Restart() => IsComplete = false;
    }
}