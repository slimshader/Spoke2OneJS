# Spoke2OneJS

A Roslyn source generator that connects [Spoke](https://github.com/Adam4lexander/Spoke) reactive fields to [OneJS](https://onejs.com) React UI.

You mark a private Spoke field with `[OneJsBind]`, and the generator writes the members that OneJS's `useEventSync()` hook expects. You don't write them by hand:

- **`State<T>`** becomes a read-only property and an `On<Name>Changed` event.
- **`Trigger` and `Trigger<T>`** become a method that JS can call to fire the trigger.

## Example

```csharp
using System.Collections.Immutable;
using Spoke;
using Spoke2OneJS;

public partial class TicTacToe
{
    [OneJsBind("History")]
    private State<ImmutableArray<string[]>> _history =
        State.Create(ImmutableArray.Create<string[]>().Add(new string[9]));

    [OneJsBind]
    private Trigger _startButtonClicked = Trigger.Create();

    [OneJsBind("CellClicked")]
    private Trigger<int> _cellClicked = Trigger.Create<int>();
}
```

The generator adds these members to `TicTacToe`:

```csharp
public ImmutableArray<string[]> History => _history.Now;

public event Action OnHistoryChanged
{
    add    { _history.Subscribe(value); }
    remove { _history.Unsubscribe(value); }
}

public void StartButtonClicked()
{
    _startButtonClicked.Invoke();
}

public void CellClicked(int param)
{
    _cellClicked.Invoke(param);
}
```

On the JS side, `useEventSync(source, "History")` reads `source.History` and re-renders when `OnHistoryChanged` fires:

```tsx
import { Button, useEventSync } from "onejs-react"

function Board({ game }: { game: CS.TicTacToe }) {
    const history = useEventSync(game, "History")
    return (
        <>
            <Button text="Start" onClick={() => game.StartButtonClicked()} />
            {/* ...render history, call game.CellClicked(i) on click... */}
        </>
    )
}
```

## What gets generated

| Field type | Generated members |
|---|---|
| `State<T>`, or any type with a readable `Now` property | `public T Name => _field.Now;` and `public event Action OnNameChanged`, which forwards to `Subscribe` and `Unsubscribe` |
| `Trigger` | `public void Name()`, which calls `_field.Invoke()` |
| `Trigger<T>` | `public void Name(T param)`, which calls `_field.Invoke(param)` |

If the field's type has a `Now` property, it is bound as a state. Otherwise, if it has a public `Invoke` method, it is bound as a trigger. When there are several `Invoke` overloads, the generator uses the one with the most parameters, so `Trigger<T>` gets the `Invoke(T)` overload.

### Rules

- **Naming:** `[OneJsBind("Name")]` sets the member name. Without an argument, the name comes from the field: the `_` or `m_` prefix is removed and the first letter is capitalized, so `_startButtonClicked` becomes `StartButtonClicked`.
- **Partial types:** the containing type, and every type it is nested in, must be `partial`.
- **Static fields:** a static field produces static members.
- **Supported containing types:** classes, structs and records, including generic and nested ones.
- **The attribute:** the generator adds `[OneJsBind]` to each assembly itself, as an `internal` attribute in the `Spoke2OneJS` namespace, so no separate runtime DLL is needed. Attribute usages are removed from the compiled assembly. To keep them, define `SPOKE2ONEJS_KEEP_ATTRIBUTES`.

### Diagnostics

| Id | Meaning |
|---|---|
| `OJS001` | The containing type is not `partial` |
| `OJS002` | The field's type has neither a readable `Now` property nor an `Invoke` method |
| `OJS003` | The bind name is not a valid C# identifier |

## Unity setup

Requires Unity 2022.3 or later (Roslyn 4.3 source generators).

1. Build the generator:
   ```
   dotnet build Spoke2OneJS.Generators -c Release
   ```
2. Copy `Spoke2OneJS.Generators/bin/Release/netstandard2.0/Spoke2OneJS.Generators.dll` into your project's `Assets/` folder. If you put it inside an asmdef folder, it only applies to that assembly.
3. Copy `Spoke2OneJS.Generators/Unity/Spoke2OneJS.Generators.dll.meta` next to it. This file already has the correct import settings: no platforms enabled, *Validate References* turned off, and the `RoslynAnalyzer` asset label. Alternatively, set these by hand in the Plugin Inspector.

If Unity reports *"Unable to resolve reference 'Microsoft.CodeAnalysis'"*, the DLL is being imported as a regular plugin. Fix this with step 3.

When you update the generator, replace only the `.dll` and keep the existing `.meta` file.

## Repository layout

| Path | Contents |
|---|---|
| `Spoke2OneJS.Generators/` | The incremental source generator (`netstandard2.0`, Microsoft.CodeAnalysis.CSharp 4.3.1) |
| `Spoke2OneJS.Sample/` | A console app that tests the generator against the real Spoke sources |

The sample compiles Spoke's engine-independent sources (`Spoke.Runtime` and `Spoke.Reactive`) from a sibling checkout, so clone Spoke next to this repo first:

```
git clone https://github.com/Adam4lexander/Spoke ../Spoke
dotnet run --project Spoke2OneJS.Sample
```
