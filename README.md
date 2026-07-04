# Cirreum.Messaging.Distributed

[![NuGet Version](https://img.shields.io/nuget/v/Cirreum.Messaging.Distributed.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.Messaging.Distributed/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cirreum.Messaging.Distributed.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.Messaging.Distributed/)
[![GitHub Release](https://img.shields.io/github/v/release/cirreum/Cirreum.Messaging.Distributed?style=flat-square&labelColor=1F1F1F&color=FF3B2E)](https://github.com/cirreum/Cirreum.Messaging.Distributed/releases)
[![License](https://img.shields.io/badge/license-MIT-F2F2F2?style=flat-square&labelColor=1F1F1F)](https://github.com/cirreum/Cirreum.Messaging.Distributed/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-003D8F?style=flat-square&labelColor=1F1F1F)](https://dotnet.microsoft.com/)

**Distributed-envelope orchestration for the Cirreum Messaging track — sibling to Cirreum.Messaging (transport).**

## Overview

**Cirreum.Messaging.Distributed** is the distributed-envelope foundation of the Messaging track. It layers distributed message orchestration (typed envelopes, registry, scanning, transport publishing contracts, options) on top of the transport abstractions provided by `Cirreum.Messaging`.

Cirreum.Messaging.Distributed contains:

- **Envelope types** — `DistributedMessage`, `DistributedMessageEnvelope` (with `ResolveMessageType()` for receiver-side type resolution), `DistributedMessageHandler`, `DistributedMessagePriority`, `DistributedMessageReceived`
- **Routing** — `DistributedMessageTargetAttribute` + `MessageTarget` (queue vs topic, per message type; pairs with `[MessageVersion]` from Cirreum.Kernel)
- **Registry** — `IDistributedMessageRegistry` / `DistributedMessageRegistry` (the channel registry over the Kernel's `IMessageRegistry<TBase>` primitive, adding per-type target lookup)
- **Transport contracts** — `IDistributedTransportPublisher<TBase>`, `EmptyTransportPublisher<TBase>`
- **Batching strategy** — `IBatchingPolicy` + `BatchingContext` / `BatchingDecision`, the pass-through `DefaultBatchingPolicy`, and the framework-supplied `TimeOfDayBatchingPolicy` (+ `TimeOfDayBatchingOptions` / `TimeOfDayScalingRule`)
- **Node identity** — `INodeIdProvider`, `DefaultNodeIdProvider`
- **Options** — `DistributedMessagingOptions`, `BackgroundDeliveryOptions`, `ReceiverOptions`
- **Metrics** — `IMessagingMetricsService`

This package is the model; the runtime delivery engine (outbound Conductor bridge, batch processor, transport publisher, inbound receiver) lives in `Cirreum.Runtime.Messaging`. It is also pulled transitively by tracks that publish or consume distributed envelopes (e.g., cross-process auth event propagation).

## Relationship to Cirreum.Messaging

`Cirreum.Messaging` (existing) provides generic queue/topic/transport abstractions (`IMessagingQueue`, `IMessagingTopicSender`, `IMessagingClient`, etc.). `Cirreum.Messaging.Distributed` (this package) provides the higher-level distributed-envelope orchestration that composes on top. The two are peers; both reference Cirreum.Kernel directly; composition happens where concrete transports wire to concrete envelope flows.

## Where it fits

```
Base                  — Cirreum.Kernel, Cirreum.Result, Cirreum.Exceptions
Common                ← Cirreum.Messaging.Distributed lives here (peer of Cirreum.Messaging,
                        Cirreum.Common, etc.)
Above                 — Provider tracks compose on top
```

## Versioning

Cirreum.Messaging.Distributed follows [Semantic Versioning](https://semver.org/).

## License

MIT — see [LICENSE](LICENSE).

---

**Cirreum Foundation Framework**  
*Layered simplicity for modern .NET*
