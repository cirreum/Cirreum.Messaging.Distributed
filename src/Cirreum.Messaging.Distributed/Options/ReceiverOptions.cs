namespace Cirreum.Messaging.Options;

/// <summary>
/// Configuration for the inbound side of a distributed-messaging channel — what
/// transport instance to consume from and what queue/subscription source(s) to bind.
/// </summary>
/// <remarks>
/// <para>
/// Presence of valid configuration triggers receiver registration in the runtime;
/// absence leaves the receiver unregistered (the default state, equivalent to
/// "this process is send-only on this channel").
/// </para>
/// <para>
/// <b>Queue vs Topic+Subscription semantics:</b>
/// </para>
/// <list type="bullet">
///   <item><description><see cref="QueueName"/> — competing consumers; exactly one consumer processes each message. Used for work distribution.</description></item>
///   <item><description><see cref="TopicName"/> + <see cref="SubscriptionName"/> — broadcast; each subscription receives a copy. Used for cross-head event reactions. Subscription name must be unique per deployed head so each head receives its own copy.</description></item>
/// </list>
/// </remarks>
public sealed class ReceiverOptions {

	/// <summary>Configuration section name.</summary>
	public const string ConfigurationName = "Receiver";

	/// <summary>The transport instance key (e.g., a keyed <c>IMessagingClient</c> registration).</summary>
	public required string InstanceKey { get; init; }

	/// <summary>
	/// Queue to consume from. Competing-consumer semantics — exactly one consumer
	/// processes each message. Optional; leave <see langword="null"/> if topic-only.
	/// </summary>
	public string? QueueName { get; init; }

	/// <summary>
	/// Topic to subscribe to. Pair with <see cref="SubscriptionName"/>. Optional;
	/// leave <see langword="null"/> if queue-only.
	/// </summary>
	public string? TopicName { get; init; }

	/// <summary>
	/// Subscription name on <see cref="TopicName"/>. Unique per deployed head so every
	/// head receives its own copy of every published message. Required when
	/// <see cref="TopicName"/> is set.
	/// </summary>
	public string? SubscriptionName { get; init; }

	/// <summary>
	/// Maximum concurrent in-flight handler invocations per source. Default <c>1</c>
	/// preserves FIFO ordering within a subscription/queue.
	/// </summary>
	public int MaxConcurrency { get; init; } = 1;

	/// <summary>Number of messages to prefetch from the broker. Default 10.</summary>
	public int PrefetchCount { get; init; } = 10;

	/// <summary>
	/// How long the receiver auto-renews a message's broker-side lock while a handler
	/// is executing. Default 5 minutes.
	/// </summary>
	public TimeSpan MaxAutoLockRenewalDuration { get; init; } = TimeSpan.FromMinutes(5);

	/// <summary>
	/// Time the receiver waits for in-flight handlers on graceful shutdown before
	/// cancelling. Default 30 seconds.
	/// </summary>
	public TimeSpan GracefulShutdownTimeout { get; init; } = TimeSpan.FromSeconds(30);

}
