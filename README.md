# Spoke2OneJS

Roslyn source generator that glues [Spoke](https://github.com/slimshader/Spoke) reactive signals to OneJS `useEventSync()`.

Mark a field in a `partial` class with `[OneJsBind]` (namespace `Spoke2OneJS`):

| Field type | Generated |
|---|---|
| `State<T>` (anything with a `Now` property) | `public T Name => _field.Now;` and `event Action OnNameChanged` forwarding to `Subscribe`/`Unsubscribe` |
| `Trigger` | `public void Name() => _field.Invoke();` |
| `Trigger<T>` | `public void Name(T param) => _field.Invoke(param);` |

The name defaults to the field name without `_` / `m_` and capitalized (`_startButtonClicked` → `StartButtonClicked`), or pass it explicitly: `[OneJsBind("History")]`.

## Unity setup

1. `dotnet build Spoke2OneJS.Generators -c Release`
2. Copy `Spoke2OneJS.Generators/bin/Release/netstandard2.0/Spoke2OneJS.Generators.dll` into `Assets/`, together with `Spoke2OneJS.Generators/Unity/Spoke2OneJS.Generators.dll.meta` (no platforms enabled, `RoslynAnalyzer` label).

## Sample

`Spoke2OneJS.Sample` compiles the Spoke sources from a sibling checkout at `../Spoke`.
