namespace Cirreum.Messaging;

using System.Text.Json.Serialization;

/// <summary>
/// Priority of a distributed message when using background (batched) delivery.
/// Messages using synchronous delivery bypass the prioritization system.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DistributedMessagePriority {

	/// <summary>
	/// Standard priority. Default for most application events.
	/// </summary>
	Standard = 0,

	/// <summary>
	/// Time-sensitive priority. Subject to rate limiting per the channel's
	/// <c>PriorityMessageRateLimit</c> setting — messages exceeding the budget downgrade
	/// to <see cref="Standard"/>.
	/// </summary>
	TimeSensitive = 1,

	/// <summary>
	/// System-level priority. Reserved for framework infrastructure messages — health
	/// monitoring, circuit-breaker notifications, and similar concerns. Subject to the
	/// same rate-limit budget as <see cref="TimeSensitive"/>.
	/// </summary>
	SystemHealth = 2

}
