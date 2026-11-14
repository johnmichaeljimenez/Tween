using System;

namespace TweenLib
{
    public abstract class TweenBase
    {
        private Action? onStart;
        private Action? onUpdate;
        private Action? onComplete;
        private Action? onKill;
        private Func<bool>? link;

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

        /// <summary>
        /// Keeps this tween alive while <paramref name="isAlive"/> returns true.
        /// When it returns false (for example a Unity object became destroyed), the tween is killed and removed.
        /// Use a predicate so Unity's overloaded null check runs inside the caller's assembly:
        /// <c>SetLink(() =&gt; target != null)</c>.
        /// </summary>
        public TweenBase SetLink(Func<bool> isAlive)
        {
            link = isAlive ?? throw new ArgumentNullException(nameof(isAlive));
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
            link = null;
        }

        /// <summary>
        /// Shared gate for all <see cref="Update"/> implementations: stopped/paused tweens skip work;
        /// a failed link kills the tween.
        /// </summary>
        protected bool CanUpdate()
        {
            if (IsComplete || IsKilled || IsPaused)
                return false;

            if (link != null && !link())
            {
                Kill();
                return false;
            }

            return true;
        }

        protected void RaiseStart() => onStart?.Invoke();

        protected void RaiseUpdate() => onUpdate?.Invoke();

        protected void RaiseComplete() => onComplete?.Invoke();

        internal virtual void Restart() => IsComplete = false;
    }
}