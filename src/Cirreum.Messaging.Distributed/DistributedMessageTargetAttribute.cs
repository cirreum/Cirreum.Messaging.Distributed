namespace Cirreum.Messaging;

/// <summary>
/// Optional within-channel routing hint on a <see cref="DistributedMessage"/>-derived
/// type. Pairs with <c>[MessageVersion]</c> from Cirreum.Kernel (which carries the
/// versioning concern).
/// </summary>
/// <param name="target">Whether this message type prefers queue (point-to-point) or
/// topic (publish-subscribe) delivery semantics within its configured channel.</param>
/// <remarks>
/// <para>
/// The channel itself is determined by the message's base type
/// (<see cref="DistributedMessage"/>), and its transport is configured where the delivery
/// engine is installed (<c>AddMessaging()</c> in <c>Cirreum.Runtime.Messaging</c>). This
/// attribute is a routing hint *within* that channel — does this message type fan out to
/// multiple subscribers, or does a single consumer process it?
/// </para>
/// <para>
/// When omitted, channels default to <see cref="MessageTarget.Topic"/> — pub/sub is
/// the more common default for domain events.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class DistributedMessageTargetAttribute(MessageTarget target) : Attribute {

	/// <summary>
	/// The delivery target for this message type within its channel.
	/// </summary>
	public MessageTarget Target { get; } = target;

}
