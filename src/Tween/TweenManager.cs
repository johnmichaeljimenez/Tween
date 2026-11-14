using System;
using System.Collections.Generic;

namespace TweenLib
{
    public static class TweenManager
    {
        private static readonly List<TweenBase> activeTweens = new List<TweenBase>();
        private static readonly Dictionary<string, TweenBase> tweensById = new Dictionary<string, TweenBase>(StringComparer.Ordinal);

        public static int ActiveCount => activeTweens.Count;

        public static TweenBase? Find(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            return tweensById.TryGetValue(id, out var tween) ? tween : null;
        }

        public static void Add(TweenBase tween)
        {
            if (tween == null)
                throw new ArgumentNullException(nameof(tween));
            if (tween.IsKilled)
                throw new ArgumentException("A killed tween cannot be added.", nameof(tween));

            if (!string.IsNullOrEmpty(tween.Id) && tweensById.TryGetValue(tween.Id, out var existing) && !ReferenceEquals(existing, tween))
                existing.Kill();

            if (!activeTweens.Contains(tween))
                activeTweens.Add(tween);

            if (!string.IsNullOrEmpty(tween.Id))
                tweensById[tween.Id] = tween;
        }

        public static bool Remove(TweenBase tween)
        {
            if (tween == null)
                return false;
            return Detach(tween);
        }

        public static bool Kill(TweenBase tween)
        {
            if (tween == null || tween.IsKilled)
                return false;
            tween.Kill();
            return true;
        }

        public static bool Kill(string id)
        {
            var tween = Find(id);
            return tween != null && Kill(tween);
        }

        public static int KillByPrefix(string prefix)
        {
            if (prefix == null)
                throw new ArgumentNullException(nameof(prefix));

            int killed = 0;
            for (int i = activeTweens.Count - 1; i >= 0; i--)
            {
                var id = activeTweens[i].Id;
                if (!string.IsNullOrEmpty(id) && id.StartsWith(prefix, StringComparison.Ordinal))
                {
                    activeTweens[i].Kill();
                    killed++;
                }
            }
            return killed;
        }

        public static void KillAll()
        {
            for (int i = activeTweens.Count - 1; i >= 0; i--)
                activeTweens[i].Kill();
        }

        public static void PauseAll()
        {
            foreach (var tween in activeTweens)
                tween.Pause();
        }

        public static void ResumeAll()
        {
            foreach (var tween in activeTweens)
                tween.Resume();
        }

        public static void Update(float deltaTime, float unscaledDeltaTime)
        {
            for (int i = activeTweens.Count - 1; i >= 0; i--)
            {
                var tween = activeTweens[i];
                tween.Update(deltaTime, unscaledDeltaTime);
                if (tween.IsComplete && !tween.IsKilled)
                    tween.Kill();
            }
        }

        internal static bool Detach(TweenBase tween)
        {
            bool removed = activeTweens.Remove(tween);

            var id = tween.Id;
            if (!string.IsNullOrEmpty(id) && tweensById.TryGetValue(id, out var tracked) && ReferenceEquals(tracked, tween))
                tweensById.Remove(id);

            return removed;
        }

        internal static void OnIdChanged(TweenBase tween, string? previousId)
        {
            if (!activeTweens.Contains(tween))
                return;

            if (!string.IsNullOrEmpty(previousId) && tweensById.TryGetValue(previousId, out var tracked) && ReferenceEquals(tracked, tween))
                tweensById.Remove(previousId);

            if (!string.IsNullOrEmpty(tween.Id))
            {
                if (tweensById.TryGetValue(tween.Id, out var existing) && !ReferenceEquals(existing, tween))
                    existing.Kill();

                tweensById[tween.Id] = tween;
            }
        }
    }
}