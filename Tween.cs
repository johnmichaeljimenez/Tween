using System;
using System.Collections.Generic;

namespace Tween
{
	public delegate float EasingFunction(float t);

	public class Tween<T> : TweenBase where T : struct
	{
		private static readonly Dictionary<Type, Func<object, object, float, object>> Lerpers = new Dictionary<Type, Func<object, object, float, object>>
		{
			{ typeof(float), (from, to, amt) => (float)from + ((float)to - (float)from) * amt }
		};

		public static void RegisterLerper(Func<T, T, float, T> lerper)
		{
			Lerpers[typeof(T)] = (from, to, amt) => lerper((T)from, (T)to, amt);
		}

		private Func<T> Getter;
		private Action<T> Setter;
		private T From;
		private T To;
		private float Duration;
		private float Delay;
		private EasingFunction EasingFunc = Easing.Linear;
		private Action OnStartAction;
		private Action OnUpdateAction;
		private Action OnCompleteAction;
		private Action OnKillAction;
		private bool useCustomFrom;
		private float elapsedTime;
		private bool hasStarted;
		private int loopCount;
		private int currentLoop;
		private bool yoyo;
		private bool isReversed;
		private int steps = 0;

		public Tween(Func<T> getter, Action<T> setter, T to, float duration, T? from = null, string id = null)
		{
			if (!Lerpers.ContainsKey(typeof(T)))
			{
				throw new ArgumentException($"No lerper defined for type {typeof(T).Name}. Register a lerper using RegisterLerper.");
			}

			Getter = getter;
			Setter = setter;
			To = to;
			Duration = duration;
			Id = id;
			if (from.HasValue)
			{
				From = from.Value;
				useCustomFrom = true;
				Setter(From);
			}
			else
			{
				useCustomFrom = false;
			}
		}

		public Tween<T> SetDelay(float delay)
		{
			Delay = delay;
			return this;
		}

		public Tween<T> SetEasing(EasingFunction easing)
		{
			EasingFunc = easing;
			return this;
		}

		public Tween<T> SetLoop(int count, bool yoyo = false)
		{
			loopCount = count;
			this.yoyo = yoyo;
			return this;
		}

		public Tween<T> SetSteps(int numSteps)
		{
			if (numSteps < 0)
			{
				throw new ArgumentException("Number of steps must be non-negative.");
			}
			steps = numSteps;
			return this;
		}

		public Tween<T> OnStart(Action action)
		{
			OnStartAction = action;
			return this;
		}

		public Tween<T> OnUpdate(Action action)
		{
			OnUpdateAction = action;
			return this;
		}

		public Tween<T> OnComplete(Action action)
		{
			OnCompleteAction = action;
			return this;
		}

		public Tween<T> OnKill(Action action)
		{
			OnKillAction = action;
			return this;
		}

		public override void Pause()
		{
			base.Pause();
		}

		public override void Resume()
		{
			base.Resume();
		}

		public override void Reverse()
		{
			isReversed = !isReversed;
			var temp = From;
			From = To;
			To = temp;
			elapsedTime = Duration - elapsedTime;
		}

		public override void Update(float dt)
		{
			if (IsCompleted || IsDisposed || IsPaused) return;

			if (Delay > 0)
			{
				Delay -= dt;
				if (Delay > 0) return;
			}

			if (!hasStarted)
			{
				hasStarted = true;
				if (!useCustomFrom)
				{
					From = Getter();
					Setter(From);
				}
				OnStartAction?.Invoke();
			}

			elapsedTime += dt * (isReversed ? -1 : 1);
			if (elapsedTime < 0) elapsedTime = 0;
			float progress = elapsedTime / Duration;
			if (progress > 1) progress = 1;

			if (steps > 0)
			{
				progress = (float)Math.Floor(progress * steps) / steps;
			}

			float easedProgress = EasingFunc(progress);
			T currentValue = (T)Lerpers[typeof(T)](From, To, easedProgress);
			Setter(currentValue);

			OnUpdateAction?.Invoke();

			if (progress >= 1 || (isReversed && progress <= 0))
			{
				if (loopCount == -1 || currentLoop < loopCount - 1)
				{
					currentLoop++;
					elapsedTime = 0;
					if (yoyo)
					{
						isReversed = !isReversed;
						var temp = From;
						From = To;
						To = temp;
					}
					OnStartAction?.Invoke();
				}
				else
				{
					IsCompleted = true;
					OnCompleteAction?.Invoke();
				}
			}
		}

		public override void Dispose()
		{
			if (!IsDisposed)
			{
				base.Dispose();
				OnKillAction?.Invoke();
				OnStartAction = null;
				OnUpdateAction = null;
				OnCompleteAction = null;
				OnKillAction = null;
			}
		}
	}
}