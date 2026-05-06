using System;

namespace Tween
{

    public abstract class TweenBase
    {
        public virtual float NormalizedTime => throw new NotImplementedException();
        public bool IsCompleted { get; set; }
        public bool IsDisposed { get; private set; }
        public bool IsPaused { get; private set; }
        public string Id { get; protected set; }
        public bool UseUnscaledTime { get; set; } = false;
        public float Parameter1 { get; set; }
        public float Parameter2 { get; set; }

        public abstract void Update(float scaledDt, float unscaledDt);

        public virtual void Pause()
        {
            IsPaused = true;
        }

        public virtual void Resume()
        {
            IsPaused = false;
        }

        public virtual void Reverse()
        {

        }

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                IsCompleted = true;
            }
        }
    }
}