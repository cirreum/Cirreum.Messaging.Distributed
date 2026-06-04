namespace Cirreum.Messaging;

/// <summary>
/// Distributed-channel registry — extends the Kernel's generic
/// <see cref="IMessageRegistry{TBase}"/> with within-channel target-routing lookup.
/// </summary>
/// <remarks>
/// Apps and infrastructure consult this registry to discover all
/// <see cref="DistributedMessage"/>-derived types in the loaded assemblies and to
/// resolve the routing target (queue vs topic) per type. Channels that aren't the
/// generic <see cref="DistributedMessage"/> channel define their own
/// <c>I{Channel}MessageRegistry : IMessageRegistry&lt;{TBase}&gt;</c> in their own
/// package — the Kernel's open-generic registry is the shared primitive.
/// </remarks>
public interface IDistributedMessageRegistry : IMessageRegistry<DistributedMessage> {

	/// <summary>
	/// Gets the <see cref="MessageTarget"/> for the given strongly-typed message.
	/// Defaults to <see cref="MessageTarget.Topic"/> when the type lacks a
	/// <see cref="DistributedMessageTargetAttribute"/>.
	/// </summary>
	MessageTarget GetTargetFor<T>() where T : DistributedMessage;

	/// <summary>
	/// Gets the <see cref="MessageTarget"/> for the given runtime <see cref="Type"/>.
	/// </summary>
	MessageTarget GetTargetFor(Type messageType);

}
