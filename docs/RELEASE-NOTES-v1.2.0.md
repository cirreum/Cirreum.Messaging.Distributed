# Cirreum.Messaging.Distributed 1.2.0 — the envelope stops resolving types; the channel owns its serialization

## Why this release exists

Three design debts lived in this package, all inherited from legacy Core, and all fixed
by leaning on `Cirreum.Kernel 1.1.0`.

**Wire-driven type resolution.** The envelope stamped a CLR type name and the receiver
resolved *whatever type the wire named* (`ResolveMessageType()` — `Type.GetType` over the
assembly-hinted name, then a loaded-assembly probe). That let the sender choose the
deserialization target, applied the `DistributedMessage` family check only *after*
deserializing, and could attempt assembly loads on hostile input. Kernel 1.1.0 put
identity resolution (`ResolveType`) on the registry base, and the envelope already stamps
`MessageIdentifier`/`MessageVersion` — so the safer pattern (resolve identity against the
receiver's own vetted scan set) became free.

**A dormant, incoherent serializer seam.** The envelope advertised app-chosen payload
formats since legacy Core (`CreateWithSerializer`, the `Func`-taking deserialize
overloads), but no engine leg ever consulted it. It was also incoherent: the envelope is
itself System.Text.Json (`FromJson`), so "pluggable payload format" only ever meant
pluggability for the inner *string* wrapped in a hardcoded-STJ envelope.

**A decorative transport seam.** `IDistributedTransportPublisher<TBase>` was registered
in two places and resolved in none — the outbound Conductor handler injects the delivery
engine directly. Its `EmptyTransportPublisher` no-op was a fossil of the pre-reset design
(where the bridge was always-on) that the reset had already routed around.

## What changed

**Inbound resolution is identity-only.** The receiver resolves
`(MessageIdentifier, MessageVersion)` via the registry and deserializes through the new
`DeserializeMessage(Type)` — the type comes from the receiver's vetted scan set, never
from the wire. `ResolveMessageType()` and the self-resolving `DeserializeMessage`
overloads are removed. The `MessageType` stamp stays on the wire as **diagnostic
metadata** — the operator's best hint on a dead-lettered envelope.

**The channel owns its serialization — System.Text.Json, options decided internally.**
`CreateWithSerializer`, the `Func`-taking `DeserializeMessage` overloads, the never-shipped
`IDistributedPayloadSerializer`, and `FromJson(string, JsonSerializerOptions)` are all
gone — callers no longer supply serialization options anywhere. Both the envelope and its
payload serialize through one internally-owned `JsonSerializerOptions` (the STJ defaults
today), so producer and consumer are always symmetric — exactly as the auth-events channel
serializes STJ inline. A message family owns its wire format end to end; a different format
is a different channel, not a serializer swap or a per-call option.

**Conductor is the outbound seam.** `IDistributedTransportPublisher<TBase>` and
`EmptyTransportPublisher<TBase>` are removed. Publishing a `DistributedMessage` fans out
through Conductor to its handlers — including the delivery engine's outbound handler when
`Cirreum.Runtime.Messaging` is installed. Custom delivery is your own
`INotificationHandler`; nothing leaves the process without a configured transport.

**The registry scans once.** `DistributedMessageRegistry` builds its queue/topic target
map through Kernel 1.1.0's `OnMessageDiscovered` hook instead of a second private assembly
scan — one enumeration populates definitions, identity resolution, and routing.

## Why this is still a minor

Every removal is a member reachable only through the framework's own umbrellas
(`Cirreum.Runtime.Messaging`, transitively `Cirreum.Runtime.Server`), which have no public
adoption yet — the track is prerelease-in-practice, so removals ship in a minor by
explicit decision rather than forcing a major for members nobody calls. An out-of-tree
caller (none is known) fails loudly at compile time, pointed at the replacement. Per the
`MinorRelease` gate, these are recorded under `### Changed`.

## Coordinated downstream work

- `Cirreum.Runtime.Messaging` (minor, next): the receiver resolves inbound types
  identity-first with per-source disposition (dead-letter what was addressed to you on a
  queue; drop-and-log what broadcast past you on a topic); the delivery engine implements
  no transport interface (the outbound handler injects it directly); classes rename to the
  Publisher / Sender / Receiver vocabulary (`OutboundDistributedMessageHandler` →
  `DistributedMessageSender`, `DefaultTransportPublisher` →
  `DistributedMessageDeliveryEngine`).
- `Cirreum.Services.Serverless` (patch): drops its now-dangling `EmptyTransportPublisher`
  registration.

## Compatibility

Additive for every engine-riding consumer — apps publish and receive through
`Cirreum.Runtime.Messaging`, whose surface is unchanged until its own release. Only code
that called the envelope's removed members or the transport interface directly is
affected, and the replacements are mechanical. Envelopes already in flight are unaffected;
the wire format is unchanged — only who resolves the type moved.

## See also

- ADR-0029 — Message Track: Type Capture and Identity-Based Inbound Resolution
- `Cirreum.Kernel 1.1.0` release notes — the registry surface this builds on
- `docs/CHANGELOG.md` — the enumerated change list
