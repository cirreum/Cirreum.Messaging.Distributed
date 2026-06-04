namespace Cirreum.Messaging.Batching;

/// <summary>
/// The batching parameters an <see cref="IBatchingPolicy"/> decides to use for the
/// current tick.
/// </summary>
/// <param name="FillWaitTime">How long the dispatcher should wait collecting messages
/// into the next batch before sending an incomplete batch.</param>
/// <param name="BatchCapacity">Maximum number of messages to include in the next batch
/// when it fills before <see cref="FillWaitTime"/> elapses.</param>
/// <param name="Reason">Optional human-readable rationale for this decision. Surfaced
/// in traces and operator logs to make adaptive behavior observable.</param>
public sealed record BatchingDecision(
	TimeSpan FillWaitTime,
	int BatchCapacity,
	string? Reason = null);
