namespace Cirreum.Messaging;

using System.Text.Json;

/// <summary>
/// Wire-format envelope wrapping a serialized <see cref="DistributedMessage"/> payload
/// with metadata for cross-process delivery.
/// </summary>
/// <remarks>
/// Created by the channel's delivery engine at send time; reconstructed by the inbound
/// receiver on the other side. The envelope shape is the stable cross-version contract;
/// both the envelope and its inner payload are System.Text.Json with the channel's own
/// options, <b>decided internally</b> (see <see cref="SerializerOptions"/>) — callers do
/// not supply serialization options, because the wire must be symmetric across producer
/// and consumer. The family owns its wire format; a different format is a different
/// channel, not a per-call option. The receiver resolves the inner type by identity
/// through the registry (<c>ResolveType(MessageIdentifier, MessageVersion)</c>) and hands
/// it to <see cref="DeserializeMessage(Type)"/>; the envelope performs no type resolution
/// of its own.
/// </remarks>
public record DistributedMessageEnvelope {

	/// <summary>
	/// The channel's wire serialization options for both the envelope and its payload —
	/// the single, internally-owned source of truth so producer and consumer are always
	/// symmetric. Currently the System.Text.Json defaults; the one place to change the
	/// channel's on-the-wire JSON behavior should that ever be needed (a wire-format
	/// decision, kept off the public API deliberately).
	/// </summary>
	private static readonly JsonSerializerOptions SerializerOptions = new();

	/// <summary>
	/// Parameterless constructor for serialization.
	/// </summary>
	public DistributedMessageEnvelope() {
		this.SerializedMessage = string.Empty;
		this.MessageIdentifier = string.Empty;
		this.MessageVersion = string.Empty;
		this.MessageType = string.Empty;
		this.ProducerId = string.Empty;
	}

	private DistributedMessageEnvelope(
		string serializedMessage,
		string messageIdentifier,
		string messageVersion,
		string messageType,
		string producerId,
		DateTimeOffset publishedAt) {
		this.SerializedMessage = serializedMessage;
		this.MessageIdentifier = messageIdentifier;
		this.MessageVersion = messageVersion;
		this.MessageType = messageType;
		this.ProducerId = producerId;
		this.PublishedAt = publishedAt;
	}

	/// <summary>
	/// Creates a new envelope from a typed message. The inner payload is serialized with
	/// System.Text.Json — the channel's wire format.
	/// </summary>
	public static DistributedMessageEnvelope Create<TMessage>(
		TMessage message,
		MessageDefinition definition,
		string producerId)
		where TMessage : DistributedMessage =>
		new(
			JsonSerializer.Serialize(message, SerializerOptions),
			definition.Identifier,
			definition.Version,
			// Full name PLUS the simple assembly name — a plain full name is only
			// resolvable from this assembly or the core library. This value is diagnostic
			// metadata (see MessageType); it is deliberately not the full
			// AssemblyQualifiedName (no version/culture/token).
			$"{typeof(TMessage).FullName ?? typeof(TMessage).Name}, {typeof(TMessage).Assembly.GetName().Name}",
			producerId,
			DateTimeOffset.UtcNow);

	/// <summary>
	/// Deserializes a JSON envelope using the channel's own serialization options.
	/// </summary>
	public static DistributedMessageEnvelope FromJson(string json) =>
		JsonSerializer.Deserialize<DistributedMessageEnvelope>(json, SerializerOptions)
			?? throw new InvalidOperationException("Unable to deserialize envelope from JSON.");

	/// <summary>The serialized payload of the inner message.</summary>
	public string SerializedMessage { get; init; }

	/// <summary>The stable message identifier (from <see cref="MessageVersionAttribute.Identifier"/>).</summary>
	public string MessageIdentifier { get; init; }

	/// <summary>The schema version (from <see cref="MessageVersionAttribute.Version"/>).</summary>
	public string MessageVersion { get; init; }

	/// <summary>
	/// The assembly-hinted CLR type name of the inner message — <b>diagnostic metadata
	/// only</b>, never a resolution input. Inbound type resolution goes through the
	/// registry's identity map (<c>ResolveType(MessageIdentifier, MessageVersion)</c>),
	/// which only ever selects from the receiver's own vetted scan set; this name exists
	/// for logging and dead-letter triage, where it is the operator's best hint about
	/// what a producer actually sent.
	/// </summary>
	public string MessageType { get; init; }

	/// <summary>The id of the producer that created the envelope.</summary>
	public string ProducerId { get; init; }

	/// <summary>
	/// UTC timestamp stamped at envelope creation. Nullable for compatibility with
	/// envelopes serialized prior to the field's introduction.
	/// </summary>
	public DateTimeOffset? PublishedAt { get; init; }

	/// <summary>
	/// Deserializes the inner message to the given concrete type using System.Text.Json.
	/// </summary>
	/// <param name="messageType">The concrete message type — resolved by the caller,
	/// canonically via the registry's identity map
	/// (<c>ResolveType(MessageIdentifier, MessageVersion)</c>). The envelope performs no
	/// type resolution of its own.</param>
	public object DeserializeMessage(Type messageType) {
		ArgumentNullException.ThrowIfNull(messageType);
		return JsonSerializer.Deserialize(this.SerializedMessage, messageType, SerializerOptions)
			?? throw new InvalidOperationException("Unable to deserialize message payload.");
	}

	/// <summary>
	/// Deserializes the inner message to a known type <typeparamref name="T"/>.
	/// </summary>
	public T DeserializeMessage<T>() =>
		JsonSerializer.Deserialize<T>(this.SerializedMessage, SerializerOptions)
			?? throw new InvalidOperationException("Unable to deserialize message payload.");

}
