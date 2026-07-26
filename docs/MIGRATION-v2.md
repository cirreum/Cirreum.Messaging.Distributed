# Cirreum.Messaging.Distributed v1 → v2 Migration

## Why v2

`Cirreum.Kernel` 2.0.0 renames Conductor's publish/subscribe markers — `INotification` →
`IDomainEvent`, `INotificationHandler<T>` → `IDomainEventHandler<T>`. Cirreum used "notification"
for two unrelated concepts: in-application publish/subscribe, and the human-facing state family a
client binds to in order to show a person something.

`DistributedMessage` extends that marker so a distributed message can be published and handled
through Conductor like any other event, so this package follows. The rename is mechanical — the
wire format, envelope, versioning, and routing are untouched.

## Breaking Changes — Find/Replace Table

| Before | After |
|---|---|
| `DistributedMessage : INotification` | `DistributedMessage : IDomainEvent` |
| `INotificationHandler<DistributedMessageReceived<T>>` | `IDomainEventHandler<DistributedMessageReceived<T>>` |
| `HandleAsync(notification, …)` | `HandleAsync(domainEvent, …)` |

## Migration Walkthrough

### Consumers

A receiving handler changes its interface and nothing else:

```csharp
// Before
public sealed class OrderPlacedConsumer
	: INotificationHandler<DistributedMessageReceived<OrderPlacedV1>> {

	public Task HandleAsync(
		DistributedMessageReceived<OrderPlacedV1> notification,
		CancellationToken cancellationToken) {
		var order = notification.Message;
	}
}

// After
public sealed class OrderPlacedConsumer
	: IDomainEventHandler<DistributedMessageReceived<OrderPlacedV1>> {

	public Task HandleAsync(
		DistributedMessageReceived<OrderPlacedV1> domainEvent,
		CancellationToken cancellationToken) {
		var order = domainEvent.Message;
	}
}
```

### Publishers

No change. A message type still declares `: DistributedMessage` with `[MessageVersion]`, and
`IPublisher.PublishAsync(msg)` still fans it out.

### Local-reaction handlers

The guidance in the README is unaffected but its vocabulary shifts. A **plain domain event** (a
type that is *not* `: DistributedMessage`) handled with `IDomainEventHandler<T>` remains the way to
react in-process; `DistributedMessage` remains reserved for work that must leave the process.

## What Didn't Change

- The wire format, `DistributedMessageEnvelope`, and every application property
- `[MessageVersion]` identity and schema versioning
- `[DistributedMessageTarget]` queue/topic routing
- `DistributedMessagingOptions` and every configuration key
- `IBatchingPolicy`, `IMessagingMetricsService`, and the messaging metric names

A v1 publisher and a v2 consumer remain wire-compatible in both directions — this release changes
no bytes on the broker.

## Downstream Package Impact

`Cirreum.Runtime.Messaging` takes its own major for the same reason. Applications that define
message types need no change beyond a re-pin; applications with **consumers** need the one-line
interface change above.
