# Cirreum.Messaging.Distributed 1.0.0

## Summary

Initial release of `Cirreum.Messaging.Distributed` — the distributed-envelope
foundation of the Messaging track, established as part of the **Cirreum 1.0
Foundation Reset** wave. It is the sibling of `Cirreum.Messaging` (generic
queue/topic/transport abstractions) and composes on top of it.

## What's included

Absorbs the distributed-envelope content from former `Cirreum.Core 5.x`:

- **Envelope types** — `DistributedMessage`, `DistributedMessageEnvelope`,
  `DistributedMessageHandler`, `DistributedMessagePriority`, `DistributedMessageReceived`
- **Registry + scanning** — `IMessageRegistry`, `MessageDefinition`,
  `MessageDefinitionAttribute`, `MessageProperty`, `MessageRegistryBase`,
  `MessageScanner`, `MessageScannerLogger`, `MessageTarget`
- **Transport contracts** — `IDistributedTransportPublisher`, `EmptyTransportPublisher`
- **Node identity** — `INodeIdProvider`, `DefaultNodeIdProvider`
- **Options** — `BackgroundDeliveryOptions`, `DistributionOptions`, `ReceiverOptions`,
  `SenderOptions`, `TimeBatchingProfile`, `TimeBatchingValidation`, `TimeScalingRule`
- **Metrics** — `IMessagingMetricsService`

## Dependencies

- `Cirreum.Kernel` `1.0.1` (published NuGet package).

No dependency on the serialization chain (`Cirreum.Result` / `Cirreum.Contracts`).

## Migration

There is no prior version to migrate from. Apps consuming the distributed-envelope
content from `Cirreum.Core 5.x` install this package alongside `Cirreum.Messaging`;
the `Cirreum.Messaging.*` namespaces are preserved. See
[`MIGRATION-v1.md`](MIGRATION-v1.md).
