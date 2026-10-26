# Cirreum.Messaging.Distributed 2.1.0 — The envelope carries its subject

## Why this release exists

A distributed message said who *produced* it (`ProducerId`) but not on whose behalf it was
published. On the receiving side every handler ran as nobody: there was no subject to run as,
and no slot on the wire to carry one.

With host invocations arriving in `Cirreum.Contracts` 6.2.0, a receiver can now run a
message's handlers as a specific subject. This release gives the envelope a place to say
which one.

## What's new

### `DistributedMessageEnvelope.Subject`

```csharp
public DistributedMessageSubject? Subject { get; init; }

public sealed record DistributedMessageSubject(
    string Id,
    string Name,
    string Scheme,
    IReadOnlyList<string> Roles);
```

Nullable, init-only, additive on the JSON round-trip. An older producer sends nothing and an
older consumer ignores it, so the wire is compatible in both directions.
`DistributedMessageEnvelope.Create` takes the subject as an optional trailing parameter.

The delivery engine in `Cirreum.Runtime.Messaging` fills it from the current invocation's
principal and effective scheme when one is current and authenticated.

### `ReceiverOptions.TrustEnvelopeSubject`

Off by default. When set, a receiver runs a message's handlers as the subject the envelope
carries, with its scheme as the origin scheme and its roles minted for the dispatch. When
unset, handlers run as the receiving host.

The subject is the **producer's assertion**, verified only by the trust between producer and
consumer on the channel. Within one application's nodes that is the same trust the self-echo
skip already rests on. Across applications it is not: a receiver that honours another
application's asserted roles has delegated its authorization to that application. Turn it on
per receiver, deliberately.

## Compatibility

Additive. The private envelope constructor gained a parameter; the public surface did not
change shape. Consumers on 2.0.6 compile unchanged.

## See also

- `docs/CHANGELOG.md` — `[2.1.0]`
- `Cirreum.Contracts` 6.2.0 release notes — host invocations
