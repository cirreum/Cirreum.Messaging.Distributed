# Changelog

All notable changes to **Cirreum.Messaging.Distributed** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added

- **`TimeOfDayBatchingPolicy`** + **`TimeOfDayBatchingOptions`** / **`TimeOfDayScalingRule`** — the framework-supplied day-of-week / time-of-day scaling policy the `IBatchingPolicy` documentation describes as usage level 2 (between the pass-through `DefaultBatchingPolicy` and a fully custom policy). The matching rule's scaling factor applies to the channel's base `BatchFillWaitTime` (capacity passes through); windows may wrap past midnight; the schedule declares an explicit `TimeZoneInfo` (defaulting to server-local) so containers running UTC can follow a business time zone. Configured in code via the runtime composition callback (`AddMessaging(m => m.UseTimeOfDayBatching(o => ...))` in `Cirreum.Runtime.Messaging`).

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
