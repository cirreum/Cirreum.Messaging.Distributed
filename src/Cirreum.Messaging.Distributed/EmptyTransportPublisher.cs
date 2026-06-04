namespace Cirreum.Messaging;

using Microsoft.Extensions.Logging;

/// <summary>
/// No-op <see cref="IDistributedTransportPublisher{TBase}"/> implementation. Logs each
/// publish call instead of dispatching to a real transport. Registered by default for
/// any channel where the app has not configured a real transport — keeps local
/// development and tests friction-free, while making absent transport configuration
/// visible in logs rather than silently dropping messages.
/// </summary>
/// <typeparam name="TBase">The channel identity.</typeparam>
public sealed class EmptyTransportPublisher<TBase>(
	ILogger<EmptyTransportPublisher<TBase>> logger
) : IDistributedTransportPublisher<TBase> where TBase : class {

	/// <inheritdoc/>
	public Task PublishAsync(
		DistributedMessageEnvelope envelope,
		MessageTarget target,
		CancellationToken cancellationToken = default) {
		logger.LogInformation(
			"EmptyTransportPublisher<{Channel}> dropping envelope [{Identifier}|v{Version}|{Target}] from {Producer}. " +
			"No real transport is registered for this channel.",
			typeof(TBase).Name,
			envelope.MessageIdentifier,
			envelope.MessageVersion,
			target,
			envelope.ProducerId);
		return Task.CompletedTask;
	}

}
