using System.Collections.Generic;

namespace Tween
{
    public class Sequence : TweenBase
    {
        private List<TweenBase> tweens = new List<TweenBase>();
        private int currentIndex = 0;
        private int loopCount;
        private int currentLoop;
        private bool yoyo;

        public Sequence(string id = null, bool useUnscaledTime = false)
        {
            Id = id;
            UseUnscaledTime = useUnscaledTime;
        }

        public Sequence Append(TweenBase tween)
        {
            tweens.Add(tween);
            return this;
        }

        public Sequence SetLoop(int count, bool yoyo = false)
        {
            loopCount = count;
            this.yoyo = yoyo;
            return this;
        }

        public override void Pause()
        {
            base.Pause();
            if (currentIndex < tweens.Count)
            {
                tweens[currentIndex].Pause();
            }
        }

        public override void Resume()
        {
            base.Resume();
            if (currentIndex < tweens.Count)
            {
                tweens[currentIndex].Resume();
            }
        }

        public override void Reverse()
        {
            if (currentIndex < tweens.Count)
            {
                tweens[currentIndex].Reverse();
            }
        }

        public override void Update(float scaledDt, float unscaledDt)
        {
            if (IsCompleted || IsDisposed || IsPaused) return;

            if (currentIndex >= tweens.Count)
            {
                if (loopCount == -1 || currentLoop < loopCount - 1)
                {
                    currentLoop++;
                    currentIndex = 0;
                    if (yoyo)
                    {
                        tweens.Reverse();
                    }
                    foreach (var tween in tweens)
                    {
                        tween.IsCompleted = false;
                        tween.Resume();
                    }
                }
                else
                {
                    IsCompleted = true;
                }
                return;
            }

            tweens[currentIndex].Update(scaledDt, unscaledDt);
            if (tweens[currentIndex].IsCompleted)
            {
                currentIndex++;
            }
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                base.Dispose();
                foreach (var tween in tweens)
                {
                    tween.Dispose();
                }
                tweens.Clear();
            }
        }
    }
}