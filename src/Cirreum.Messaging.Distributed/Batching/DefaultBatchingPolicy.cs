namespace Cirreum.Messaging.Batching;

/// <summary>
/// Default no-op <see cref="IBatchingPolicy"/>. Returns the channel's configured base
/// values without adjustment. Registered automatically by the framework when no other
/// policy is configured for a channel.
/// </summary>
public sealed class DefaultBatchingPolicy : IBatchingPolicy {

	/// <inheritdoc/>
	public BatchingDecision Evaluate(BatchingContext context) =>
		new(context.BaseFillWaitTime, context.BaseBatchCapacity);

}
