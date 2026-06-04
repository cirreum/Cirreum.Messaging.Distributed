namespace Cirreum.Messaging;

/// <summary>
/// Provides a stable identifier for the current process replica/node for use in
/// distributed message echo prevention and audit.
/// </summary>
/// <remarks>
/// <para>
/// Distinct from <see cref="DistributedMessageEnvelope.ProducerId"/>, which identifies
/// the application/head and is shared across all replicas of the same deployed head.
/// <c>NodeId</c> uniquely identifies a single running process — every replica of every
/// head has a different <c>NodeId</c>.
/// </para>
/// <para>
/// The distinction matters for echo prevention in multi-replica deployments. When a
/// replica publishes a distributed message, the broker's subscription delivers the
/// message back to that head's subscription — including to the replica that originated
/// the publish. The originating replica has already applied the change locally and must
/// skip the redelivered echo; other replicas of the same head (different <c>NodeId</c>,
/// same <c>ProducerId</c>) must process it to converge their local state.
/// </para>
/// <para>
/// The default runtime implementation resolves <see cref="NodeId"/> via a chain of environment
/// hints (Container Apps replica name, App Service instance ID, container hostname, machine
/// name + PID) with a generated GUID fallback. Apps that need deterministic identifiers
/// (e.g., for integration tests) register a custom implementation.
/// </para>
/// </remarks>
public interface INodeIdProvider {

	/// <summary>
	/// Gets the unique identifier for the current process replica.
	/// Stable for the lifetime of the process; not stable across restarts.
	/// </summary>
	string NodeId { get; }

}
