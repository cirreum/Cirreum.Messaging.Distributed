# Changelog

All notable changes to **Cirreum.Messaging.Distributed** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

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
