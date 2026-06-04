namespace Cirreum.Messaging.Metrics;

/// <summary>
/// Collects and reports metrics for a distributed-messaging channel — queue depths,
/// batch sizes, delivery latency, error rates.
/// </summary>
/// <remarks>
/// Transport runtimes provide concrete implementations (typically backed by
/// OpenTelemetry meters). This surface defines the contract.
/// </remarks>
public interface IMessagingMetricsService : IDisposable {

	/// <summary>Records that a message was received for delivery.</summary>
	void RecordMessageReceived(string messageType, MessageTarget kind);

	/// <summary>Records that a message was queued for delivery.</summary>
	void RecordMessageQueued(string messageType, MessageTarget kind, long queueTimeMs, DistributedMessagePriority priority);

	/// <summary>Records that a message was dequeued for delivery.</summary>
	void RecordMessageDequeued(string messageType, MessageTarget kind, long queueWaitTimeMs, DistributedMessagePriority priority);

	/// <summary>Records that a message was successfully delivered.</summary>
	void RecordMessageDelivered(string messageType, MessageTarget kind, long processingTimeMs);

	/// <summary>Records that a message was successfully delivered as part of a batch.</summary>
	void RecordMessageDeliveredInBatch(string messageType, MessageTarget kind, long processingTimeMs, long totalTimeMs);

	/// <summary>Records that a message delivery failed.</summary>
	void RecordMessageFailed(string messageType, MessageTarget kind, string errorType, long processingTimeMs);

	/// <summary>Records that a partial batch was created.</summary>
	void RecordPartialBatch(int batchCapacity, int batchSize);

	/// <summary>Records information about a processed batch.</summary>
	void RecordBatchProcessed(
		int batchCapacity,
		int batchSize,
		long processingTimeMs,
		int successCount,
		int failureCount,
		int standardCount,
		int timeSensitiveCount,
		int systemCount);

	/// <summary>Records the current queue depth.</summary>
	Task RecordQueueDepth(int queueDepth);

}
