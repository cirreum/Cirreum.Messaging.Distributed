namespace Cirreum.Messaging;

using System.Text.Json.Serialization;

/// <summary>
/// Within-channel routing hint for a distributed message.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageTarget {

	/// <summary>
	/// Deliver to a queue — point-to-point, single consumer processes the message.
	/// </summary>
	Queue = 0,

	/// <summary>
	/// Deliver to a topic — publish-subscribe, multiple subscribers each receive a copy.
	/// </summary>
	Topic = 1

}
