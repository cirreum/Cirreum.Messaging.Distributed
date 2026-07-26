# Cirreum.Messaging.Distributed 2.0.0 — A Rename, and Nothing on the Wire

## Why this release exists

`Cirreum.Kernel` 2.0.0 renames Conductor's publish/subscribe markers: `INotification` becomes
`IDomainEvent`, and `INotificationHandler<T>` becomes `IDomainEventHandler<T>`. `DistributedMessage`
extends that marker so a message can be published and handled through Conductor like any other
event, so this package follows.

Cirreum had used "notification" for two opposite concepts — in-application publish/subscribe, and
the human-facing state a client binds to in order to show a person something. A distributed-message
consumer reacts to something that happened; it renders nothing for a user, and the old name left
that ambiguous at a glance.

## What changes

Three types now extend the renamed markers: `DistributedMessage`, `DistributedMessageReceived<T>`,
and `DistributedMessageHandler`. For a consuming application that is one line per handler:

```csharp
// Before
public sealed class OrderPlacedConsumer
	: INotificationHandler<DistributedMessageReceived<OrderPlacedV1>> {

	public Task HandleAsync(
		DistributedMessageReceived<OrderPlacedV1> notification,
		CancellationToken cancellationToken) { … }
}

// After
public sealed class OrderPlacedConsumer
	: IDomainEventHandler<DistributedMessageReceived<OrderPlacedV1>> {

	public Task HandleAsync(
		DistributedMessageReceived<OrderPlacedV1> domainEvent,
		CancellationToken cancellationToken) { … }
}
```

Publishers change nothing. A message type still declares `: DistributedMessage` with
`[MessageVersion]`, and `IPublisher.PublishAsync(msg)` still fans it out.

## Nothing moves on the wire

The reason a major here is smaller than it looks, and the thing to check before deploying:

**No bytes change on the broker.** The envelope, application properties, `[MessageVersion]`
identity, routing, serialization, and self-echo stamping are all untouched. A **v1 publisher and a
v2 consumer are wire-compatible in both directions**, so the two can run side by side through a
rolling deployment — no coordinated cutover, no drain, no ordering requirement between services.

Every breaking change in this release is a compile error in your own source tree. None of it is
observable to another process.

## Compatibility

Breaking at compile time only. See [`MIGRATION-v2.md`](MIGRATION-v2.md).

Before upgrading, grep for `INotificationHandler<DistributedMessageReceived` — that is the whole
migration for most applications.

## Coordinated downstream work

Part of the `Cirreum.Kernel` 2.0.0 wave. `Cirreum.Runtime.Messaging` takes a major
(**3.0.0**) for the same reason and carries the same wire guarantee.

## See also

- [`MIGRATION-v2.md`](MIGRATION-v2.md)
- [`CHANGELOG.md`](CHANGELOG.md)
- `Cirreum.Kernel` 2.0.0 release notes — the origin of the rename
