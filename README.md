# Tween

A small .NET tweening library. Animate values over time with easing, loops, sequences, and a central manager.

**Target:** .NET 8

## Quick start

Reference the project, then drive everything from your game/app loop:

```csharp
// Once per frame
TweenManager.Update(deltaTime, unscaledDeltaTime);
```

```csharp
float x = 0f;

Tween.To(() => x, v => x = v, to: 100f, duration: 1f)
    .SetEasing(Easing.QuadOut)
    .OnComplete(() => Console.WriteLine("done"));
```

`Tween.To` registers the tween with `TweenManager` automatically. You only need to call `Update`.

## Tweens

```csharp
Tween.To(() => value, v => value = v, to: 10f, duration: 2f)
    .SetFrom(0f)                 // start value (optional; defaults to current)
    .SetDelay(0.5f)              // wait before starting
    .SetEasing(Easing.CubicOut)  // curve
    .SetSteps(8)                 // snap progress into N steps
    .SetLoops(3, yoyo: true)     // play 3 times; yoyo reverses each loop
    .SetId("player.fade")        // look up / kill by id later
    .OnStart(() => { })
    .OnUpdate(() => { })
    .OnComplete(() => { })
    .OnKill(() => { });
```

Useful controls on any tween:

| Method / property | What it does |
|---|---|
| `Pause()` / `Resume()` | Freeze or continue |
| `Reverse()` | Play backward from the current point |
| `Kill()` | Stop and remove from the manager |
| `UseUnscaledTime` | Use the unscaled delta instead of scaled |
| `IsComplete` / `IsPaused` / `IsKilled` | State flags |
| `NormalizedTime` | Progress from 0 to 1 |

Loop count notes:

- `1` play once (default)
- `N` play N times
- `-1` loop forever
- `0` not allowed

## Sequences

Run tweens one after another. Gaps and one-shot callbacks are supported.

```csharp
Tween.Sequence()
    .Append(Tween.To(() => x, v => x = v, 100f, 1f))
    .AppendInterval(0.25f)
    .AppendCallback(() => PlaySound())
    .Append(Tween.To(() => y, v => y = v, 50f, 0.5f))
    .SetLoops(2, yoyo: true)
    .OnComplete(() => Console.WriteLine("sequence done"));
```

`Append` takes ownership of the child tween (it is detached from the manager and driven by the sequence).

## TweenManager

Keeps the active list and advances it each frame.

```csharp
TweenManager.Update(deltaTime, unscaledDeltaTime);

TweenManager.Find("player.fade");
TweenManager.Kill("player.fade");
TweenManager.KillByPrefix("ui.");
TweenManager.KillAll();

TweenManager.PauseAll();
TweenManager.ResumeAll();

TweenManager.Remove(tween);   // detach without killing
TweenManager.ActiveCount;     // how many are running
```

If two tweens share the same id, the older one is killed when the newer one is added.

## Easing

Built-in curves (In / Out / InOut where it makes sense):

- Linear
- Quad, Cubic, Quart, Quint
- Sine, Expo, Circ
- Back, Elastic, Bounce

Plus two helpers:

```csharp
Easing.Parabola(height: 1.5f);
Easing.Shake(frequency: 20f, amplitude: 1f);
```

Pass any `EasingFunction` (`float -> float`) to `SetEasing`.

## Custom types

Numeric primitives are registered out of the box (`float`, `double`, `int`, and friends). For your own structs, register a lerper once:

```csharp
Lerper.Register<Vector2>((from, to, t) =>
    new Vector2(
        from.X + (to.X - from.X) * t,
        from.Y + (to.Y - from.Y) * t));

Tween.To(() => pos, v => pos = v, target, 1f);
```

If a type has no lerper, `Tween.To` / `new Tween<T>(...)` throws and tells you to register one.

## Manual tweens

You can build a tween without auto-registering it:

```csharp
var tween = new Tween<float>(() => value, v => value = v, 10f, 1f);
TweenManager.Add(tween);
// or drive it yourself:
tween.Update(deltaTime, unscaledDeltaTime);
```

Same idea for `new Sequence()`.

## Build & test

```bash
dotnet build src/Tween/Tween.csproj
dotnet test tests/Tween.Tests/Tween.Tests.csproj
```

## Layout

```
src/Tween/          library
tests/Tween.Tests/  xUnit tests
```
