namespace Cirreum.Messaging;

using System.Text.Json;

/// <summary>
/// Wire-format envelope wrapping a serialized <see cref="DistributedMessage"/> payload
/// with metadata for cross-process delivery.
/// </summary>
/// <remarks>
/// Created by the outbound transport publisher at send time; reconstructed by the inbound
/// receiver on the other side. The envelope shape is the stable cross-version contract;
/// the inner serialized payload follows the message type's schema as captured by its
/// <see cref="MessageVersionAttribute"/> identifier+version pair.
/// </remarks>
public record DistributedMessageEnvelope {

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
	/// Creates a new envelope from a typed message, using <c>System.Text.Json</c>
	/// defaults for serialization.
	/// </summary>
	public static DistributedMessageEnvelope Create<TMessage>(
		TMessage message,
		MessageDefinition definition,
		string producerId)
		where TMessage : DistributedMessage =>
		CreateWithSerializer(
			message,
			definition,
			producerId,
			m => JsonSerializer.Serialize(m));

	/// <summary>
	/// Creates a new envelope from a typed message using a caller-supplied serializer.
	/// Use when the channel requires a non-default <see cref="JsonSerializerOptions"/>
	/// (camelCase, custom converters, etc.) or a non-JSON wire format.
	/// </summary>
	public static DistributedMessageEnvelope CreateWithSerializer<TMessage>(
		TMessage message,
		MessageDefinition definition,
		string producerId,
		Func<TMessage, string> serializer)
		where TMessage : DistributedMessage =>
		new(
			serializer(message),
			definition.Identifier,
			definition.Version,
			// Full name PLUS the simple assembly name — a plain full name is only
			// resolvable from this assembly or the core library, so receivers could
			// never re-materialize app-defined message types. Deliberately not the
			// full AssemblyQualifiedName: no version/culture/token, so assembly
			// version drift between producer and consumer doesn't break resolution.
			$"{typeof(TMessage).FullName ?? typeof(TMessage).Name}, {typeof(TMessage).Assembly.GetName().Name}",
			producerId,
			DateTimeOffset.UtcNow);

	/// <summary>
	/// Deserializes a JSON envelope.
	/// </summary>
	public static DistributedMessageEnvelope FromJson(string json) =>
		JsonSerializer.Deserialize<DistributedMessageEnvelope>(json)
			?? throw new InvalidOperationException("Unable to deserialize envelope from JSON.");

	/// <summary>
	/// Deserializes a JSON envelope with custom serializer options.
	/// </summary>
	public static DistributedMessageEnvelope FromJson(string json, JsonSerializerOptions options) =>
		JsonSerializer.Deserialize<DistributedMessageEnvelope>(json, options)
			?? throw new InvalidOperationException("Unable to deserialize envelope from JSON.");

	/// <summary>The serialized payload of the inner message.</summary>
	public string SerializedMessage { get; init; }

	/// <summary>The stable message identifier (from <see cref="MessageVersionAttribute.Identifier"/>).</summary>
	public string MessageIdentifier { get; init; }

	/// <summary>The schema version (from <see cref="MessageVersionAttribute.Version"/>).</summary>
	public string MessageVersion { get; init; }

	/// <summary>The fully qualified CLR type name of the inner message.</summary>
	public string MessageType { get; init; }

	/// <summary>The id of the producer that created the envelope.</summary>
	public string ProducerId { get; init; }

	/// <summary>
	/// UTC timestamp stamped at envelope creation. Nullable for compatibility with
	/// envelopes serialized prior to the field's introduction.
	/// </summary>
	public DateTimeOffset? PublishedAt { get; init; }

	/// <summary>
	/// Resolves the CLR type named by <see cref="MessageType"/>, or <see langword="null"/>
	/// when the type is not available in the current process.
	/// </summary>
	/// <remarks>
	/// Tries <see cref="Type.GetType(string)"/> first (handles the assembly-hinted format
	/// this envelope stamps), then falls back to searching the loaded assemblies by full
	/// name — which also resolves envelopes from older producers that stamped a bare
	/// full name.
	/// </remarks>
	public Type? ResolveMessageType() {

		if (string.IsNullOrEmpty(this.MessageType)) {
			return null;
		}

		var type = Type.GetType(this.MessageType);
		if (type is not null) {
			return type;
		}

		// Legacy envelopes carry a bare full name; strip any assembly hint and probe
		// the loaded assemblies directly.
		var fullName = this.MessageType.Split(',')[0].Trim();
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
			if (assembly.IsDynamic) {
				continue;
			}
			type = assembly.GetType(fullName);
			if (type is not null) {
				return type;
			}
		}

		return null;
	}

	/// <summary>
	/// Deserializes the inner message using the captured <see cref="MessageType"/> and
	/// default JSON options.
	/// </summary>
	public object DeserializeMessage() {
		var type = this.ResolveMessageType()
			?? throw new InvalidOperationException($"Could not resolve type '{this.MessageType}'.");
		return JsonSerializer.Deserialize(this.SerializedMessage, type)
			?? throw new InvalidOperationException("Unable to deserialize message payload.");
	}

	/// <summary>
	/// Deserializes the inner message using a caller-supplied deserializer.
	/// </summary>
	public object DeserializeMessage(Func<Type, string, object> deserializer) {
		var type = this.ResolveMessageType()
			?? throw new InvalidOperationException($"Could not resolve type '{this.MessageType}'.");
		return deserializer(type, this.SerializedMessage);
	}

	/// <summary>
	/// Deserializes the inner message to a known type <typeparamref name="T"/>.
	/// </summary>
	public T DeserializeMessage<T>() =>
		JsonSerializer.Deserialize<T>(this.SerializedMessage)
			?? throw new InvalidOperationException("Unable to deserialize message payload.");

	/// <summary>
	/// Deserializes the inner message using a caller-supplied deserializer.
	/// </summary>
	public T DeserializeMessage<T>(Func<string, T> deserializer) =>
		deserializer(this.SerializedMessage);

}
