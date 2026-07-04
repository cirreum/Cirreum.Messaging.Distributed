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
	/// <remarks>
	/// The target map is built from a direct assembly-scan pass rather than
	/// <see cref="Type.GetType(string)"/> over the captured type names —
	/// <c>Type.GetType</c> with a plain full name only resolves types in this assembly
	/// or the core library, so message types defined in app assemblies would silently
	/// fall back to <see cref="MessageTarget.Topic"/>.
	/// </remarks>
	public async ValueTask InitializeAsync() {
		await this.DefaultInitializationAsync().ConfigureAwait(false);
		foreach (var type in AssemblyScanner.ScanExportedTypes(IsConcreteDistributedMessage)) {
			var attr = type.GetCustomAttribute<DistributedMessageTargetAttribute>();
			var target = attr?.Target ?? MessageTarget.Topic;
			this._targets.TryAdd(type.FullName!, target);
		}
	}

	private static bool IsConcreteDistributedMessage(Type type) =>
		type.IsClass
		&& !type.IsAbstract
		&& typeof(DistributedMessage).IsAssignableFrom(type);

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
