namespace Cirreum.Messaging.Options;

/// <summary>
/// Configuration for background (batched, asynchronous) message delivery within a
/// single distributed-messaging channel.
/// </summary>
/// <remarks>
/// <para>
/// Carries only flat operational tuning knobs suitable for <c>appsettings.json</c>.
/// Sophisticated dynamic batching behavior (time-of-day scaling, traffic-aware adjustment)
/// lives in <c>IBatchingPolicy</c> implementations configured in code via the
/// <c>UseBatchingPolicy&lt;T&gt;(...)</c> or <c>UseTimeOfDayBatching(...)</c> fluent
/// builders — outside the appsettings surface where validation, IDE support, and
/// compile-time safety are more useful.
/// </para>
/// </remarks>
public sealed class BackgroundDeliveryOptions {

	/// <summary>
	/// Configuration section name for binding under the channel's options root.
	/// </summary>
	public const string ConfigurationName = "BackgroundDelivery";

	/// <summary>
	/// Whether background delivery is the default when a message instance does not
	/// explicitly set <see cref="DistributedMessage.UseBackgroundDelivery"/>. Apps that
	/// want fully synchronous-by-default delivery set this to <see langword="false"/>.
	/// </summary>
	public bool UseBackgroundDeliveryByDefault { get; set; }

	/// <summary>
	/// Maximum combined rate (per minute) of <c>TimeSensitive</c> + <c>SystemHealth</c>
	/// priority messages. Messages exceeding the budget downgrade to
	/// <see cref="DistributedMessagePriority.Standard"/>. Prevents priority inflation.
	/// Default: 100/min.
	/// </summary>
	public int PriorityMessageRateLimit { get; set; } = 100;

	/// <summary>
	/// Maximum age in seconds before a buffered message's effective priority is
	/// promoted one level. Prevents starvation under sustained high-priority traffic.
	/// Set to 0 to disable. Default: 60 seconds.
	/// </summary>
	public int PriorityAgePromotionThreshold { get; set; } = 60;

	/// <summary>
	/// Number of consecutive failures before the circuit breaker opens and the
	/// dispatcher stops attempting sends. Default: 5.
	/// </summary>
	public int CircuitBreakerThreshold { get; set; } = 5;

	/// <summary>
	/// How long the circuit stays open before attempting half-open recovery.
	/// Default: 1 minute.
	/// </summary>
	public TimeSpan CircuitResetTimeout { get; set; } = TimeSpan.FromMinutes(1);

	/// <summary>
	/// Maximum number of messages buffered awaiting send. Higher values absorb traffic
	/// spikes at the cost of memory. Default: 1000.
	/// </summary>
	public int QueueCapacity { get; set; } = 1000;

	/// <summary>
	/// Base maximum number of messages per outbound batch. Effective value may be
	/// adjusted at runtime by the configured <c>IBatchingPolicy</c>. Default: 10.
	/// </summary>
	public int BatchCapacity { get; set; } = 10;

	/// <summary>
	/// Base maximum wait for a batch to fill before sending it incomplete. Effective
	/// value may be adjusted at runtime by the configured <c>IBatchingPolicy</c>.
	/// Default: 50ms.
	/// </summary>
	public TimeSpan BatchFillWaitTime { get; set; } = TimeSpan.FromMilliseconds(50);

}
