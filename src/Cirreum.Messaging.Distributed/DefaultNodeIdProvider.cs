namespace Cirreum.Messaging;

/// <summary>
/// Default <see cref="INodeIdProvider"/> implementation that resolves the current
/// process's node identity via a chain of environment-based hints, falling back to
/// a generated GUID when no hint is available.
/// </summary>
/// <remarks>
/// Resolution chain (first non-empty value wins):
/// (1) <c>CONTAINER_APP_REPLICA_NAME</c> — Azure Container Apps replica name.
/// (2) <c>WEBSITE_INSTANCE_ID</c> — Azure App Service instance ID.
/// (3) <c>HOSTNAME</c> — container hostname (Kubernetes, generic container runtimes).
/// (4) <c>{MachineName}:{ProcessId}</c> — local development and unmanaged environments.
/// (5) <c>Guid.NewGuid()</c> truncated — last-resort fallback ensuring uniqueness.
/// </remarks>
public sealed class DefaultNodeIdProvider : INodeIdProvider {

	/// <summary>
	/// Resolves the node id once at construction and caches the result.
	/// </summary>
	public DefaultNodeIdProvider() {
		this.NodeId = Resolve();
	}

	/// <inheritdoc />
	public string NodeId { get; }

	private static string Resolve() {
		var containerAppReplica = Environment.GetEnvironmentVariable("CONTAINER_APP_REPLICA_NAME");
		if (!string.IsNullOrWhiteSpace(containerAppReplica)) {
			return containerAppReplica;
		}
		var appServiceInstance = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
		if (!string.IsNullOrWhiteSpace(appServiceInstance)) {
			return appServiceInstance;
		}
		var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
		if (!string.IsNullOrWhiteSpace(hostname)) {
			return hostname;
		}
		var machine = Environment.MachineName;
		if (!string.IsNullOrWhiteSpace(machine)) {
			return $"{machine}:{Environment.ProcessId}";
		}
		return Guid.NewGuid().ToString("N")[..8];
	}

}
