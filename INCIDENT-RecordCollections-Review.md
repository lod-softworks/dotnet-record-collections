## Incident: RecordCollections security/correctness review findings

### Summary
An internal review of the `Lod.RecordCollections` library and its IL post-processing tool identified **two high-risk issues** that can lead to (1) **silent data-structure corruption** due to an `Equals`/`GetHashCode` contract violation, and (2) **potential code execution during build tooling** due to unsafe executable discovery/launch behavior in the IL assembler.

### Impact
- **Correctness / reliability**: Violating the `Equals`/`GetHashCode` contract can break `Dictionary`/`HashSet` behavior in subtle ways (failed lookups, duplicates, “missing” keys).
- **Security / supply-chain**: Executing the first `ilasm.exe`/`ildasm.exe` found under the working directory can allow **running attacker-controlled binaries** if the working directory is untrusted.

### Affected components
- `src/Lod.RecordCollections/Collections/RecordCollectionComparer.cs`
- `src/Lod.RecordCollections.IlAssembler/Program.cs`
- `src/Lod.RecordCollections/Collections/RecordCollectionCloner.cs`
- `src/Lod.RecordCollections/Collections.Generic/RecordDictionary.cs` (perf/allocations)
- `src/Lod.RecordCollections/Collections/RecordCollectionComparer.DefaultStrategy.cs` (enumerator disposal)

---

## Findings (ranked)

### 1) Critical: `Equals`/`GetHashCode` contract violation for base/derived record collections
**Concern**: Critical (stop-ship)

**What**:
- `RecordCollectionComparer.GetHashCode(IReadOnlyRecordCollection)` seeds the hash with `collection.GetType().GetHashCode()`.
- `RecordCollectionComparer.Equals(IReadOnlyRecordCollection? x, IReadOnlyRecordCollection? y)` allows equality when the runtime types are assignable (base/derived), not strictly equal.

This creates a case where `x.Equals(y)` can be `true` while `x.GetHashCode() != y.GetHashCode()`, violating the .NET contract.

**Why it matters**:
This can cause silent corruption / incorrect behavior in hash-based collections.

**Recommended remediation**:
Choose one:
- **Safest/minimal**: Require strict runtime type equality (`x.GetType() == y.GetType()`).
- Alternative: Use a consistent “equality contract” type for hashing *and* equality (but the current `EqualityContract` is protected on the record types and not used by the comparer).

**Docs**:
- https://learn.microsoft.com/dotnet/api/system.object.gethashcode
- https://learn.microsoft.com/dotnet/api/system.object.equals
- https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record

---

### 2) High: IL assembler executes first `ilasm.exe`/`ildasm.exe` found under current directory
**Concern**: High (security)

**What**:
`Lod.RecordCollections.IlAssembler` searches for `ilasm.exe` and `ildasm.exe` by recursively enumerating files from `Directory.GetCurrentDirectory()` and runs the first match.

**Why it matters**:
If the working directory is a repo checkout / CI workspace / otherwise writable by an attacker (or a malicious PR), a fake `ildasm.exe`/`ilasm.exe` could be planted and executed, leading to code execution in the build/release environment.

**Recommended remediation**:
- Resolve tool paths from a **trusted location** (e.g., the NuGet package install directory for `runtime.win-x64.*` packages).
- Avoid “search all subdirectories”.
- Use `ProcessStartInfo` with explicit settings (no shell execute; explicit arguments).

**Docs**:
- https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo

---

### 3) High: `RecordCollectionCloner` swallows all exceptions during cloning
**Concern**: High (reliability / diagnosability)

**What**:
`catch { }` in `RecordCollectionCloner.TryCloneElement(Type, object)` suppresses failures and silently returns the original element.

**Why it matters**:
Masks real errors (reflection restrictions, trimming/AOT issues, invocation exceptions) and makes debugging extremely difficult.

**Recommended remediation**:
- Catch specific exceptions and either rethrow or surface a diagnostic hook (e.g., event/callback) so callers can detect cloning failures.
- Consider documenting trimming/AOT limitations explicitly.

**Docs**:
- https://learn.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions
- https://learn.microsoft.com/dotnet/core/deploying/trimming/prepare-libraries-for-trimming

---

### 4) Medium: avoidable allocations in `RecordDictionary(IEnumerable<KeyValuePair<...>>)`
**Concern**: Medium (performance)

**What**:
`RecordDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, ...)` uses `collection?.ToDictionary(...)` to create an intermediate dictionary and then passes it to the base constructor.

**Why it matters**:
Extra enumeration + allocation; amplified by `RecordEnumerable.ToRecordDictionary(...)`.

**Recommended remediation**:
Use the base `Dictionary` constructor that accepts `IEnumerable<KeyValuePair<...>>` directly, or populate without creating an intermediate dictionary.

**Docs**:
- https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2.-ctor

---

### 5) Medium: enumerator disposal in default strategy
**Concern**: Medium (resource safety)

**What**:
`DefaultStrategy.Equals` uses non-generic enumerators without disposing them.

**Why it matters**:
Some enumerators hold resources; not disposing is a correctness/perf smell.

**Recommended remediation**:
Dispose enumerators via `using`/`try/finally`, or prefer `foreach`.

**Docs**:
- https://learn.microsoft.com/dotnet/api/system.collections.ienumerator

---

### 6) Low→Medium (strategic): types in `System.*` namespaces
**Concern**: Low→Medium (compatibility risk)

**What**:
The package defines public types in `System.Collections.Generic` and `System.Linq`.

**Why it matters**:
.NET guidance discourages this; future BCL additions could collide and break consumers.

**Recommended remediation**:
If this is required for “drop-in replacement” goals, document the collision risk prominently.

**Docs**:
- https://learn.microsoft.com/dotnet/standard/design-guidelines/names-of-namespaces

---

## Man-down (stop-ship) check
- **Man-down = YES** if:
  - Derived record-collection types can appear, and these types are used as keys in `Dictionary`/`HashSet` or otherwise rely on hash-based equality.
  - The IL assembler runs in CI/release contexts where the working directory could contain untrusted files.
- Otherwise **Man-down = NO**, but findings (1) and (2) should still be treated as top-priority remediation items.

