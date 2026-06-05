# Migration to Cirreum.Messaging.Distributed v1.0.0

## Context

This is the **initial release** of `Cirreum.Messaging.Distributed`. There is no
prior version of this package to migrate from. The package was established as part
of the **Cirreum 1.0 Foundation Reset** wave, absorbing the distributed-envelope
content that previously lived in `Cirreum.Core 5.x`.

## Migrating from `Cirreum.Core 5.x`

Apps that consumed the distributed-envelope types from `Cirreum.Core 5.x` migrate
by installing `Cirreum.Messaging.Distributed` alongside the existing
`Cirreum.Messaging` (transport) package:

```xml
<PackageReference Include="Cirreum.Messaging" Version="1.0.*" />
<PackageReference Include="Cirreum.Messaging.Distributed" Version="1.0.0" />
```

The `Cirreum.Messaging.*` namespaces are **preserved**, so existing `using`
directives and type references continue to resolve once the package reference is
in place. No source changes are required beyond adding the package.

### Types that moved into this package

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

`Cirreum.Messaging.Distributed` depends only on `Cirreum.Kernel` (`1.0.1`),
consumed as a published NuGet package. It does not depend on the serialization
chain (`Cirreum.Result` / `Cirreum.Contracts`).
