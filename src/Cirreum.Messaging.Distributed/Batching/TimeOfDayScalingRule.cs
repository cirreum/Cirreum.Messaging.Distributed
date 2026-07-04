namespace Cirreum.Messaging.Batching;

/// <summary>
/// One day-of-week / hour-window rule in a <see cref="TimeOfDayBatchingOptions"/>
/// schedule, mapping a recurring time period to a batching scaling factor.
/// </summary>
/// <remarks>
/// Rules are evaluated in order; the first rule matching the current day and hour wins.
/// A window may span midnight by setting <see cref="StartHour"/> greater than
/// <see cref="EndHour"/> (e.g., 22 → 6).
/// </remarks>
public sealed class TimeOfDayScalingRule {

	/// <summary>
	/// The days of the week this rule applies to. Must contain at least one day.
	/// </summary>
	public IList<DayOfWeek> Days { get; set; } = [];

	/// <summary>
	/// Inclusive starting hour of the window, 0–23.
	/// </summary>
	public int StartHour { get; set; }

	/// <summary>
	/// Exclusive ending hour of the window, 1–24. When less than <see cref="StartHour"/>,
	/// the window wraps past midnight.
	/// </summary>
	public int EndHour { get; set; } = 24;

	/// <summary>
	/// The factor applied to the channel's base <c>BatchFillWaitTime</c> while this rule
	/// is active. Less than 1.0 shortens the fill wait (high-traffic periods — send
	/// sooner); greater than 1.0 lengthens it (quiet periods — collect longer). Must be
	/// greater than zero.
	/// </summary>
	public double ScalingFactor { get; set; } = 1.0;

	/// <summary>
	/// Optional human-readable rationale. Surfaced as the <see cref="BatchingDecision.Reason"/>
	/// in traces and operator logs while the rule is active.
	/// </summary>
	public string? Description { get; set; }

}