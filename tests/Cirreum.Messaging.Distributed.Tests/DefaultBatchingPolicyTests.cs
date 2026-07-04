namespace Cirreum.Messaging.Distributed.Tests;

using Cirreum.Messaging.Batching;

public class DefaultBatchingPolicyTests {

	[Fact]
	public void Evaluate_PassesBaseValuesThroughUnchanged() {
		var policy = new DefaultBatchingPolicy();
		var context = new BatchingContext(
			DateTimeOffset.UtcNow,
			TimeSpan.FromMilliseconds(75),
			25,
			CurrentQueueDepth: 500,
			RecentSendRatePerSecond: 123.4,
			RecentErrorRate: 0.9);

		var decision = policy.Evaluate(context);

		decision.FillWaitTime.Should().Be(TimeSpan.FromMilliseconds(75));
		decision.BatchCapacity.Should().Be(25);
		decision.Reason.Should().BeNull();
	}

}