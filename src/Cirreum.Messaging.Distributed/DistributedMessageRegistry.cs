namespace Cirreum.Messaging;

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

/// <summary>
/// Concrete <see cref="IDistributedMessageRegistry"/> built on the Kernel's
/// <see cref="MessageRegistryBase{TBase}"/>. Adds caching of the
/// <see cref="MessageTarget"/> per discovered message type from the
/// <see cref="DistributedMessageTargetAttribute"/> when present.
/// </summary>
/// <remarks>
/// Runtime composition (typically <c>Cirreum.Runtime.Messaging</c> or app code) constructs
/// and initializes this registry by calling <see cref="InitializeAsync"/> during host
/// startup.
/// </remarks>
public sealed class DistributedMessageRegistry(
	ILogger<DistributedMessageRegistry> logger
) : MessageRegistryBase<DistributedMessage>(logger), IDistributedMessageRegistry {

	private readonly ConcurrentDictionary<string, MessageTarget> _targets = new();

	/// <summary>
	/// Performs the standard Kernel scan and additionally captures the
	/// <see cref="DistributedMessageTargetAttribute"/> for each discovered type.
	/// </summary>
	public async ValueTask InitializeAsync() {
		await this.DefaultInitializationAsync().ConfigureAwait(false);
		foreach (var definition in this.GetAll()) {
			var type = Type.GetType(definition.MessageType);
			if (type is null) {
				continue;
			}
			var attr = type.GetCustomAttribute<DistributedMessageTargetAttribute>();
			var target = attr?.Target ?? MessageTarget.Topic;
			this._targets.TryAdd(definition.MessageType, target);
		}
	}

	/// <inheritdoc/>
	public MessageTarget GetTargetFor<T>() where T : DistributedMessage =>
		this.GetTargetFor(typeof(T));

	/// <inheritdoc/>
	public MessageTarget GetTargetFor(Type messageType) {
		ArgumentNullException.ThrowIfNull(messageType);
		if (!typeof(DistributedMessage).IsAssignableFrom(messageType)) {
			throw new ArgumentException(
				$"Type {messageType.FullName} is not a {nameof(DistributedMessage)}.",
				nameof(messageType));
		}
		return this._targets.TryGetValue(messageType.FullName!, out var target)
			? target
			: MessageTarget.Topic;
	}

}
