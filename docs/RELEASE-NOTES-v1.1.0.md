# Cirreum.Messaging.Distributed 1.1.0 — Time-of-day batching + cross-assembly type resolution

## Why this release exists

Two reasons. First, the `IBatchingPolicy` documentation has always described three usage levels — the pass-through default, a framework-supplied time-of-day policy, and fully custom — but only levels 1 and 3 existed. This release ships level 2. Second, writing the package's first test suite surfaced two real type-resolution defects that would have silently degraded any app-defined message type; both are fixed before any real consumer ships against them.

## What's new

### `TimeOfDayBatchingPolicy`

Simple day-of-week / hour-window scaling of the channel's base batch-fill wait time, without writing a custom `IBatchingPolicy`:

```csharp
builder.AddMessaging(m => m.UseTimeOfDayBatching(schedule => {
	schedule.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
	schedule.Rules.Add(new() {
		Days = [DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday],
		StartHour = 16,
		EndHour = 23,
		ScalingFactor = 0.5, // high volume expected — halve the fill wait, send sooner
		Description = "Weekend evening spike"
	});
}));
```

Rules are first-match-wins, windows may wrap past midnight, the schedule validates at construction, and the matching rule's `Description` surfaces as the `BatchingDecision.Reason` in operator logs. Unlike the pre-reset time-profile machinery, the schedule declares an explicit `TimeZoneInfo` (defaulting to server-local) — so containers running UTC can follow a business time zone. Capacity passes through untouched; only the fill wait scales.

### `DistributedMessageEnvelope.ResolveMessageType()`

Resolves the envelope's captured CLR type, or returns `null` when the type isn't available in the process. Handles both the new assembly-hinted name format and bare full names from older producers. Receivers should prefer it over raw `Type.GetType(envelope.MessageType)`.

## Fixed: two cross-assembly type-resolution defects

- **Registry target routing.** `DistributedMessageRegistry` resolved captured type names with `Type.GetType(fullName)`, which only finds types in its own assembly or the core library. Result: `[DistributedMessageTarget(MessageTarget.Queue)]` on any app-defined message type was silently ignored and everything routed to the topic. The target map is now built from a direct assembly-scan pass.
- **Envelope wire type names.** Envelopes stamped a bare `Type.FullName`, which receivers cannot resolve for app-defined types. Envelopes now stamp `FullName, AssemblySimpleName` — deliberately without version/culture/token, so producer/consumer assembly-version drift doesn't break resolution — and deserialization goes through `ResolveMessageType()`, which also accepts the legacy bare-name format for envelopes already in flight.

## Compatibility

Fully additive API surface; no breaking changes. The envelope's `MessageType` wire value gains an assembly hint — consumers using `ResolveMessageType()` or `DeserializeMessage(...)` handle both formats transparently. First release with a test suite (40 tests).

## See also

- `docs/CHANGELOG.md` — condensed change list.
- `Cirreum.Runtime.Messaging` 2.0.0 — the delivery engine that consumes this release (`UseTimeOfDayBatching` lives on its `AddMessaging` composition callback).
