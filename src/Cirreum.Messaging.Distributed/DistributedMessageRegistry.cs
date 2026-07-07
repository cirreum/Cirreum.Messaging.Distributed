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
/// startup. The routing-target map is captured per discovery through the base scan's
/// <see cref="MessageRegistryBase{TBase}.OnMessageDiscovered"/> hook — one assembly
/// enumeration populates definitions, identity resolution, and routing together.
/// </remarks>
public sealed class DistributedMessageRegistry(
	ILogger<DistributedMessageRegistry> logger
) : MessageRegistryBase<DistributedMessage>(logger), IDistributedMessageRegistry {

	private readonly ConcurrentDictionary<string, MessageTarget> _targets = new();

	/// <summary>
	/// Performs the standard Kernel scan; the routing-target map is captured per
	/// discovery via <see cref="OnMessageDiscovered"/>.
	/// </summary>
	public ValueTask InitializeAsync() => this.DefaultInitializationAsync();

	/// <inheritdoc/>
	protected override void OnMessageDiscovered(MessageDiscovery discovery) {
		var attr = discovery.ClrType.GetCustomAttribute<DistributedMessageTargetAttribute>();
		this._targets.TryAdd(discovery.Definition.MessageType, attr?.Target ?? MessageTarget.Topic);
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
