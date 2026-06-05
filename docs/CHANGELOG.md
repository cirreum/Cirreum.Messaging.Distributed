# Changelog

All notable changes to **Cirreum.Messaging.Distributed** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

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
