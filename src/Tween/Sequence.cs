using System;
using System.Collections.Generic;

namespace TweenLib
{
    public sealed class Sequence : TweenBase
    {
        private readonly List<TweenBase> children = new List<TweenBase>();
        private int currentIndex;

        public int Count => children.Count;

        public override float NormalizedTime
        {
            get
            {
                if (children.Count == 0)
                    return IsComplete ? 1f : 0f;
                if (currentIndex >= children.Count)
                    return 1f;
                return (currentIndex + children[currentIndex].NormalizedTime) / children.Count;
            }
        }

        public new Sequence SetId(string id)
        {
            base.SetId(id);
            return this;
        }

        public new Sequence SetLoops(int count, bool yoyo = false)
        {
            base.SetLoops(count, yoyo);
            return this;
        }

        public new Sequence OnStart(Action action)
        {
            base.OnStart(action);
            return this;
        }

        public new Sequence OnUpdate(Action action)
        {
            base.OnUpdate(action);
            return this;
        }

        public new Sequence OnComplete(Action action)
        {
            base.OnComplete(action);
            return this;
        }

        public new Sequence OnKill(Action action)
        {
            base.OnKill(action);
            return this;
        }

        public Sequence Append(TweenBase tween)
        {
            if (tween == null)
                throw new ArgumentNullException(nameof(tween));
            if (ReferenceEquals(tween, this))
                throw new ArgumentException("A sequence cannot contain itself.");
            if (tween.IsKilled)
                throw new ArgumentException("A killed tween cannot be added to a sequence.");
            if (children.Contains(tween))
                throw new ArgumentException("This tween is already in the sequence.");

            TweenManager.Detach(tween);
            children.Add(tween);
            return this;
        }

        public Sequence AppendCallback(Action action) => Append(new CallbackAction(action));

        public Sequence AppendInterval(float duration) => Append(new Interval(duration));

        public override void Pause()
        {
            base.Pause();
            foreach (var child in children)
                child.Pause();
        }

        public override void Resume()
        {
            base.Resume();
            foreach (var child in children)
                child.Resume();
        }

        public override void Reverse()
        {
            IsComplete = false;
            foreach (var child in children)
                child.Reverse();
            children.Reverse();
            currentIndex = 0;
        }

        internal override void Restart()
        {
            IsComplete = false;
            currentIndex = 0;
            foreach (var child in children)
                child.Restart();
        }

        public override void Kill()
        {
            if (IsKilled)
                return;

            foreach (var child in children)
                child.Kill();
            base.Kill();
        }

        public override void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (IsComplete || IsKilled || IsPaused)
                return;

            if (children.Count == 0)
            {
                IsComplete = true;
                RaiseComplete();
                return;
            }

            var child = children[currentIndex];
            child.Update(deltaTime, unscaledDeltaTime);
            if (!child.IsComplete && !child.IsKilled)
                return;

            currentIndex++;
            if (currentIndex < children.Count)
                return;

            if (LoopCount < 0 || CurrentLoop < LoopCount - 1)
            {
                CurrentLoop++;
                if (Yoyo)
                {
                    // Play children backward in reverse order, continuing from
                    // their end values instead of restarting from the original from.
                    Reverse();
                }
                else
                {
                    Restart();
                }
            }
            else
            {
                IsComplete = true;
                RaiseComplete();
            }
        }
    }
}