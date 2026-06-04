namespace Cirreum.Messaging;

/// <summary>
/// Publishes wire-format <see cref="DistributedMessageEnvelope"/> payloads for the
/// channel identified by <typeparamref name="TBase"/> to its configured transport
/// destination.
/// </summary>
/// <typeparam name="TBase">The channel identity. Apps register one publisher per channel
/// via <c>AddDistributedMessaging&lt;TBase&gt;(...)</c>; the framework's outbound
/// interceptor resolves the right publisher based on the message's runtime type.
/// Examples: <c>DistributedMessage</c> for the generic domain-events channel,
/// <c>IAuthenticationEvent</c> for the auth-events channel.</typeparam>
/// <remarks>
/// <para>
/// Each registered channel is independently configured — connection string / namespace,
/// queue name, topic name, batching policy, retry policy. Apps wanting full isolation
/// between channels (different Service Bus namespaces, different broker types, different
/// security boundaries) get it by registering separate <typeparamref name="TBase"/> types.
/// </para>
/// <para>
/// Transport packages (e.g., <c>Cirreum.Messaging.AzureServiceBus</c>) provide the
/// concrete implementation. This surface defines the contract.
/// </para>
/// </remarks>
public interface IDistributedTransportPublisher<TBase> where TBase : class {

	/// <summary>
	/// Sends the envelope using the channel's configured delivery semantics. Implementations
	/// honor the <see cref="DistributedMessageTargetAttribute"/> on the original message
	/// type (when present) to route between queue and topic destinations within the channel.
	/// </summary>
	/// <param name="envelope">The wire-format envelope to send.</param>
	/// <param name="target">Within-channel routing hint for this specific message type.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task PublishAsync(
		DistributedMessageEnvelope envelope,
		MessageTarget target,
		CancellationToken cancellationToken = default);

}
