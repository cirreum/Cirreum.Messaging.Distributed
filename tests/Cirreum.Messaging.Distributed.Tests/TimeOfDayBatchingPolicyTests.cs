namespace Cirreum.Messaging.Distributed.Tests;

using Cirreum.Messaging.Batching;

public class TimeOfDayBatchingPolicyTests {

	/// <summary>
	/// Fixed UTC-5 zone so hour/day math is deterministic regardless of the test host's
	/// local zone or DST rules.
	/// </summary>
	private static readonly TimeZoneInfo UtcMinus5 = TimeZoneInfo.CreateCustomTimeZone(
		"Test UTC-5", TimeSpan.FromHours(-5), "Test UTC-5", "Test UTC-5");

	private static readonly TimeSpan BaseWait = TimeSpan.FromMilliseconds(100);
	private const int BaseCapacity = 10;

	/// <summary>
	/// Builds a context whose UTC instant lands on the given day/hour in the UTC-5 zone.
	/// </summary>
	private static BatchingContext ContextAt(DayOfWeek day, int hour) {
		// 2026-07-06 is a Monday. Walk to the requested day, then add the hour and the
		// zone offset so the *local* (UTC-5) wall clock reads exactly (day, hour).
		var monday = new DateTimeOffset(2026, 7, 6, 0, 0, 0, TimeSpan.Zero);
		var dayOffset = ((int)day - (int)DayOfWeek.Monday + 7) % 7;
		var utc = monday.AddDays(dayOffset).AddHours(hour + 5);
		return new BatchingContext(utc, BaseWait, BaseCapacity, 0, 0.0, 0.0);
	}

	private static TimeOfDayBatchingPolicy Policy(Action<TimeOfDayBatchingOptions> configure) {
		var options = new TimeOfDayBatchingOptions { TimeZone = UtcMinus5 };
		configure(options);
		return new TimeOfDayBatchingPolicy(options);
	}

	[Fact]
	public void MatchingRule_ScalesFillWaitTime_AndPassesCapacityThrough() {
		var policy = Policy(o => o.Rules.Add(new() {
			Days = [DayOfWeek.Friday],
			StartHour = 16,
			EndHour = 23,
			ScalingFactor = 0.5,
			Description = "Friday evening spike"
		}));

		var decision = policy.Evaluate(ContextAt(DayOfWeek.Friday, 18));

		decision.FillWaitTime.Should().Be(TimeSpan.FromMilliseconds(50));
		decision.BatchCapacity.Should().Be(BaseCapacity);
		decision.Reason.Should().Be("Friday evening spike");
	}

	[Fact]
	public void NonMatchingDay_UsesDefaultFactor() {
		var policy = Policy(o => o.Rules.Add(new() {
			Days = [DayOfWeek.Friday],
			StartHour = 16,
			EndHour = 23,
			ScalingFactor = 0.5
		}));

		var decision = policy.Evaluate(ContextAt(DayOfWeek.Tuesday, 18));

		decision.FillWaitTime.Should().Be(BaseWait);
		decision.BatchCapacity.Should().Be(BaseCapacity);
	}

	[Theory]
	[InlineData(15, false)] // before the window
	[InlineData(16, true)]  // inclusive start
	[InlineData(22, true)]  // last in-window hour
	[InlineData(23, false)] // exclusive end
	public void HourWindow_IsInclusiveStart_ExclusiveEnd(int hour, bool expectScaled) {
		var policy = Policy(o => o.Rules.Add(new() {
			Days = [DayOfWeek.Friday],
			StartHour = 16,
			EndHour = 23,
			ScalingFactor = 0.5
		}));

		var decision = policy.Evaluate(ContextAt(DayOfWeek.Friday, hour));

		decision.FillWaitTime.Should().Be(expectScaled
			? TimeSpan.FromMilliseconds(50)
			: BaseWait);
	}

	[Theory]
	[InlineData(23, true)]  // late-night side of the wrap
	[InlineData(3, true)]   // early-morning side of the wrap
	[InlineData(6, false)]  // exclusive end after the wrap
	[InlineData(12, false)] // midday, outside the window
	public void Window_MayWrapPastMidnight(int hour, bool expectScaled) {
		var policy = Policy(o => o.Rules.Add(new() {
			Days = [DayOfWeek.Saturday],
			StartHour = 22,
			EndHour = 6,
			ScalingFactor = 2.0
		}));

		var decision = policy.Evaluate(ContextAt(DayOfWeek.Saturday, hour));

		decision.FillWaitTime.Should().Be(expectScaled
			? TimeSpan.FromMilliseconds(200)
			: BaseWait);
	}

