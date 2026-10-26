namespace Cirreum.Messaging.Distributed.Tests;

using Cirreum.Messaging;
using System.Text.Json;

public class DistributedMessageSubjectTests {

	private static readonly MessageDefinition QueueRoutedDefinition = new(
		"tests.queue-routed",
		"1.0",
		typeof(QueueRoutedTestMessage).AssemblyQualifiedName!,
		[]);

	[Fact]
	public void Create_WithoutSubject_LeavesSubjectNull() {
		var envelope = DistributedMessageEnvelope.Create(
			new QueueRoutedTestMessage("hello"),
			QueueRoutedDefinition,
			"test-producer");

		envelope.Subject.Should().BeNull();
	}

	[Fact]
	public void Subject_RoundTripsThroughJson() {
		var subject = new DistributedMessageSubject("u-1", "Ada", "Cirreum.Entra", ["Admin", "Editor"]);
		var envelope = DistributedMessageEnvelope.Create(
			new QueueRoutedTestMessage("hello"),
			QueueRoutedDefinition,
			"test-producer",
			subject);

		var restored = DistributedMessageEnvelope.FromJson(JsonSerializer.Serialize(envelope));

		restored.Subject.Should().NotBeNull();
		restored.Subject!.Id.Should().Be("u-1");
		restored.Subject.Name.Should().Be("Ada");
		restored.Subject.Scheme.Should().Be("Cirreum.Entra");
		restored.Subject.Roles.Should().Equal("Admin", "Editor");
	}

	[Fact]
	public void FromJson_WithoutSubjectField_ReadsNull() {
		var envelope = DistributedMessageEnvelope.Create(
			new QueueRoutedTestMessage("hello"),
			QueueRoutedDefinition,
			"test-producer");
		var json = JsonSerializer.Serialize(envelope);
		json.Should().Contain("\"Subject\":null");
		var legacy = json.Replace(",\"Subject\":null", "");

		var restored = DistributedMessageEnvelope.FromJson(legacy);

		restored.Subject.Should().BeNull();
		restored.ProducerId.Should().Be("test-producer");
	}

}
