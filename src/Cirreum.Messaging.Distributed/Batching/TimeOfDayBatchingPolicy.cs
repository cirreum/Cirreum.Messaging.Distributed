namespace Cirreum.Messaging.Batching;

/// <summary>
/// Framework-supplied <see cref="IBatchingPolicy"/> for simple day-of-week /
/// time-of-day scaling — the middle ground between the pass-through
/// <see cref="DefaultBatchingPolicy"/> and a fully custom policy.
/// </summary>
/// <remarks>
/// <para>
/// The matching rule's <see cref="TimeOfDayScalingRule.ScalingFactor"/> (or the
/// schedule's default) is applied to the channel's base
/// <see cref="BatchingContext.BaseFillWaitTime"/>; the batch capacity passes through
/// unchanged. Rule windows are evaluated in the schedule's configured
/// <see cref="TimeOfDayBatchingOptions.TimeZone"/>.
/// </para>
/// <para>
/// Apps register it via the runtime composition callback — e.g.,
/// <c>AddMessaging(m => m.UseTimeOfDayBatching(o => ...))</c>.
/// </para>
/// </remarks>
public sealed class TimeOfDayBatchingPolicy : IBatchingPolicy {

	private readonly TimeOfDayBatchingOptions _options;
	private readonly TimeZoneInfo _timeZone;

	/// <summary>
	/// Initializes the policy with a validated schedule.
	/// </summary>
	/// <param name="options">The schedule configuration.</param>
	/// <exception cref="ArgumentNullException">The options are null.</exception>
	/// <exception cref="ArgumentException">A scaling factor is not greater than zero, a
	/// rule has no days, or a rule's hours fall outside 0–23 (start) / 1–24 (end).</exception>
	public TimeOfDayBatchingPolicy(TimeOfDayBatchingOptions options) {

		ArgumentNullException.ThrowIfNull(options);

		if (options.DefaultScalingFactor <= 0) {
			throw new ArgumentException(
				"DefaultScalingFactor must be greater than zero.",
				nameof(options));
		}

		foreach (var rule in options.Rules) {
			if (rule.Days is not { Count: > 0 }) {
				throw new ArgumentException(
					"Every rule must specify at least one day of the week.",
					nameof(options));
			}
			if (rule.StartHour is < 0 or > 23) {
				throw new ArgumentException(
					$"Rule StartHour {rule.StartHour} must be between 0 and 23.",
					nameof(options));
			}
			if (rule.EndHour is < 1 or > 24) {
				throw new ArgumentException(
					$"Rule EndHour {rule.EndHour} must be between 1 and 24.",
					nameof(options));
			}
			if (rule.ScalingFactor <= 0) {
				throw new ArgumentException(
					"Rule ScalingFactor must be greater than zero.",
					nameof(options));
			}
		}

		this._options = options;
		this._timeZone = options.TimeZone ?? TimeZoneInfo.Local;
	}

	/// <inheritdoc/>
	public BatchingDecision Evaluate(BatchingContext context) {

		var local = TimeZoneInfo.ConvertTime(context.Now, this._timeZone);
		var rule = this.FindMatchingRule(local.DayOfWeek, local.Hour);
		var factor = rule?.ScalingFactor ?? this._options.DefaultScalingFactor;

		if (factor == 1.0) {
			return new(context.BaseFillWaitTime, context.BaseBatchCapacity, rule?.Description);
		}

		var scaledWait = TimeSpan.FromMilliseconds(
			context.BaseFillWaitTime.TotalMilliseconds * factor);

		return new(
			scaledWait,
			context.BaseBatchCapacity,
			rule?.Description ?? FormattableString.Invariant($"Time-of-day scaling {factor:0.##}x"));
	}

	private TimeOfDayScalingRule? FindMatchingRule(DayOfWeek day, int hour) {

		foreach (var rule in this._options.Rules) {

			if (!rule.Days.Contains(day)) {
				continue;
			}

			// A window may wrap past midnight (StartHour > EndHour)
			var matches = rule.StartHour > rule.EndHour
				? hour >= rule.StartHour || hour < rule.EndHour
				: hour >= rule.StartHour && hour < rule.EndHour;

			if (matches) {
				return rule;
			}
		}

		return null;
	}

}