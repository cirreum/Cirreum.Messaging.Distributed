namespace Cirreum.Messaging.Batching;

/// <summary>
/// Schedule configuration for <see cref="TimeOfDayBatchingPolicy"/> — a set of
/// day-of-week / hour-window rules, each scaling the channel's base batch-fill wait
/// time during its period.
/// </summary>
/// <remarks>
/// Configured in code via the runtime composition callback (e.g.,
/// <c>AddMessaging(m => m.UseTimeOfDayBatching(o => ...))</c>) — dynamic batching is a
/// code concern, not an appsettings one.
/// </remarks>
public sealed class TimeOfDayBatchingOptions {

	/// <summary>
	/// The time zone the rule windows are expressed in. Defaults to the server's local
	/// zone — set this explicitly when deploying to containers or regions where server
	/// time is UTC but the traffic pattern follows a business time zone.
	/// </summary>
	public TimeZoneInfo? TimeZone { get; set; }

	/// <summary>
	/// The factor applied when no rule matches the current time. Default 1.0 (base
	/// values pass through unchanged). Must be greater than zero.
	/// </summary>
	public double DefaultScalingFactor { get; set; } = 1.0;

	/// <summary>
	/// The scaling rules, evaluated in order — first match wins.
	/// </summary>
	public IList<TimeOfDayScalingRule> Rules { get; set; } = [];

}