	[Fact]
	public void FirstMatchingRule_Wins() {
		var policy = Policy(o => {
			o.Rules.Add(new() { Days = [DayOfWeek.Monday], StartHour = 0, EndHour = 24, ScalingFactor = 0.5, Description = "first" });
			o.Rules.Add(new() { Days = [DayOfWeek.Monday], StartHour = 0, EndHour = 24, ScalingFactor = 3.0, Description = "second" });
		});

		var decision = policy.Evaluate(ContextAt(DayOfWeek.Monday, 12));

		decision.FillWaitTime.Should().Be(TimeSpan.FromMilliseconds(50));
		decision.Reason.Should().Be("first");
	}

	[Fact]
	public void NoMatch_WithNonUnitDefaultFactor_ScalesByDefault() {
		var policy = Policy(o => o.DefaultScalingFactor = 1.5);

		var decision = policy.Evaluate(ContextAt(DayOfWeek.Wednesday, 12));

		decision.FillWaitTime.Should().Be(TimeSpan.FromMilliseconds(150));
		decision.BatchCapacity.Should().Be(BaseCapacity);
	}

	[Fact]
	public void UnitFactor_ReturnsBaseValuesUnchanged() {
		var policy = Policy(o => o.Rules.Add(new() {
			Days = [DayOfWeek.Monday],
			StartHour = 0,
			EndHour = 24,
			ScalingFactor = 1.0,
			Description = "no-op window"
		}));

		var decision = policy.Evaluate(ContextAt(DayOfWeek.Monday, 12));

		decision.FillWaitTime.Should().Be(BaseWait);
		decision.BatchCapacity.Should().Be(BaseCapacity);
		decision.Reason.Should().Be("no-op window");
	}

	[Fact]
	public void MatchingRule_WithoutDescription_ReportsFactorAsReason() {
		var policy = Policy(o => o.Rules.Add(new() {
			Days = [DayOfWeek.Monday],
			StartHour = 0,
			EndHour = 24,
			ScalingFactor = 0.25
		}));

		var decision = policy.Evaluate(ContextAt(DayOfWeek.Monday, 12));

		decision.Reason.Should().Be("Time-of-day scaling 0.25x");
	}

	[Fact]
	public void Evaluation_UsesTheConfiguredTimeZone_NotUtc() {
		// 02:00 UTC Saturday is 21:00 Friday in UTC-5 — a Friday-evening rule must match.
		var policy = Policy(o => o.Rules.Add(new() {
			Days = [DayOfWeek.Friday],
			StartHour = 16,
			EndHour = 23,
			ScalingFactor = 0.5
		}));

		var utcSaturdayEarly = new DateTimeOffset(2026, 7, 11, 2, 0, 0, TimeSpan.Zero); // Sat 02:00Z
		var decision = policy.Evaluate(new BatchingContext(utcSaturdayEarly, BaseWait, BaseCapacity, 0, 0.0, 0.0));

		decision.FillWaitTime.Should().Be(TimeSpan.FromMilliseconds(50));
	}

	[Fact]
	public void NullOptions_Throw() {
		var act = () => new TimeOfDayBatchingPolicy(null!);
		act.Should().Throw<ArgumentNullException>();
	}

	[Theory]
	[InlineData(0.0)]
	[InlineData(-1.0)]
	public void NonPositiveDefaultFactor_Throws(double factor) {
		var act = () => new TimeOfDayBatchingPolicy(new() { DefaultScalingFactor = factor });
		act.Should().Throw<ArgumentException>().WithMessage("*DefaultScalingFactor*");
	}

	[Fact]
	public void RuleWithoutDays_Throws() {
		var act = () => new TimeOfDayBatchingPolicy(new() {
			Rules = [new() { StartHour = 0, EndHour = 24, ScalingFactor = 0.5 }]
		});
		act.Should().Throw<ArgumentException>().WithMessage("*at least one day*");
	}

	[Theory]
	[InlineData(-1, 24)]
	[InlineData(24, 24)]
	[InlineData(0, 0)]
	[InlineData(0, 25)]
	public void OutOfRangeHours_Throw(int startHour, int endHour) {
		var act = () => new TimeOfDayBatchingPolicy(new() {
			Rules = [new() {
				Days = [DayOfWeek.Monday],
				StartHour = startHour,
				EndHour = endHour,
				ScalingFactor = 0.5
			}]
		});
		act.Should().Throw<ArgumentException>().WithMessage("*Hour*");
	}

	[Theory]
	[InlineData(0.0)]
	[InlineData(-0.5)]
	public void NonPositiveRuleFactor_Throws(double factor) {
		var act = () => new TimeOfDayBatchingPolicy(new() {
			Rules = [new() {
				Days = [DayOfWeek.Monday],
				StartHour = 0,
				EndHour = 24,
				ScalingFactor = factor
			}]
		});
		act.Should().Throw<ArgumentException>().WithMessage("*ScalingFactor*");
	}

}