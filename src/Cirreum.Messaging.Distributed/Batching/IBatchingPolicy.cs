namespace Cirreum.Messaging.Batching;

/// <summary>
/// Strategy abstraction for dynamic batching behavior on a distributed-messaging
/// channel. Replaces the hardcoded time-of-day batching profile system from
/// <c>Cirreum.Core 5.x</c>; the framework's default implementation is a no-op (returns
/// the channel's configured base values), apps may inject custom policies for
/// time-of-day, traffic-aware, queue-depth-aware, or business-signal-aware batching.
/// </summary>
/// <remarks>
/// <para>
/// Registered as a singleton per channel via the runtime composition extension. The framework's
/// background-delivery loop calls <see cref="Evaluate"/> at each batching tick and applies
/// the returned <see cref="BatchingDecision"/>.
/// </para>
/// <para>
/// Three usage levels: (1) default no-op — most apps register no policy and inherit
/// channel base values; (2) framework-supplied <c>TimeOfDayBatchingPolicy</c> via the
/// <c>UseTimeOfDayBatching(...)</c> fluent builder — for apps that want simple
/// day-of-week / time-of-day scaling; (3) custom <see cref="IBatchingPolicy"/>
/// implementation — for apps with sophisticated needs (queue-depth-aware, traffic-
/// aware, integrated with business signals like active marketing campaigns).
/// </para>
/// </remarks>
public interface IBatchingPolicy {

	/// <summary>
	/// Evaluates the current context and returns the batching parameters the dispatcher
	/// should use for the next tick.
	/// </summary>
	/// <param name="context">Snapshot of the channel's configured base values plus
	/// observable runtime signals.</param>
	BatchingDecision Evaluate(BatchingContext context);

}
