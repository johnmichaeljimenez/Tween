namespace Tween
{
    public static class TweenManager
    {
        private static List<TweenBase> activeTweens = new List<TweenBase>();
        private static Dictionary<string, TweenBase> tweensById = new Dictionary<string, TweenBase>();

        public static void Add(TweenBase tween)
        {
            if (!tween.IsDisposed)
            {
                if (!string.IsNullOrEmpty(tween.Id) && tweensById.ContainsKey(tween.Id))
                {
                    tweensById[tween.Id].Dispose();
                    tweensById.Remove(tween.Id);
                    activeTweens.Remove(tween);
                }

                activeTweens.Add(tween);
                if (!string.IsNullOrEmpty(tween.Id))
                {
                    tweensById[tween.Id] = tween;
                }
            }
        }

        public static void Update(float scaledDt, float unscaledDt)
        {
            for (int i = activeTweens.Count - 1; i >= 0; i--)
            {
                if (activeTweens[i].IsDisposed)
                {
                    if (!string.IsNullOrEmpty(activeTweens[i].Id))
                    {
                        tweensById.Remove(activeTweens[i].Id);
                    }
                    activeTweens.RemoveAt(i);
                    continue;
                }

                activeTweens[i].Update(scaledDt, unscaledDt);
                if (activeTweens[i].IsCompleted)
                {
                    if (!string.IsNullOrEmpty(activeTweens[i].Id))
                    {
                        tweensById.Remove(activeTweens[i].Id);
                    }
                    activeTweens[i].Dispose();
                    activeTweens.RemoveAt(i);
                }
            }
        }

        public static void Remove(TweenBase tween)
        {
            if (tween != null && !tween.IsDisposed)
            {
                if (!string.IsNullOrEmpty(tween.Id))
                {
                    tweensById.Remove(tween.Id);
                }
                tween.Dispose();
                activeTweens.Remove(tween);
            }
        }

        public static void Clear()
        {
            foreach (var tween in activeTweens)
            {
                if (!string.IsNullOrEmpty(tween.Id))
                {
                    tweensById.Remove(tween.Id);
                }
                tween.Dispose();
            }
            activeTweens.Clear();
            tweensById.Clear();
        }

        public static void ClearByPrefix(string prefix)
        {
            foreach (var tween in activeTweens)
            {
                if (!string.IsNullOrEmpty(tween.Id) && tween.Id.StartsWith(prefix))
                {
                    tweensById.Remove(tween.Id);
                }

                tween.Dispose();
            }
            activeTweens.Clear();
            tweensById.Clear();
        }

        public static void PauseAll()
        {
            foreach (var tween in activeTweens)
            {
                tween.Pause();
            }
        }

        public static void ResumeAll()
        {
            foreach (var tween in activeTweens)
            {
                tween.Resume();
            }
        }
    }
}