namespace Cirreum.Messaging.Options;

/// <summary>
/// Per-channel configuration for distributed messaging. One options instance is bound
/// per registered channel from its own configuration section (the delivery engine binds
/// it where installed, via <c>AddMessaging()</c> in <c>Cirreum.Runtime.Messaging</c>).
/// </summary>
/// <remarks>
/// <para>
/// The per-channel model supports independent transport configuration, subscription
/// naming, and routing destinations — e.g. auth events on a dedicated Service Bus
/// namespace separate from domain events on the main namespace, with each channel
/// scaling and being secured independently.
/// </para>
/// </remarks>
public sealed class DistributedMessagingOptions {

	/// <summary>Configuration section root name (under the channel's own root).</summary>
	public const string ConfigurationName = "Distributed";

	/// <summary>
	/// Transport instance key — typically a keyed <c>IMessagingClient</c> registration
	/// from the transport package (e.g., <c>"app-primary"</c>, <c>"auth-secure"</c>).
	/// </summary>
	public string? InstanceKey { get; set; }

	/// <summary>
	/// Default queue name for this channel. Messages decorated with
	/// <c>[DistributedMessageTarget(MessageTarget.Queue)]</c> route here.
	/// </summary>
	public string QueueName { get; set; } = "";

	/// <summary>
	/// Default topic name for this channel. Messages decorated with
	/// <c>[DistributedMessageTarget(MessageTarget.Topic)]</c> (and the implicit default
	/// when no target attribute is present) route here.
	/// </summary>
	public string TopicName { get; set; } = "";

	/// <summary>
	/// Background-delivery tuning for this channel.
	/// </summary>
	public BackgroundDeliveryOptions BackgroundDelivery { get; set; } = new();

	/// <summary>
	/// Optional inbound configuration. When unset, the channel is publish-only on this
	/// process.
	/// </summary>
	public ReceiverOptions? Receiver { get; set; }

}
