namespace Cirreum.Messaging.Distributed.Tests;

using Cirreum.Messaging;
using Microsoft.Extensions.Logging.Abstractions;

public class DistributedMessageRegistryTests {

	private static async Task<DistributedMessageRegistry> InitializedRegistry() {
		var registry = new DistributedMessageRegistry(
			NullLogger<DistributedMessageRegistry>.Instance);
		await registry.InitializeAsync();
		return registry;
	}

	[Fact]
	public async Task GetTargetFor_HonorsTheTargetAttribute_AcrossAssemblies() {
		var registry = await InitializedRegistry();

		registry.GetTargetFor<QueueRoutedTestMessage>().Should().Be(MessageTarget.Queue);
	}

	[Fact]
	public async Task GetTargetFor_DefaultsToTopic_WhenNoAttribute() {
		var registry = await InitializedRegistry();

		registry.GetTargetFor<UnroutedTestMessage>().Should().Be(MessageTarget.Topic);
	}

	[Fact]
	public async Task GetTargetFor_RuntimeTypeOverload_MatchesGeneric() {
		var registry = await InitializedRegistry();

		registry.GetTargetFor(typeof(QueueRoutedTestMessage)).Should().Be(MessageTarget.Queue);
	}

	[Fact]
	public async Task GetTargetFor_NonDistributedMessageType_Throws() {
		var registry = await InitializedRegistry();

		var act = () => registry.GetTargetFor(typeof(string));

		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public async Task GetDefinitionFor_ReturnsTheScannedDefinition() {
		var registry = await InitializedRegistry();

		var definition = registry.GetDefinitionFor<UnroutedTestMessage>();

		definition.Identifier.Should().Be("tests.unrouted");
		definition.Version.Should().Be("2.1");
		definition.MessageType.Should().Be(typeof(UnroutedTestMessage).FullName);
	}

}