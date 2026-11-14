# Tween

A small tweening library for **.NET 8** and **Unity 2022.3+**. Animate values over time with easing, loops, sequences, and a central manager.

**Targets:** `net8.0` + `netstandard2.1` (Unity 2021.2+ / 2022 LTS)

## Install

### .NET (apps, libraries)

Project reference:

```xml
<ProjectReference Include="..\Tween\src\Tween\Tween.csproj" />
```

Or build/pack the project and reference the `TweenLib` package / `TweenLib.dll`.

```bash
dotnet build src/Tween/Tween.csproj
dotnet pack src/Tween/Tween.csproj -c Release
```

### Unity 2022.3+

This repo is a dual package. Unity consumes **`src/Tween`** as a UPM package (source + `package.json` + `TweenLib.asmdef`).

**Git URL** (path to the package folder):

```
https://github.com/johnmichaeljimenez/Tween.git?path=/src/Tween
```

**Local** — Package Manager → Add package from disk → select `src/Tween/package.json`.

The assembly is pure C# (`noEngineReferences`), so it does not depend on `UnityEngine`. Drive it from a `MonoBehaviour`/`PlayerLoop` update:

```csharp
void Update()
{
    TweenLib.TweenManager.Update(Time.deltaTime, Time.unscaledDeltaTime);
}
```

Register Unity types once at startup if you tween them:

```csharp
TweenLib.Lerper.Register<Vector3>((from, to, t) => Vector3.LerpUnclamped(from, to, t));
TweenLib.Lerper.Register<Quaternion>((from, to, t) => Quaternion.SlerpUnclamped(from, to, t));
TweenLib.Lerper.Register<Color>((from, to, t) => Color.LerpUnclamped(from, to, t));
```

## Quick start

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
| `SetLink(() => target != null)` | Auto-kill when the predicate becomes false (Unity-safe target guard) |
| `UseUnscaledTime` | Use the unscaled delta instead of scaled |
| `IsComplete` / `IsPaused` / `IsKilled` | State flags |
| `NormalizedTime` | Progress from 0 to 1 |

### Unity destroyed objects

Ongoing tweens keep calling your getter/setter each frame. If the `MonoBehaviour` / `Transform` / `GameObject` they close over is destroyed, those callbacks can throw (`MissingReferenceException`).

Because this package is pure C# (`noEngineReferences`), it cannot special-case `UnityEngine.Object`. Use **`SetLink`** with a predicate that runs Unity's overloaded null check in *your* code:

```csharp
Tween.To(() => transform.localScale, v => transform.localScale = v, Vector3.one, 0.4f)
    .SetLink(() => this != null)   // or () => gameObject != null / () => target != null
    .SetEasing(Easing.BackOut);
```

When the predicate returns `false`, the tween is **killed**, removed from `TweenManager`, and `OnKill` runs — before the next getter/setter call.

`SetLink` also works on sequences. Child tweens can have their own links independently.

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

Both `netstandard2.1` and `net8.0` are built from the same sources (C# 9 so Unity 2022 can compile the UPM package).

## Layout

```
src/Tween/          library (.NET multi-target + Unity UPM package)
  Tween.csproj      netstandard2.1;net8.0
  package.json      Unity package manifest
  TweenLib.asmdef   Unity assembly definition
tests/Tween.Tests/  xUnit tests
```

## Compatibility notes

| Surface | Support |
|---|---|
| `net8.0` | ProjectReference or NuGet |
| Unity 2022.3 LTS | UPM source package under `src/Tween` |
| API profile | `netstandard2.1` (no .NET 6+ only APIs in the shared path) |
| Language | C# 9 (Unity 2022 default) |
