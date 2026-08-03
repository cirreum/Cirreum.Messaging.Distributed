# Changelog

All notable changes to **Cirreum.Messaging.Distributed** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Updated

- Updated NuGet packages.

## [2.0.0] - 2026-07-26

### Updated

- Re-pinned `Cirreum.Kernel` `1.3.0` → `2.0.0`, which carries the marker rename this release follows.

### Changed

- **Conductor's publish/subscribe markers are renamed** — `INotification` → `IDomainEvent`,
  `INotificationHandler<T>` → `IDomainEventHandler<T>` — following `Cirreum.Kernel` 2.0.0.
  Cirreum used "notification" for two unrelated concepts: in-application publish/subscribe, and
  the human-facing state family a client binds to in order to show a person something.
  `IDomainEvent` names the first for what it is; "notification" now refers only to the second.

  **`INotificationState` and `IScopedNotificationState` keep their names** — they are the
  human-facing concept, and preserving that separation is the point of the rename. A project-wide
  find/replace of "Notification" will destroy it.

## [1.2.2] - 2026-07-24

### Updated

- Updated NuGet packages.

## [1.2.1] - 2026-07-20

### Updated

- Updated NuGet packages.

## [1.2.0] - 2026-07-07

### Added

- `DistributedMessageEnvelope.DeserializeMessage(Type)` — deserialization against a **caller-resolved** concrete type (canonically the registry's `ResolveType(MessageIdentifier, MessageVersion)`). The envelope performs no type resolution of its own.

### Changed

- **The envelope no longer resolves CLR types, and the channel owns its serialization (ADR-0029).** `ResolveMessageType()` and the two self-resolving `DeserializeMessage` overloads (parameterless, and `Func<Type, string, object>`-only) are removed — replaced by `DeserializeMessage(Type)`. Inbound type resolution belongs to the registry's identity map, which only ever selects from the receiver's own vetted scan set; a wire-stamped CLR type name is no longer a resolution input anywhere in the message track. The `MessageType` stamp remains on the wire as diagnostic metadata (logging and dead-letter triage), documented as resolution-inert.
- **The channel owns its serialization — System.Text.Json with internally-decided options.** The pluggable seam is removed (`CreateWithSerializer`, the `Func`-taking `DeserializeMessage` overloads, and the never-shipped `IDistributedPayloadSerializer`), and so is `FromJson(string, JsonSerializerOptions)` — callers no longer supply serialization options at all. Both the envelope and its payload serialize through one internally-owned `JsonSerializerOptions` (currently the STJ defaults), so producer and consumer are always symmetric. A message family owns its wire format end to end (the auth-events channel serializes STJ inline the same way); a different format is a different channel, not a serializer swap or a per-call option. The envelope was already STJ (`FromJson`), so a "pluggable payload serializer" only ever wrapped the inner string in a hardcoded-STJ envelope — incoherent, and consumer-less.
- **`IDistributedTransportPublisher<TBase>` and `EmptyTransportPublisher<TBase>` are removed.** They were registered (the `Empty` no-op by `Cirreum.Services.Serverless`; the engine by `Cirreum.Runtime.Messaging`) but never resolved on any live path — the outbound Conductor handler injects the engine directly. Conductor is the channel's outbound extension seam: publish a `DistributedMessage` and Conductor fans out to handlers, including the engine's outbound handler when the delivery engine is installed; custom delivery is your own `INotificationHandler`.
- All member removals ship as a minor per ADR-0029's prerelease convention — this package is reachable only through the framework's own umbrellas, which have no public adoption yet; any external caller fails loudly at compile time, pointed at the replacement.
- `DistributedMessageRegistry` builds its routing-target map through the Kernel 1.1.0 `OnMessageDiscovered` hook instead of a second private assembly scan — one enumeration now populates definitions, identity resolution, and routing. Behavioral footnote: an *unversioned* type carrying `[DistributedMessageTarget]` no longer lands in the target map — such a type cannot obtain a definition, so no shipping path ever reached `GetTargetFor` with it, and the miss fallback remains `Topic`; the Kernel scanner now warns about exactly this at startup.
- Re-pinned `Cirreum.Kernel` → 1.1.0 (the `MessageDiscovery`/`ResolveType`/`OnMessageDiscovered` surface this release builds on).

## [1.1.1] - 2026-07-05

### Updated

- Updated NuGet packages.

## [1.1.0] - 2026-07-04

### Added

- **`TimeOfDayBatchingPolicy`** + **`TimeOfDayBatchingOptions`** / **`TimeOfDayScalingRule`** — the framework-supplied day-of-week / time-of-day scaling policy the `IBatchingPolicy` documentation describes as usage level 2 (between the pass-through `DefaultBatchingPolicy` and a fully custom policy). The matching rule's scaling factor applies to the channel's base `BatchFillWaitTime` (capacity passes through); windows may wrap past midnight; the schedule declares an explicit `TimeZoneInfo` (defaulting to server-local) so containers running UTC can follow a business time zone. Configured in code via the runtime composition callback (`AddMessaging(m => m.UseTimeOfDayBatching(o => ...))` in `Cirreum.Runtime.Messaging`).
- **`DistributedMessageEnvelope.ResolveMessageType()`** — resolves the envelope's captured CLR type, or `null` when unavailable in the process. Handles both the new assembly-hinted format and bare full names from older producers (falls back to probing loaded assemblies). Receivers should prefer this over raw `Type.GetType(envelope.MessageType)`.
- **First test project** — `tests/Cirreum.Messaging.Distributed.Tests` covering the batching policies, envelope creation/round-trip/type-resolution, and registry target routing.

### Fixed

- **Registry target routing silently defaulted to `Topic` for app-defined message types.** `DistributedMessageRegistry.InitializeAsync` resolved captured type names with `Type.GetType(fullName)`, which only finds types in this assembly or the core library — so `[DistributedMessageTarget(MessageTarget.Queue)]` on a message type in any other assembly was ignored. The target map is now built from a direct assembly-scan pass.
- **Envelope type names were not resolvable across assemblies.** `Create` stamped a bare `Type.FullName`, which receivers cannot resolve for app-defined types (inherited from the pre-reset implementation, where only framework-internal message types happened to work). Envelopes now stamp `FullName, AssemblySimpleName` — deliberately without version/culture/token so producer/consumer assembly-version drift doesn't break resolution — and `DeserializeMessage` resolves via `ResolveMessageType()`, which also accepts the legacy bare-full-name format.

## [1.0.0] - 2026-06-05

### Added

- Initial release. Cirreum.Messaging.Distributed is the distributed-envelope foundation of the Messaging track, established as part of the **Cirreum 1.0 Foundation Reset** wave.
- Absorbs distributed-envelope content from former `Cirreum.Core 5.x`:
  - **Envelope types** — `DistributedMessage`, `DistributedMessageEnvelope`, `DistributedMessageHandler`, `DistributedMessagePriority`, `DistributedMessageReceived`
  - **Registry + scanning** — `IMessageRegistry`, `MessageDefinition`, `MessageDefinitionAttribute`, `MessageProperty`, `MessageRegistryBase`, `MessageScanner`, `MessageScannerLogger`, `MessageTarget`
  - **Transport contracts** — `IDistributedTransportPublisher`, `EmptyTransportPublisher`
  - **Node identity** — `INodeIdProvider`, `DefaultNodeIdProvider`
  - **Options** — `BackgroundDeliveryOptions`, `DistributionOptions`, `ReceiverOptions`, `SenderOptions`, `TimeBatchingProfile`, `TimeBatchingValidation`, `TimeScalingRule`
  - **Metrics** — `IMessagingMetricsService`

  See [`MIGRATION-v1.md`](MIGRATION-v1.md) for migration from `Cirreum.Core 5.x`.
