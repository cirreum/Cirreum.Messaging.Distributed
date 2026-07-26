namespace Cirreum.Messaging;

using Cirreum.Conductor;
using Microsoft.Extensions.Logging;

/// <summary>
/// Optional abstract base for handlers of a specific <typeparamref name="TMessage"/>.
/// Implements <see cref="IDomainEventHandler{TDomainEvent}"/> (from Cirreum.Kernel)
/// so the handler plugs directly into Conductor's publisher pipeline.
/// </summary>
/// <typeparam name="TMessage">The distributed message type being handled. Must inherit
/// <see cref="DistributedMessage"/>.</typeparam>
/// <remarks>
/// <para>
/// Apps can implement <see cref="IDomainEventHandler{TDomainEvent}"/> directly if they
/// don't need a base class. This abstract is offered as a convenience for handlers that
/// want structured logging and a hook-shaped processing override.
/// </para>
/// </remarks>
public abstract class DistributedMessageHandler<TMessage>(
	ILogger<DistributedMessageHandler<TMessage>> logger
) : IDomainEventHandler<TMessage> where TMessage : DistributedMessage {

	/// <summary>
	/// Logger for derived handler use.
	/// </summary>
	protected ILogger<DistributedMessageHandler<TMessage>> Logger { get; } = logger;

	/// <inheritdoc/>
	public Task HandleAsync(
		TMessage notification,
		CancellationToken cancellationToken = default) =>
		this.ProcessAsync(notification, cancellationToken);

	/// <summary>
	/// Processes the message. Implementations should be idempotent — the same message
	/// may be redelivered.
	/// </summary>
	protected abstract Task ProcessAsync(TMessage message, CancellationToken cancellationToken);

}
