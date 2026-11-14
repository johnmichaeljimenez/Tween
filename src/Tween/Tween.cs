using System;

namespace TweenLib
{
    public static class Tween
    {
        public static Tween<T> To<T>(Func<T> getter, Action<T> setter, T to, float duration) where T : struct
        {
            var tween = new Tween<T>(getter, setter, to, duration);
            TweenManager.Add(tween);
            return tween;
        }

        public static Sequence Sequence()
        {
            var sequence = new Sequence();
            TweenManager.Add(sequence);
            return sequence;
        }
    }
}