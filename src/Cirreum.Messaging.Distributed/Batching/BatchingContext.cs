namespace Cirreum.Messaging.Batching;

/// <summary>
/// Snapshot of operational signals passed to <see cref="IBatchingPolicy"/> on each
/// evaluation tick. Carries the channel's configured base values plus live observables
/// the policy may consider when deciding current batching behavior.
/// </summary>
/// <param name="Now">The current UTC timestamp.</param>
/// <param name="BaseFillWaitTime">The channel's configured base
/// <c>BackgroundDeliveryOptions.BatchFillWaitTime</c>.</param>
/// <param name="BaseBatchCapacity">The channel's configured base
/// <c>BackgroundDeliveryOptions.BatchCapacity</c>.</param>
/// <param name="CurrentQueueDepth">Number of messages currently buffered awaiting send
/// on this channel.</param>
/// <param name="RecentSendRatePerSecond">Observed send rate over a recent rolling
/// window. Implementations may use this for traffic-aware adjustments.</param>
/// <param name="RecentErrorRate">Observed error rate (failed/total) over a recent
/// rolling window. Implementations may use this for circuit-aware slowdowns.</param>
public sealed record BatchingContext(
	DateTimeOffset Now,
	TimeSpan BaseFillWaitTime,
	int BaseBatchCapacity,
	int CurrentQueueDepth,
	double RecentSendRatePerSecond,
	double RecentErrorRate);
