namespace Cirreum.Messaging.Distributed.Tests;

using Cirreum.Messaging;
using System.Text.Json;

public class DistributedMessageEnvelopeTests {

	private static readonly MessageDefinition QueueRoutedDefinition = new(
		"tests.queue-routed",
		"1.0",
		typeof(QueueRoutedTestMessage).AssemblyQualifiedName!,
		[]);

	[Fact]
	public void Create_CapturesDefinitionProducerAndTimestamp() {
		var before = DateTimeOffset.UtcNow;

		var envelope = DistributedMessageEnvelope.Create(
			new QueueRoutedTestMessage("hello"),
			QueueRoutedDefinition,
			"test-producer");

		envelope.MessageIdentifier.Should().Be("tests.queue-routed");
		envelope.MessageVersion.Should().Be("1.0");
		envelope.ProducerId.Should().Be("test-producer");
		envelope.PublishedAt.Should().NotBeNull()
			.And.BeOnOrAfter(before);
	}

	[Fact]
	public void Create_StampsAnAssemblyHintedTypeName_WithoutVersionDetails() {
		var envelope = DistributedMessageEnvelope.Create(
			new QueueRoutedTestMessage("hello"),
			QueueRoutedDefinition,
			"test-producer");

		envelope.MessageType.Should().Be(
			$"{typeof(QueueRoutedTestMessage).FullName}, {typeof(QueueRoutedTestMessage).Assembly.GetName().Name}");
		envelope.MessageType.Should().NotContain("Version=");
	}

	[Fact]
	public void ResolveMessageType_ResolvesCrossAssemblyTypes() {
		var envelope = DistributedMessageEnvelope.Create(
			new QueueRoutedTestMessage("hello"),
			QueueRoutedDefinition,
			"test-producer");

		envelope.ResolveMessageType().Should().Be<QueueRoutedTestMessage>();
	}

	[Fact]
	public void ResolveMessageType_ResolvesLegacyBareFullNames() {
		// Envelopes from pre-1.1.0 producers stamped a bare full name.
		var legacy = new DistributedMessageEnvelope {
			MessageType = typeof(QueueRoutedTestMessage).FullName!
		};

		legacy.ResolveMessageType().Should().Be<QueueRoutedTestMessage>();
	}

	[Fact]
	public void ResolveMessageType_UnknownType_ReturnsNull() {
		var envelope = new DistributedMessageEnvelope {
			MessageType = "No.Such.Namespace.NoSuchType, No.Such.Assembly"
		};

		envelope.ResolveMessageType().Should().BeNull();
	}

	[Fact]
	public void DeserializeMessage_Untyped_RestoresCrossAssemblyPayloads() {
		var envelope = DistributedMessageEnvelope.Create(
			new QueueRoutedTestMessage("untyped"),
			QueueRoutedDefinition,
			"test-producer");

		var message = envelope.DeserializeMessage();

		message.Should().BeOfType<QueueRoutedTestMessage>()
			.Which.Payload.Should().Be("untyped");
	}

	[Fact]
	public void Envelope_RoundTrips_ThroughJson() {
		var original = DistributedMessageEnvelope.Create(
			new QueueRoutedTestMessage("round-trip"),
			QueueRoutedDefinition,
			"test-producer");

		var roundTripped = DistributedMessageEnvelope.FromJson(JsonSerializer.Serialize(original));

		roundTripped.Should().Be(original);
	}

	[Fact]
	public void DeserializeMessage_Typed_RestoresThePayload() {
		var envelope = DistributedMessageEnvelope.Create(
			new QueueRoutedTestMessage("payload-42"),
			QueueRoutedDefinition,
			"test-producer");

		var message = envelope.DeserializeMessage<QueueRoutedTestMessage>();

		message.Payload.Should().Be("payload-42");
	}

	[Fact]
	public void FromJson_InvalidJson_Throws() {
		var act = () => DistributedMessageEnvelope.FromJson("not json");
		act.Should().Throw<JsonException>();
	}

}